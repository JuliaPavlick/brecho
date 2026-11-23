using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using GarimpoChic.Util;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Telas do painel: números certos depois das consultas mais leves, e as peças compartilhadas do site.js
public class PainelTests
{
    // As telas do painel recebem os dados num "let x = [...]" dentro do <script>
    static JsonElement DadosDaPagina(string html, string variavel)
    {
        var m = Regex.Match(html, $@"let {variavel} = (\[.*?\]);\s*\n", RegexOptions.Singleline);
        Assert.True(m.Success, $"não achei '{variavel}' na página");
        return JsonDocument.Parse(m.Groups[1].Value).RootElement;
    }

    [Fact]
    public async Task Clientes_mostra_pedidos_e_total_gasto_certos()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);
        // Um pedido cancelado da Ana não conta no total gasto, mas conta como pedido
        await site.Banco(async db =>
        {
            var ana = (await db.Usuarios.ToListAsync()).Single(u => u.Nome == "Ana Clara");
            var pedido = await db.Pedidos.FirstAsync(p => p.ClienteId == ana.Id);
            pedido.Status = StatusPedido.Cancelado;
            return await db.SaveChangesAsync();
        });
        var esperado = await site.Banco(async db =>
        {
            var pedidos = await db.Pedidos.Include(p => p.Itens).Include(p => p.Cliente).ToListAsync();
            return pedidos.GroupBy(p => p.Cliente!.Nome).ToDictionary(g => g.Key,
                g => (Qtd: g.Count(), Gasto: g.Where(p => p.Status != StatusPedido.Cancelado).Sum(p => p.Total)));
        });

        var (_, html) = await nav.Abrir("/painel/clientes");
        var pessoas = Regex.Match(html, @"const pessoas = (\[.*?\]);\s*\n", RegexOptions.Singleline).Groups[1].Value;

        foreach (var p in JsonDocument.Parse(pessoas).RootElement.EnumerateArray())
        {
            var nome = p.GetProperty("nome").GetString()!;
            var (qtd, gasto) = esperado.GetValueOrDefault(nome);
            Assert.Equal(qtd, p.GetProperty("pedidos").GetInt32());
            Assert.Equal(gasto, p.GetProperty("gasto").GetDecimal());
        }
    }

    [Fact]
    public async Task Pedidos_mostra_cliente_contato_e_itens()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);
        var maisRecente = await site.Banco(db => db.Pedidos.Include(p => p.Itens).Include(p => p.Cliente)
            .OrderByDescending(p => p.Data).FirstAsync());

        var (_, html) = await nav.Abrir("/painel/pedidos");
        var primeiro = DadosDaPagina(html, "pedidos")[0];

        Assert.Equal(maisRecente.Id, primeiro.GetProperty("id").GetInt32());
        Assert.Equal(maisRecente.Cliente!.Nome, primeiro.GetProperty("cliente").GetString());
        Assert.Equal(maisRecente.Cliente.Telefone, primeiro.GetProperty("telefone").GetString());
        Assert.Equal(maisRecente.Cliente.Email, primeiro.GetProperty("email").GetString());
        Assert.Equal(maisRecente.Itens.Count, primeiro.GetProperty("itens").GetArrayLength());
    }

    [Fact]
    public async Task Produtos_mostra_o_nome_da_vendedora()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir("/painel/produtos");
        var casaco = DadosDaPagina(html, "produtos").EnumerateArray().Single(p => p.GetProperty("nome").GetString() == "Casaco de Lã");
        var blusa = DadosDaPagina(html, "produtos").EnumerateArray().Single(p => p.GetProperty("nome").GetString() == "Blusa Cropped");

        Assert.Equal("Ana Clara", casaco.GetProperty("vendedora").GetString());
        Assert.Equal(JsonValueKind.Null, blusa.GetProperty("vendedora").ValueKind);
        Assert.Equal(1, casaco.GetProperty("qtdFotos").GetInt32());
    }

    [Fact]
    public async Task Financeiro_e_dashboard_somam_as_vendas_do_mes()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);
        var inicioMes = Formato.ParaUtc(new DateTime(Formato.Agora.Year, Formato.Agora.Month, 1));
        var receita = await site.Banco(async db => (await db.Pedidos.Include(p => p.Itens)
            .Where(p => p.Data >= inicioMes && p.Status != StatusPedido.Cancelado).ToListAsync()).Sum(p => p.Total));

        var (r, html) = await nav.Abrir("/painel/financeiro");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains(Formato.Moeda(receita), WebUtility.HtmlDecode(html));
        // As movimentações vão como JSON na página (o travessão sai escapado), então lê o JSON
        var movimentos = Regex.Match(html, @"const movimentacoes = (\[.*?\]);\s*\n", RegexOptions.Singleline).Groups[1].Value;
        Assert.Contains(JsonDocument.Parse(movimentos).RootElement.EnumerateArray(),
            m => m.GetProperty("descricao").GetString() == "Repasse — Ana Clara");
        Assert.Equal(HttpStatusCode.OK, (await nav.Abrir("/painel")).Resposta.StatusCode);
    }

    [Theory]
    [InlineData("/painel/produtos", "tabelaPaginada(", "janela('modal', 'btn-cancelar')", "chipsDeFiltro('status'")]
    [InlineData("/painel/pedidos", "tabelaPaginada(", "janela('modal', 'btn-fechar')", "chipsDeFiltro('status'")]
    [InlineData("/painel/clientes", "tabelaPaginada(", "janela('modal', 'btn-fechar')", "chipsDeFiltro('papel'")]
    [InlineData("/painel/calendario", "janela('modal', 'btn-cancelar')", "janela(", "janela(")]
    public async Task Telas_usam_as_pecas_compartilhadas(string url, params string[] trechos)
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfJulia);

        var (_, html) = await nav.Abrir(url);

        foreach (var t in trechos) Assert.Contains(t, html);
        // A cópia antiga do fechamento da janela não pode ter sobrado
        Assert.DoesNotContain("function fecharModal", html);
        Assert.DoesNotContain("document.addEventListener('keydown'", html);
    }

    [Fact]
    public async Task Site_js_define_as_pecas_compartilhadas()
    {
        using var site = new SiteDeTeste();

        var js = await site.CreateClient().GetStringAsync("/js/site.js");

        Assert.Contains("function janela(", js);
        Assert.Contains("function chipsDeFiltro(", js);
        Assert.Contains("function tabelaPaginada(", js);
    }
}
