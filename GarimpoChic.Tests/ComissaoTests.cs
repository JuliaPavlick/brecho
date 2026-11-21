using System.Net;
using System.Text;
using System.Text.Json;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using GarimpoChic.Util;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Comissão das vendedoras: percentual único (padrão 50%), devido quando o pedido é entregue
public class ComissaoTests
{
    static StringContent Json(object corpo) => new(JsonSerializer.Serialize(corpo), Encoding.UTF8, "application/json");

    static Task<Comissao.Extrato> ExtratoDaAna(SiteDeTeste site) =>
        site.Banco(async db => (await Comissao.ExtratosAsync(db)).Single(e => e.Nome == "Ana Clara"));

    // A Mariana compra o Casaco de Lã (peça da Ana) e devolve o pedido criado
    static async Task<Pedido> ComprarCasaco(SiteDeTeste site)
    {
        using var cliente = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);
        await cliente.Fetch($"/sacola/{(await site.Produto("Casaco de Lã")).Id}");
        await cliente.Enviar("/sacola/finalizar", new() { ["Entrega"] = "retirada" });
        return await site.Banco(db => db.Pedidos.Include(p => p.Itens).OrderByDescending(p => p.Id).FirstAsync());
    }

    static async Task MudarStatus(SiteDeTeste site, int pedidoId, string status)
    {
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var r = await funcionaria.Fetch($"/painel/pedidos/{pedidoId}/status", Json(new { status }));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Venda_de_peca_de_vendedora_guarda_a_vendedora_e_o_percentual_do_dia()
    {
        using var site = new SiteDeTeste();
        var ana = await site.Banco(async db => (await db.Usuarios.ToListAsync()).Single(u => u.Nome == "Ana Clara").Id);

        var pedido = await ComprarCasaco(site);

        var item = Assert.Single(pedido.Itens);
        Assert.Equal(ana, item.VendedoraId);
        Assert.Equal(50m, item.PercentualVendedora);
        Assert.Equal(Comissao.Valor(item.Preco, 50m), item.ValorVendedora);
    }

    [Fact]
    public async Task Peca_da_loja_nao_gera_comissao()
    {
        using var site = new SiteDeTeste();
        using var cliente = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);
        await cliente.Fetch($"/sacola/{(await site.Produto("Bolsa Retrô")).Id}");
        await cliente.Enviar("/sacola/finalizar", new() { ["Entrega"] = "retirada" });

        var item = await site.Banco(db => db.PedidoItens.OrderByDescending(i => i.Id).FirstAsync());

        Assert.Null(item.VendedoraId);
        Assert.Null(item.PercentualVendedora);
        Assert.Equal(0m, item.ValorVendedora);
    }

    [Fact]
    public async Task Comissao_so_e_devida_quando_o_pedido_e_entregue()
    {
        using var site = new SiteDeTeste();
        var antes = await ExtratoDaAna(site);

        var pedido = await ComprarCasaco(site);
        var valor = pedido.Itens.Single().ValorVendedora;
        var depoisDaCompra = await ExtratoDaAna(site);
        await MudarStatus(site, pedido.Id, StatusPedido.Entregue);
        var depoisDaEntrega = await ExtratoDaAna(site);

        Assert.Equal(antes.Devido, depoisDaCompra.Devido);
        Assert.Equal(antes.Aguardando + valor, depoisDaCompra.Aguardando);
        Assert.Equal(antes.Devido + valor, depoisDaEntrega.Devido);
        Assert.Equal(antes.PecasEntregues + 1, depoisDaEntrega.PecasEntregues);
        Assert.Equal(antes.Aguardando, depoisDaEntrega.Aguardando);
    }

    [Fact]
    public async Task Pedido_cancelado_nao_gera_comissao()
    {
        using var site = new SiteDeTeste();
        var antes = await ExtratoDaAna(site);

        var pedido = await ComprarCasaco(site);
        await MudarStatus(site, pedido.Id, StatusPedido.Cancelado);
        var depois = await ExtratoDaAna(site);

        Assert.Equal(antes.Devido, depois.Devido);
        Assert.Equal(antes.Aguardando, depois.Aguardando);
    }

    [Fact]
    public async Task Repasse_registrado_desconta_do_saldo()
    {
        using var site = new SiteDeTeste();
        var antes = await ExtratoDaAna(site);
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        var r = await funcionaria.Fetch("/painel/financeiro/repasses",
            Json(new { vendedoraId = antes.VendedoraId, valor = "10,00" }));
        var depois = await ExtratoDaAna(site);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(antes.Pago + 10m, depois.Pago);
        Assert.Equal(antes.Saldo - 10m, depois.Saldo);
    }

    [Fact]
    public async Task Mudar_o_percentual_vale_so_para_as_proximas_vendas()
    {
        using var site = new SiteDeTeste();
        var primeira = await ComprarCasaco(site);
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        var r = await funcionaria.Fetch("/painel/financeiro/comissao", Json(new { percentual = "60" }));
        // O casaco volta para a loja e é vendido de novo, já com 60%
        await MudarStatus(site, primeira.Id, StatusPedido.Cancelado);
        var segunda = await ComprarCasaco(site);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(60m, await site.Banco(Comissao.PercentualAsync));
        Assert.Equal(50m, await site.Banco(db => db.PedidoItens.Where(i => i.PedidoId == primeira.Id).Select(i => i.PercentualVendedora).SingleAsync()));
        Assert.Equal(60m, segunda.Itens.Single().PercentualVendedora);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("150")]
    [InlineData("abc")]
    public async Task Percentual_fora_de_1_a_100_e_recusado(string percentual)
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        var r = await funcionaria.Fetch("/painel/financeiro/comissao", Json(new { percentual }));

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal(Comissao.PercentualPadrao, await site.Banco(Comissao.PercentualAsync));
    }

    [Fact]
    public async Task Cliente_nao_muda_o_percentual()
    {
        using var site = new SiteDeTeste();
        using var cliente = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);

        await cliente.Fetch("/painel/financeiro/comissao", Json(new { percentual = "99" }));

        Assert.Equal(Comissao.PercentualPadrao, await site.Banco(Comissao.PercentualAsync));
    }

    [Fact]
    public async Task Vendas_antigas_de_demonstracao_ganham_vendedora_e_percentual_ao_iniciar()
    {
        using var site = new SiteDeTeste();

        var itensDoCasaco = await site.Banco(db => db.PedidoItens.Where(i => i.Nome == "Casaco de Lã").ToListAsync());

        Assert.NotEmpty(itensDoCasaco);
        Assert.All(itensDoCasaco, i =>
        {
            Assert.NotNull(i.VendedoraId);
            Assert.Equal(50m, i.PercentualVendedora);
        });
    }

    [Fact]
    public async Task Financeiro_mostra_as_comissoes_e_o_percentual()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var ana = await ExtratoDaAna(site);

        var (_, html) = await funcionaria.Abrir("/painel/financeiro");
        html = WebUtility.HtmlDecode(html);

        Assert.Contains("Comissões das vendedoras", html);
        Assert.Contains("id=\"percentual\"", html);
        Assert.Contains("value=\"50\"", html);
        Assert.Contains(Formato.Moeda(ana.Devido), html);
        Assert.Contains(Formato.Moeda(ana.Pago), html);
    }

    [Fact]
    public async Task Vendedora_ve_os_proprios_ganhos()
    {
        using var site = new SiteDeTeste();
        var pedido = await ComprarCasaco(site);
        await MudarStatus(site, pedido.Id, StatusPedido.Entregue);
        var ana = await ExtratoDaAna(site);
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfAna);

        var (_, html) = await nav.Abrir("/vender");
        html = WebUtility.HtmlDecode(html);

        Assert.Contains("Meus ganhos", html);
        Assert.Contains("você recebe 50%", html);
        Assert.Contains(Formato.Moeda(ana.Pago), html);
        Assert.Contains($"Peças suas já entregues: <strong>{ana.PecasEntregues}</strong>", html);
    }
}
