using GarimpoChic.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Marcações que o leitor de tela e quem navega pelo teclado precisam encontrar nas páginas
public class AcessibilidadeTests
{
    [Theory]
    [InlineData("/painel/produtos", "aria-label=\"Buscar peças pelo nome\"")]
    [InlineData("/painel/pedidos", "aria-label=\"Buscar pedidos por cliente ou número\"")]
    [InlineData("/painel/clientes", "aria-label=\"Buscar clientes por nome ou e-mail\"")]
    [InlineData("/painel/calendario", "aria-label=\"Mês anterior\"")]
    [InlineData("/painel/calendario", "aria-label=\"Próximo mês\"")]
    [InlineData("/painel", "role=\"img\" aria-describedby=\"grafico-resumo\"")]
    [InlineData("/painel", "aria-pressed=\"true\">Semana</button>")]
    public async Task Paineis_tem_nomes_para_o_leitor_de_tela(string url, string marcacao)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir(url);

        Assert.Contains(marcacao, html);
    }

    [Fact]
    public async Task Campo_do_arquivo_de_backup_tem_nome()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);
        // A restauração só aparece com a loja vazia (sem produtos e sem pedidos)
        await site.Banco(async db =>
        {
            await db.PedidoItens.ExecuteDeleteAsync();
            await db.Pedidos.ExecuteDeleteAsync();
            return await db.Produtos.ExecuteDeleteAsync();
        });

        var (_, html) = await nav.Abrir("/painel/backup");

        Assert.Contains("aria-label=\"Arquivo .zip do backup\"", html);
    }

    [Fact]
    public async Task Icones_dos_cartoes_do_painel_nao_repetem_o_titulo()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir("/painel");

        Assert.DoesNotContain("alt=\"Peças vendidas\"", html);
        Assert.DoesNotContain("alt=\"Lucro bruto\"", html);
    }

    [Fact]
    public async Task Explorar_agora_do_inicio_e_um_link_de_verdade()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);

        var (_, html) = await nav.Abrir("/inicio");

        Assert.Contains("<a href=\"/explorar\" class=\"botao-hero\">", html);
        Assert.DoesNotContain("data-href=", html);
    }

    [Fact]
    public async Task Css_mantem_o_foco_visivel_e_abre_o_menu_pelo_teclado()
    {
        using var site = new SiteDeTeste();
        var http = site.CreateClient();

        var geral = await http.GetStringAsync("/css/site.css");
        var cliente = await http.GetStringAsync("/css/dashboard_cliente.css");
        var painel = await http.GetStringAsync("/css/dashfunc.css");

        Assert.Contains(":focus-visible", geral);
        Assert.Contains(".sidebar:focus-within .texto-menu", cliente);
        Assert.Contains(".sidebar:focus-within .menu .texto", painel);
    }

    [Fact]
    public async Task Script_liga_a_mensagem_de_erro_ao_campo()
    {
        using var site = new SiteDeTeste();

        var js = await site.CreateClient().GetStringAsync("/js/site.js");

        Assert.Contains("aria-invalid", js);
        Assert.Contains("aria-describedby", js);
    }
}
