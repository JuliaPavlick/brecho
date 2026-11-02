using GarimpoChic.Data;
using GarimpoChic.Models;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Util;

// Números do painel e do financeiro, calculados a partir dos pedidos e repasses reais
public class Relatorios
{
    public record Venda(DateTime Data, decimal Valor, int Pecas);

    public record Serie(string[] Labels, decimal[] Valores);

    public List<Venda> Vendas { get; private init; } = [];
    public List<Repasse> Repasses { get; private init; } = [];

    // Os totais são somados em memória porque o SQLite (usado no desenvolvimento) não soma decimal no banco
    public static async Task<Relatorios> CarregarAsync(BrechoContext db)
    {
        var desde = DateTime.UtcNow.AddDays(-400);

        // Pedidos cancelados não são venda
        var pedidos = await db.Pedidos.Include(p => p.Itens)
            .Where(p => p.Data >= desde && p.Status != StatusPedido.Cancelado).ToListAsync();
        var repasses = await db.Repasses.Include(r => r.Vendedora).Where(r => r.Data >= desde).ToListAsync();

        return new Relatorios
        {
            Vendas = pedidos.Select(p => new Venda(Formato.Local(p.Data), p.Total, p.Itens.Count)).ToList(),
            Repasses = repasses,
        };
    }

    static DateTime InicioMes(DateTime d) => new(d.Year, d.Month, 1);

    IEnumerable<Venda> VendasEntre(DateTime de, DateTime ate) => Vendas.Where(v => v.Data >= de && v.Data < ate);

    public decimal ReceitaMes(int mesesAtras = 0)
    {
        var inicio = InicioMes(Formato.Agora).AddMonths(-mesesAtras);
        return VendasEntre(inicio, inicio.AddMonths(1)).Sum(v => v.Valor);
    }

    public int PecasMes(int mesesAtras = 0)
    {
        var inicio = InicioMes(Formato.Agora).AddMonths(-mesesAtras);
        return VendasEntre(inicio, inicio.AddMonths(1)).Sum(v => v.Pecas);
    }

    public decimal RepassesMes()
    {
        var inicio = InicioMes(Formato.Agora);
        return Repasses.Where(r => Formato.Local(r.Data) >= inicio).Sum(r => r.Valor);
    }

    public decimal LucroMes => ReceitaMes() - RepassesMes();

    public decimal TicketMedio => PecasMes() == 0 ? 0 : ReceitaMes() / PecasMes();

    // Variação de peças vendidas em relação ao mês passado, ex.: "+18%"
    // Compara com o mesmo trecho do mês passado (do dia 1 até o mesmo dia); senão, no começo do mês daria "-95%"
    public string VariacaoPecas()
    {
        var agora = Formato.Agora;
        var inicio = InicioMes(agora);
        var inicioAntes = inicio.AddMonths(-1);
        var fimAntes = inicioAntes.Add(agora - inicio);

        var antes = VendasEntre(inicioAntes, fimAntes).Sum(v => v.Pecas);
        if (antes == 0) return "Primeiro mês com vendas";
        var pct = (VendasEntre(inicio, agora).Sum(v => v.Pecas) - antes) * 100.0 / antes;
        return $"{(pct >= 0 ? "+" : "")}{pct:0}% em relação ao mesmo período do mês passado";
    }

    // Dados do gráfico "Movimento de Vendas" nos três períodos
    public Dictionary<string, Serie> Grafico()
    {
        var hoje = Formato.Agora.Date;
        var pt = Formato.PtBr.DateTimeFormat;

        var dias = Enumerable.Range(0, 7).Select(i => hoje.AddDays(i - 6)).ToArray();
        var semana = new Serie(
            dias.Select(d => Capitalizar(pt.GetAbbreviatedDayName(d.DayOfWeek).TrimEnd('.'))).ToArray(),
            dias.Select(d => VendasEntre(d, d.AddDays(1)).Sum(v => v.Valor)).ToArray());

        var semanas = Enumerable.Range(0, 4).Select(i => hoje.AddDays(-27 + i * 7)).ToArray();
        var mes = new Serie(
            semanas.Select((_, i) => $"Semana {i + 1}").ToArray(),
            semanas.Select(s => VendasEntre(s, s.AddDays(7)).Sum(v => v.Valor)).ToArray());

        var meses = Enumerable.Range(1, hoje.Month).Select(m => new DateTime(hoje.Year, m, 1)).ToArray();
        var ano = new Serie(
            meses.Select(m => Capitalizar(pt.GetAbbreviatedMonthName(m.Month).TrimEnd('.'))).ToArray(),
            meses.Select(m => VendasEntre(m, m.AddMonths(1)).Sum(v => v.Valor)).ToArray());

        return new() { ["semana"] = semana, ["mes"] = mes, ["ano"] = ano };
    }

    // Valor curto para os cards, ex.: "R$ 12,3k"
    public static string MoedaCurta(decimal valor) =>
        valor >= 1000 ? "R$ " + (valor / 1000).ToString("0.#", Formato.PtBr) + "k" : Formato.Moeda(valor);

    static string Capitalizar(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
}
