using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Sacola e finalizar compra
public class CompraTests
{
    static async Task<JsonElement> Json(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;

    [Fact]
    public async Task Adicionar_e_tirar_da_sacola()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        var peca = await site.Produto("Bolsa Retrô");

        var colocou = await Json(await nav.Fetch($"/sacola/{peca.Id}"));
        var tirou = await Json(await nav.Fetch($"/sacola/{peca.Id}"));

        Assert.True(colocou.GetProperty("naSacola").GetBoolean());
        Assert.Equal(1, colocou.GetProperty("total").GetInt32());
        Assert.False(tirou.GetProperty("naSacola").GetBoolean());
        Assert.Equal(0, tirou.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task Finalizar_compra_cria_o_pedido_vende_as_pecas_e_esvazia_a_sacola()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        var bolsa = await site.Produto("Bolsa Retrô");
        var salto = await site.Produto("Salto Clássico");
        await nav.Fetch($"/sacola/{bolsa.Id}");
        await nav.Fetch($"/sacola/{salto.Id}");
        var pedidosAntes = await site.Banco(db => db.Pedidos.CountAsync());

        var r = await nav.Enviar("/sacola/finalizar");

        Assert.Equal("/compras", r.Destino());
        Assert.Equal(pedidosAntes + 1, await site.Banco(db => db.Pedidos.CountAsync()));
        var pedido = await site.Banco(db => db.Pedidos.Include(p => p.Itens).OrderByDescending(p => p.Id).FirstAsync());
        Assert.Equal(StatusPedido.Preparo, pedido.Status);
        Assert.Equal(bolsa.Preco + salto.Preco, pedido.Total);
        Assert.Equal(StatusProduto.Vendido, (await site.Produto("Bolsa Retrô")).Status);
        Assert.Equal(StatusProduto.Vendido, (await site.Produto("Salto Clássico")).Status);
        Assert.Equal(0, await site.Banco(db => db.Sacola.CountAsync()));
        Assert.Contains(pedido.Numero, (await nav.Abrir("/compras")).Html);
    }

    [Fact]
    public async Task Peca_vendida_para_outra_pessoa_nao_entra_no_pedido()
    {
        using var site = new SiteDeTeste();
        using var mariana = site.Navegador();
        using var ana = site.Navegador();
        await mariana.Entrar(SiteDeTeste.CpfMariana);
        await ana.Entrar(SiteDeTeste.CpfAna);
        var bolsa = await site.Produto("Bolsa Retrô");
        await mariana.Fetch($"/sacola/{bolsa.Id}");
        await ana.Fetch($"/sacola/{bolsa.Id}");

        await ana.Enviar("/sacola/finalizar");
        var pedidosAntes = await site.Banco(db => db.Pedidos.CountAsync());
        var r = await mariana.Enviar("/sacola/finalizar");

        Assert.Equal("/sacola", r.Destino());
        Assert.Equal(pedidosAntes, await site.Banco(db => db.Pedidos.CountAsync()));
    }

    [Fact]
    public async Task Peca_indisponivel_nao_entra_na_sacola()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        var reservada = await site.Produto("Lenço de Seda");

        var r = await nav.Fetch($"/sacola/{reservada.Id}");

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    // O site.js trata todo botão "btn-sacola" com data-id como "adicionar à sacola" (fetch).
    // O "Finalizar compra" usa o mesmo visual, mas não pode ter data-id, senão o clique vira fetch e o pedido não é enviado.
    [Fact]
    public async Task Botao_finalizar_compra_e_um_envio_de_formulario_e_nao_um_botao_de_peca()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Fetch($"/sacola/{(await site.Produto("Bolsa Retrô")).Id}");

        var (_, html) = await nav.Abrir("/sacola");
        var form = Regex.Match(html, """<form action="/sacola/finalizar"[\s\S]*?</form>""").Value;
        var botao = Regex.Match(form, """<button[^>]*>\s*Finalizar compra""").Value;

        Assert.Contains("type=\"submit\"", botao);
        Assert.DoesNotContain("data-id", botao);

        var js = await (await site.CreateClient().GetAsync("/js/site.js")).Content.ReadAsStringAsync();
        Assert.Contains("closest('button.btn-sacola[data-id]')", js);
    }

    [Fact]
    public async Task Cancelar_o_pedido_devolve_as_pecas_para_a_loja()
    {
        using var site = new SiteDeTeste();
        using var cliente = site.Navegador();
        using var funcionaria = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var bolsa = await site.Produto("Bolsa Retrô");
        await cliente.Fetch($"/sacola/{bolsa.Id}");
        await cliente.Enviar("/sacola/finalizar");
        var pedido = await site.Banco(db => db.Pedidos.OrderByDescending(p => p.Id).FirstAsync());

        var r = await funcionaria.Fetch($"/painel/pedidos/{pedido.Id}/status",
            JsonContent(new { status = StatusPedido.Cancelado }));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(StatusProduto.Disponivel, (await site.Produto("Bolsa Retrô")).Status);
    }

    [Fact]
    public async Task Pedido_entregue_nao_muda_mais_de_status()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var entregue = await site.Banco(db => db.Pedidos.FirstAsync(p => p.Status == StatusPedido.Entregue));

        var r = await funcionaria.Fetch($"/painel/pedidos/{entregue.Id}/status",
            JsonContent(new { status = StatusPedido.Cancelado }));

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    static StringContent JsonContent(object corpo) =>
        new(JsonSerializer.Serialize(corpo), System.Text.Encoding.UTF8, "application/json");
}
