using System.Net;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Quem pode ver e fazer o quê
public class PermissoesTests
{
    [Theory]
    [InlineData("/explorar")]
    [InlineData("/privacidade")]
    [InlineData("/cadastro")]
    public async Task Visitante_abre_as_paginas_publicas(string url)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        Assert.Equal(HttpStatusCode.OK, (await nav.Abrir(url)).Resposta.StatusCode);
    }

    [Fact]
    public async Task Visitante_ve_a_pagina_de_uma_peca_publicada()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        var peca = await site.Produto("Bolsa Retrô");

        var (r, html) = await nav.Abrir($"/peca/{peca.Id}");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("Bolsa Retrô", html);
        Assert.Contains("Entrar para comprar", html);
    }

    [Theory]
    [InlineData("/inicio")]
    [InlineData("/sacola")]
    [InlineData("/compras")]
    [InlineData("/perfil")]
    [InlineData("/painel")]
    public async Task Visitante_e_mandado_para_o_login(string url)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (r, _) = await nav.Abrir(url);

        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.StartsWith("/entrar?ReturnUrl=", r.Destino());
    }

    [Fact]
    public async Task Chamada_fetch_sem_login_recebe_401_para_a_tela_levar_ao_login()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Fetch($"/favoritos/{(await site.Produto("Bolsa Retrô")).Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Peca_em_avaliacao_nao_aparece_para_clientes()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        var emAvaliacao = await site.Produto("Jaqueta Jeans Oversized");

        var (r, _) = await nav.Abrir($"/peca/{emAvaliacao.Id}");
        var (_, catalogo) = await nav.Abrir("/explorar");

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.DoesNotContain("Jaqueta Jeans Oversized", catalogo);
    }

    [Fact]
    public async Task Foto_de_peca_em_avaliacao_so_aparece_para_a_loja()
    {
        using var site = new SiteDeTeste();
        using var visitante = site.Navegador();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var jaqueta = await site.Produto("Jaqueta Jeans Oversized");
        var fotoId = await site.Banco(db => db.ProdutoFotos.Where(f => f.ProdutoId == jaqueta.Id).Select(f => f.Id).FirstAsync());

        Assert.Equal(HttpStatusCode.NotFound, (await visitante.Abrir($"/fotos/{fotoId}")).Resposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await funcionaria.Abrir($"/fotos/{fotoId}")).Resposta.StatusCode);
    }

    [Theory]
    [InlineData("/painel")]
    [InlineData("/painel/clientes")]
    [InlineData("/painel/backup")]
    [InlineData("/painel/backup/baixar")]
    public async Task Cliente_nao_entra_no_painel(string url)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);

        var (r, _) = await nav.Abrir(url);

        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.StartsWith("/perfis", r.Destino());
    }

    [Fact]
    public async Task Cliente_nao_consegue_excluir_peca_pelo_painel()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        var peca = await site.Produto("Bolsa Retrô");

        await nav.Fetch($"/painel/produtos/{peca.Id}/excluir");

        Assert.True(await site.Banco(db => db.Produtos.AnyAsync(p => p.Id == peca.Id)));
    }

    [Fact]
    public async Task Funcionaria_entra_no_painel()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        foreach (var url in new[] { "/painel", "/painel/produtos", "/painel/pedidos", "/painel/clientes", "/painel/financeiro" })
            Assert.Equal(HttpStatusCode.OK, (await nav.Abrir(url)).Resposta.StatusCode);
    }

    [Fact]
    public async Task Funcionaria_que_perde_o_acesso_sai_do_painel_na_hora()
    {
        using var site = new SiteDeTeste();
        using var julia = site.Navegador();
        await julia.Entrar(SiteDeTeste.CpfJulia);
        Assert.Equal(HttpStatusCode.OK, (await julia.Abrir("/painel")).Resposta.StatusCode);

        await site.Banco(db => db.Usuarios.Where(u => u.EhFuncionaria)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.EhFuncionaria, false)));

        Assert.Equal(HttpStatusCode.Redirect, (await julia.Abrir("/painel")).Resposta.StatusCode);
    }

    // Conta de demonstração com os três papéis, para testar tudo com um login só
    [Fact]
    public async Task Admin_de_teste_entra_na_loja_no_painel_e_no_vender()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Entrar("000.000.001-91");

        Assert.Equal("/perfis", r.Destino());
        foreach (var url in new[] { "/inicio", "/vender", "/painel", "/painel/financeiro", "/painel/clientes" })
            Assert.Equal(HttpStatusCode.OK, (await nav.Abrir(url)).Resposta.StatusCode);
    }

    [Fact]
    public async Task Vendedora_tem_limite_de_pecas_esperando_avaliacao()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfAna);
        await nav.Abrir("/vender");
        async Task<int> EmAvaliacao() =>
            await site.Banco(db => db.Produtos.CountAsync(p => p.Status == StatusProduto.Avaliacao && p.Vendedora!.Nome == "Ana Clara"));

        var envio = new Dictionary<string, string> { ["nome"] = "Peça de teste", ["categoria"] = "Roupas", ["preco"] = "50,00" };
        while (await EmAvaliacao() < 20) await nav.Enviar("/vender", envio);
        await nav.Enviar("/vender", envio);

        Assert.Equal(20, await EmAvaliacao());
    }
}
