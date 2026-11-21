using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Escolha de entrega na sacola: retirar na loja ou receber em casa
public class EntregaTests
{
    static Dictionary<string, string> EmCasa(string cep = "89201-000") => new()
    {
        ["Entrega"] = "envio", ["Cep"] = cep, ["Endereco"] = "Rua das Flores", ["Numero"] = "120",
        ["Complemento"] = "apto 3", ["Bairro"] = "Centro", ["Cidade"] = "Joinville", ["Estado"] = "sc",
    };

    static async Task<Navegador> MarianaComBolsaNaSacola(SiteDeTeste site)
    {
        var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Fetch($"/sacola/{(await site.Produto("Bolsa Retrô")).Id}");
        await nav.Abrir("/sacola");
        return nav;
    }

    static Task<Pedido> UltimoPedido(SiteDeTeste site) =>
        site.Banco(db => db.Pedidos.AsNoTracking().OrderByDescending(p => p.Id).FirstAsync());

    [Fact]
    public async Task Receber_em_casa_guarda_o_endereco_no_pedido()
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);

        var r = await nav.Enviar("/sacola/finalizar", EmCasa("89201000"));

        Assert.Equal("/compras", r.Destino());
        var p = await UltimoPedido(site);
        Assert.Equal(TipoEntrega.Envio, p.Entrega);
        Assert.Equal("89201-000", p.Cep);
        Assert.Equal("120", p.NumeroEndereco);
        Assert.Equal("SC", p.Estado);
        Assert.Equal("Rua das Flores, 120 (apto 3) — Centro, Joinville/SC — CEP 89201-000", p.EnderecoCompleto);
    }

    [Fact]
    public async Task Retirar_na_loja_nao_guarda_endereco()
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);
        // Mesmo que o formulário mande campos de endereço (ficaram escondidos), retirada não guarda nada
        var campos = EmCasa();
        campos["Entrega"] = "retirada";

        await nav.Enviar("/sacola/finalizar", campos);

        var p = await UltimoPedido(site);
        Assert.Equal(TipoEntrega.Retirada, p.Entrega);
        Assert.Null(p.Cep);
        Assert.Null(p.Endereco);
        Assert.Equal("Retirar na loja", p.DescricaoEntrega);
    }

    [Theory]
    [InlineData("Entrega", "", "Escolha como você quer receber")]
    [InlineData("Cep", "123", "CEP válido")]
    [InlineData("Endereco", "", "Preencha rua, número, bairro e cidade")]
    [InlineData("Estado", "XX", "Escolha o estado")]
    public async Task Entrega_incompleta_volta_para_a_sacola_sem_criar_pedido(string campo, string valor, string mensagem)
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);
        var pedidosAntes = await site.Banco(db => db.Pedidos.CountAsync());
        var campos = EmCasa();
        campos[campo] = valor;

        var r = await nav.Enviar("/sacola/finalizar", campos);
        var html = WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains(mensagem, html);
        Assert.Equal(pedidosAntes, await site.Banco(db => db.Pedidos.CountAsync()));
        Assert.Equal(StatusProduto.Disponivel, (await site.Produto("Bolsa Retrô")).Status);
        if (campo != "Endereco") Assert.Contains("value=\"Rua das Flores\"", html); // o que foi digitado continua lá
    }

    [Fact]
    public async Task Proxima_compra_ja_vem_com_o_ultimo_endereco()
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);
        await nav.Enviar("/sacola/finalizar", EmCasa());
        await nav.Fetch($"/sacola/{(await site.Produto("Salto Clássico")).Id}");

        var (_, html) = await nav.Abrir("/sacola");

        Assert.Contains("value=\"Rua das Flores\"", html);
        Assert.Contains("value=\"89201-000\"", html);
        Assert.Matches("value=\"envio\" checked=\"checked\"", html);
        Assert.DoesNotContain("id=\"campos-endereco\" hidden", html);
    }

    [Fact]
    public async Task Primeira_compra_comeca_com_retirada_e_endereco_escondido()
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);

        var (_, html) = await nav.Abrir("/sacola");

        Assert.Matches("value=\"retirada\" checked=\"checked\"", html);
        Assert.Contains("id=\"campos-endereco\" hidden", html);
    }

    [Fact]
    public async Task Entrega_aparece_em_minhas_compras_no_painel_e_no_email()
    {
        using var site = new SiteDeTeste();
        using var cliente = await MarianaComBolsaNaSacola(site);
        await cliente.Enviar("/sacola/finalizar", EmCasa());
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        var (_, compras) = await cliente.Abrir("/compras");
        var (_, painel) = await funcionaria.Abrir("/painel/pedidos");
        var pedidos = Regex.Match(painel, @"let pedidos = (\[.*?\]);\s*\n", RegexOptions.Singleline).Groups[1].Value;
        var primeiro = JsonDocument.Parse(pedidos).RootElement[0];
        var email = site.Emails.Para("mariana.souza@email.com").Single();

        Assert.Contains("Receber em casa: Rua das Flores, 120", WebUtility.HtmlDecode(compras));
        Assert.StartsWith("Receber em casa: Rua das Flores, 120", primeiro.GetProperty("entrega").GetString());
        Assert.Contains("Rua das Flores", WebUtility.HtmlDecode(email.Html));
    }

    [Fact]
    public async Task Excluir_a_conta_apaga_o_endereco_dos_pedidos()
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);
        await nav.Enviar("/sacola/finalizar", EmCasa());
        await site.Banco(db => db.Pedidos.ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, StatusPedido.Entregue)));
        await nav.Abrir("/perfil");

        await nav.Enviar("/perfil/excluir", new() { ["senha"] = SiteDeTeste.Senha });

        var p = await UltimoPedido(site);
        Assert.Equal(TipoEntrega.Envio, p.Entrega); // o tipo fica (não identifica ninguém)
        Assert.Null(p.Endereco);
        Assert.Null(p.Cep);
        Assert.Null(p.NumeroEndereco);
        Assert.Null(p.Cidade);
    }

    [Fact]
    public async Task Baixar_meus_dados_inclui_o_endereco_dos_pedidos()
    {
        using var site = new SiteDeTeste();
        using var nav = await MarianaComBolsaNaSacola(site);
        await nav.Enviar("/sacola/finalizar", EmCasa());

        var (_, json) = await nav.Abrir("/perfil/meus-dados");

        Assert.Contains("Rua das Flores", json);
        Assert.Contains("89201-000", json);
    }
}
