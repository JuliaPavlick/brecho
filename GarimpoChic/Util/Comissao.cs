using System.Globalization;
using GarimpoChic.Data;
using GarimpoChic.Models;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Util;

// Comissão das vendedoras externas: a vendedora recebe um percentual do preço de cada peça dela que foi vendida.
//   - O percentual é um só para a loja e a funcionária muda no Financeiro (padrão: 50%).
//   - Cada item vendido guarda o percentual do dia da venda (PedidoItem.PercentualVendedora).
//   - O valor só passa a ser devido quando o pedido é entregue; cancelado não conta.
//   - Saldo = devido − repasses já registrados.
public static class Comissao
{
    const string ChavePercentual = "PercentualVendedora";
    public const decimal PercentualPadrao = 50m;

    public static decimal Valor(decimal preco, decimal percentual) =>
        Math.Round(preco * percentual / 100m, 2, MidpointRounding.AwayFromZero);

    public static async Task<decimal> PercentualAsync(BrechoContext db)
    {
        var c = await db.Configuracoes.AsNoTracking().FirstOrDefaultAsync(x => x.Chave == ChavePercentual);
        return c != null && decimal.TryParse(c.Valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : PercentualPadrao;
    }

    public static async Task DefinirPercentualAsync(BrechoContext db, decimal percentual)
    {
        var c = await db.Configuracoes.FindAsync(ChavePercentual);
        if (c == null) db.Configuracoes.Add(c = new Configuracao { Chave = ChavePercentual });
        c.Valor = percentual.ToString(CultureInfo.InvariantCulture);
        await db.SaveChangesAsync();
    }

    // Vendas feitas antes desta funcionalidade: liga cada item à vendedora da peça, com o percentual atual
    public static async Task PreencherVendasAntigasAsync(BrechoContext db)
    {
        var itens = await db.PedidoItens
            .Where(i => i.VendedoraId == null && i.ProdutoId != null && i.Produto!.VendedoraId != null)
            .Select(i => new { Item = i, i.Produto!.VendedoraId })
            .ToListAsync();
        if (itens.Count == 0) return;

        var percentual = await PercentualAsync(db);
        foreach (var x in itens)
        {
            x.Item.VendedoraId = x.VendedoraId;
            x.Item.PercentualVendedora = percentual;
        }
        await db.SaveChangesAsync();
    }

    public record Extrato(int VendedoraId, string Nome, int PecasEntregues, decimal Devido, decimal Aguardando, decimal Pago)
    {
        // Positivo: a loja deve. Negativo: a loja pagou a mais (adiantamento).
        public decimal Saldo => Devido - Pago;
    }

    // Um extrato por vendedora (ou só o da vendedora indicada)
    public static async Task<List<Extrato>> ExtratosAsync(BrechoContext db, int? soDaVendedora = null)
    {
        var itens = await db.PedidoItens
            .Where(i => i.VendedoraId != null && i.Pedido!.Status != StatusPedido.Cancelado)
            .Where(i => soDaVendedora == null || i.VendedoraId == soDaVendedora)
            .Select(i => new { VendedoraId = i.VendedoraId!.Value, i.Preco, i.PercentualVendedora, i.Pedido!.Status })
            .ToListAsync();

        var repasses = await db.Repasses
            .Where(r => soDaVendedora == null || r.VendedoraId == soDaVendedora)
            .Select(r => new { r.VendedoraId, r.Valor })
            .ToListAsync();

        var comMovimento = itens.Select(i => i.VendedoraId).Concat(repasses.Select(r => r.VendedoraId)).ToHashSet();
        var vendedoras = await db.Usuarios
            .Where(u => soDaVendedora == null ? (u.EhVendedora && u.ExcluidaEm == null) || comMovimento.Contains(u.Id) : u.Id == soDaVendedora)
            .OrderBy(u => u.Nome)
            .Select(u => new { u.Id, u.Nome })
            .ToListAsync();

        return vendedoras.Select(v =>
        {
            var dela = itens.Where(i => i.VendedoraId == v.Id).ToList();
            var entregues = dela.Where(i => i.Status == StatusPedido.Entregue).ToList();
            decimal ValorDe(decimal preco, decimal? pct) => pct is decimal p ? Valor(preco, p) : 0;

            return new Extrato(
                v.Id,
                v.Nome,
                entregues.Count,
                entregues.Sum(i => ValorDe(i.Preco, i.PercentualVendedora)),
                dela.Where(i => i.Status != StatusPedido.Entregue).Sum(i => ValorDe(i.Preco, i.PercentualVendedora)),
                repasses.Where(r => r.VendedoraId == v.Id).Sum(r => r.Valor));
        }).ToList();
    }
}
