using System.Net;
using System.Text.RegularExpressions;
using GarimpoChic.Tests.Infra;

namespace GarimpoChic.Tests;

// Para onde cada endereço leva e o que cada menu oferece
public class NavegacaoTests
{
    [Fact]
    public async Task Visitante_que_chega_no_endereco_principal_ve_as_pecas()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (r, _) = await nav.Abrir("/");

        Assert.Equal("/explorar", r.Destino());
    }

    [Fact]
    public async Task Quem_ja_entrou_vai_para_a_sua_area()
    {
        using var site = new SiteDeTeste();
        using var cliente = site.Navegador();
        using var funcionaria = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        Assert.Equal("/inicio", (await cliente.Abrir("/")).Resposta.Destino());
        Assert.Equal("/perfis", (await funcionaria.Abrir("/")).Resposta.Destino());
    }

    [Fact]
    public async Task Link_antigo_de_login_continua_funcionando()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (r, _) = await nav.Abrir("/?ReturnUrl=%2Fsacola");

        Assert.Equal("/entrar?ReturnUrl=%2Fsacola", r.Destino());
    }

    [Fact]
    public async Task Formulario_de_login_antigo_que_envia_para_a_raiz_ainda_entra()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Abrir("/entrar");

        var r = await nav.Enviar("/", new() { ["cpf"] = SiteDeTeste.CpfMariana, ["senha"] = SiteDeTeste.Senha });

        Assert.Equal("/inicio", r.Destino());
    }

    [Fact]
    public async Task Pagina_de_login_fica_em_entrar()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (r, html) = await nav.Abrir("/entrar");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("action=\"/entrar\"", html);
    }

    [Fact]
    public async Task Links_de_entrar_do_catalogo_levam_ao_login_e_voltam_para_a_peca()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        var peca = await site.Produto("Bolsa Retrô");

        var (_, html) = await nav.Abrir($"/peca/{peca.Id}");

        Assert.Contains($"href=\"/entrar?ReturnUrl=%2Fpeca%2F{peca.Id}\"", html);
        Assert.DoesNotContain("href=\"/?ReturnUrl", html);
    }

    // No celular, os itens com "extra" saem da barra de baixo e vão para o botão "Mais"
    static List<string> Extras(string html) =>
        Regex.Matches(html, "<a href=\"([^\"]+)\" class=\"extra[^\"]*\"").Select(m => m.Groups[1].Value).ToList();

    [Fact]
    public async Task Menu_da_cliente_no_celular_tem_o_botao_mais_com_perfil_e_sair()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);

        var (_, html) = await nav.Abrir("/inicio");

        Assert.Contains("class=\"menu-mais\"", html);
        Assert.Equal(["/compras", "/vender", "/perfil", "/sair"], Extras(html));
    }

    [Fact]
    public async Task Menu_da_funcionaria_no_celular_tem_o_botao_mais()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir("/painel");

        Assert.Contains("class=\"menu-mais\"", html);
        Assert.Equal(["/painel/calendario", "/painel/financeiro", "/painel/clientes", "/painel/backup", "/inicio"], Extras(html));
    }

    [Fact]
    public async Task Visitante_tem_poucos_itens_e_nao_precisa_do_botao_mais()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (_, html) = await nav.Abrir("/explorar");

        Assert.DoesNotContain("class=\"menu-mais\"", html);
    }
}
