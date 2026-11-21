using System.Text.RegularExpressions;
using GarimpoChic.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GarimpoChic.Tests;

// Tema claro/escuro (wwwroot/css/tema.css): padrão de cada área, botão de troca e cores sempre vindas do tema
public class TemaTests
{
    [Theory]
    [InlineData("/explorar", "escuro")]
    [InlineData("/entrar", "escuro")]
    [InlineData("/cadastro", "escuro")]
    public async Task Loja_e_telas_de_conta_abrem_no_escuro(string url, string tema)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (_, html) = await nav.Abrir(url);

        Assert.Contains($"<html lang=\"pt-BR\" data-tema-padrao=\"{tema}\">", html);
    }

    [Fact]
    public async Task Painel_abre_no_claro()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir("/painel");

        Assert.Contains("<html lang=\"pt-BR\" data-tema-padrao=\"claro\">", html);
    }

    [Theory]
    [InlineData("/explorar")]
    [InlineData("/entrar")]
    public async Task Paginas_aplicam_o_tema_antes_de_desenhar_e_tem_o_botao(string url)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (_, html) = await nav.Abrir(url);

        // O script do tema vem antes de qualquer CSS, para a página não piscar no tema errado
        var script = html.IndexOf("localStorage.getItem('tema')", StringComparison.Ordinal);
        var css = html.IndexOf("/css/tema.css", StringComparison.Ordinal);
        Assert.True(script > 0 && css > script, "o script do tema precisa vir antes do tema.css");
        Assert.True(css < html.IndexOf("/css/site.css", StringComparison.Ordinal), "tema.css precisa vir antes do site.css");
        Assert.Contains("data-alternar-tema", html);
    }

    [Fact]
    public async Task Painel_tem_o_botao_de_tema()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir("/painel/pedidos");

        Assert.Contains("data-alternar-tema", html);
    }

    [Fact]
    public async Task Tema_define_as_cores_do_claro_e_do_escuro()
    {
        using var site = new SiteDeTeste();

        var css = await site.CreateClient().GetStringAsync("/css/tema.css");

        Assert.Contains("[data-tema=\"escuro\"]", css);
        Assert.Contains("[data-tema=\"claro\"]", css);
        foreach (var variavel in new[] { "--fundo", "--superficie", "--texto", "--acao", "--acao-texto", "--erro", "--moldura" })
            Assert.Equal(2, Regex.Matches(css, $@"\s{variavel}:").Count);
    }

    // Se um CSS de tela voltar a ter cor fixa (#fff, #4c6b44...), ela não muda com o tema e quebra o claro ou o escuro
    [Fact]
    public void Nenhum_css_de_tela_tem_cor_fixa()
    {
        using var site = new SiteDeTeste();
        var wwwroot = site.Services.GetRequiredService<IWebHostEnvironment>().WebRootPath;

        var comCorFixa = Directory.GetFiles(Path.Combine(wwwroot, "css"), "*.css")
            .Where(f => Path.GetFileName(f) != "tema.css")
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"#[0-9a-fA-F]{3,8}\b|(?<![-\w])color:\s*(white|black)\b")
                .Select(m => $"{Path.GetFileName(f)}: {m.Value}"))
            .ToList();

        Assert.True(comCorFixa.Count == 0, "Use as variáveis do tema.css:\n" + string.Join("\n", comCorFixa));
    }
}
