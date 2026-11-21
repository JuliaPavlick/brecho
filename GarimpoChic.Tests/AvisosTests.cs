using System.Net;
using System.Text;
using System.Text.Json;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Avisos por e-mail para clientes e vendedoras
public class AvisosTests
{
    // E-mails das contas de demonstração (Data/DadosIniciais.cs)
    const string EmailMariana = "mariana.souza@email.com";
    const string EmailAna = "anaclara@email.com";
    const string EmailCamila = "camila.rocha@email.com";

    static StringContent Json(object corpo) => new(JsonSerializer.Serialize(corpo), Encoding.UTF8, "application/json");

    static async Task<Pedido> MarianaCompra(SiteDeTeste site, string peca)
    {
        using var cliente = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);
        await cliente.Fetch($"/sacola/{(await site.Produto(peca)).Id}");
        await cliente.Enviar("/sacola/finalizar", new() { ["Entrega"] = "retirada" });
        return await site.Banco(db => db.Pedidos.OrderByDescending(p => p.Id).FirstAsync());
    }

    static async Task<Navegador> Funcionaria(SiteDeTeste site)
    {
        var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);
        return nav;
    }

    [Fact]
    public async Task Cliente_recebe_confirmacao_do_pedido()
    {
        using var site = new SiteDeTeste();

        var pedido = await MarianaCompra(site, "Bolsa Retrô");

        var email = Assert.Single(site.Emails.Para(EmailMariana));
        Assert.Contains($"Recebemos seu pedido {pedido.Numero}", email.Assunto);
        Assert.Contains("Bolsa Retrô", WebUtility.HtmlDecode(email.Html)); // acentos vão como entidades HTML
        Assert.Contains("/compras", email.Html);
    }

    [Theory]
    [InlineData(StatusPedido.Caminho, "está a caminho")]
    [InlineData(StatusPedido.Entregue, "foi entregue")]
    [InlineData(StatusPedido.Cancelado, "foi cancelado")]
    public async Task Cliente_e_avisada_quando_o_pedido_muda(string status, string trecho)
    {
        using var site = new SiteDeTeste();
        var pedido = await MarianaCompra(site, "Bolsa Retrô");
        using var funcionaria = await Funcionaria(site);

        await funcionaria.Fetch($"/painel/pedidos/{pedido.Id}/status", Json(new { status }));

        Assert.Contains(site.Emails.Para(EmailMariana), e => e.Assunto.Contains(trecho) && e.Assunto.Contains(pedido.Numero));
    }

    [Fact]
    public async Task Salvar_o_mesmo_status_nao_manda_aviso_de_novo()
    {
        using var site = new SiteDeTeste();
        var pedido = await MarianaCompra(site, "Bolsa Retrô");
        using var funcionaria = await Funcionaria(site);

        await funcionaria.Fetch($"/painel/pedidos/{pedido.Id}/status", Json(new { status = StatusPedido.Preparo }));

        Assert.Single(site.Emails.Para(EmailMariana)); // só a confirmação da compra
    }

    [Fact]
    public async Task Vendedora_sabe_quanto_vai_receber_quando_a_peca_e_entregue()
    {
        using var site = new SiteDeTeste();
        var pedido = await MarianaCompra(site, "Casaco de Lã"); // peça da Ana
        using var funcionaria = await Funcionaria(site);

        await funcionaria.Fetch($"/painel/pedidos/{pedido.Id}/status", Json(new { status = StatusPedido.Entregue }));

        var email = Assert.Single(site.Emails.Para(EmailAna));
        Assert.Contains("Casaco de Lã", email.Assunto);
        var valor = await site.Banco(db => db.PedidoItens.Where(i => i.PedidoId == pedido.Id).Select(i => i.Preco).SingleAsync());
        Assert.Contains(WebUtility.HtmlEncode(GarimpoChic.Util.Formato.Moeda(GarimpoChic.Util.Comissao.Valor(valor, 50m))), email.Html);
    }

    [Fact]
    public async Task Vendedora_nao_recebe_aviso_antes_da_entrega()
    {
        using var site = new SiteDeTeste();
        var pedido = await MarianaCompra(site, "Casaco de Lã");
        using var funcionaria = await Funcionaria(site);

        await funcionaria.Fetch($"/painel/pedidos/{pedido.Id}/status", Json(new { status = StatusPedido.Caminho }));

        Assert.Empty(site.Emails.Para(EmailAna));
    }

    [Fact]
    public async Task Vendedora_e_avisada_quando_a_peca_e_aprovada()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = await Funcionaria(site);
        var jaqueta = await site.Produto("Jaqueta Jeans Oversized"); // da Camila, em avaliação

        await funcionaria.Fetch($"/painel/produtos/{jaqueta.Id}", Navegador.Multipart(new()
        {
            ["Nome"] = jaqueta.Nome, ["Descricao"] = jaqueta.Descricao, ["Categoria"] = jaqueta.Categoria,
            ["Preco"] = "149.90", ["Status"] = StatusProduto.Disponivel,
        }));

        var email = Assert.Single(site.Emails.Para(EmailCamila));
        Assert.Contains("está à venda", email.Assunto);
        Assert.Contains($"/peca/{jaqueta.Id}", email.Html);
    }

    [Fact]
    public async Task Editar_peca_ja_a_venda_nao_manda_aviso()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = await Funcionaria(site);
        var casaco = await site.Produto("Casaco de Lã");

        await funcionaria.Fetch($"/painel/produtos/{casaco.Id}", Navegador.Multipart(new()
        {
            ["Nome"] = casaco.Nome, ["Descricao"] = "Nova descrição", ["Categoria"] = casaco.Categoria,
            ["Preco"] = "199.90", ["Status"] = StatusProduto.Disponivel,
        }));

        Assert.Empty(site.Emails.Para(EmailAna));
    }

    [Fact]
    public async Task Vendedora_e_avisada_quando_a_peca_e_recusada()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = await Funcionaria(site);
        var jaqueta = await site.Produto("Jaqueta Jeans Oversized");

        await funcionaria.Fetch($"/painel/produtos/{jaqueta.Id}/excluir");

        var email = Assert.Single(site.Emails.Para(EmailCamila));
        Assert.Contains("Jaqueta Jeans Oversized", email.Assunto);
        Assert.Contains("não vai entrar à venda", email.Html);
    }

    [Fact]
    public async Task Vendedora_e_avisada_do_repasse()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = await Funcionaria(site);
        var ana = await site.Banco(async db => (await db.Usuarios.ToListAsync()).Single(u => u.Nome == "Ana Clara").Id);

        await funcionaria.Fetch("/painel/financeiro/repasses", Json(new { vendedoraId = ana, valor = "42,50" }));

        var email = Assert.Single(site.Emails.Para(EmailAna));
        Assert.Contains("42,50", email.Assunto);
    }

    [Fact]
    public async Task Nome_da_peca_vai_escapado_no_email()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = await Funcionaria(site);
        var jaqueta = await site.Produto("Jaqueta Jeans Oversized");

        await funcionaria.Fetch($"/painel/produtos/{jaqueta.Id}", Navegador.Multipart(new()
        {
            ["Nome"] = "<script>alert(1)</script>", ["Categoria"] = "Roupas", ["Preco"] = "10", ["Status"] = StatusProduto.Disponivel,
        }));

        var email = Assert.Single(site.Emails.Para(EmailCamila));
        Assert.DoesNotContain("<script>", email.Html);
        Assert.Contains("&lt;script&gt;", email.Html);
    }

    [Fact]
    public async Task Conta_sem_email_nao_recebe_aviso()
    {
        using var site = new SiteDeTeste();
        await site.Banco(db => db.Usuarios.Where(u => u.Nome == "Mariana Souza")
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Email, "")));

        await MarianaCompra(site, "Bolsa Retrô");

        Assert.Empty(site.Emails.Todos);
    }
}
