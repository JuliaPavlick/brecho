using System.Net;
using System.Text.Json;
using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Exclusão de peças, fotos recusadas e formulários que não perdem o que foi digitado
public class DadosDaLojaTests
{
    // Menor PNG válido o bastante para o site reconhecer pelo começo do arquivo
    static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    static readonly byte[] NaoEImagem = "isto não é uma foto"u8.ToArray();

    // ---------- Excluir peça ----------

    [Fact]
    public async Task Peca_num_pedido_em_andamento_nao_pode_ser_excluida()
    {
        using var site = new SiteDeTeste();
        using var cliente = site.Navegador();
        using var funcionaria = site.Navegador();
        await cliente.Entrar(SiteDeTeste.CpfMariana);
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var bolsa = await site.Produto("Bolsa Retrô");
        await cliente.Fetch($"/sacola/{bolsa.Id}");
        await cliente.Enviar("/sacola/finalizar", new() { ["Entrega"] = "retirada" });

        var r = await funcionaria.Fetch($"/painel/produtos/{bolsa.Id}/excluir");

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Contains("ainda não foi entregue", await r.Content.ReadAsStringAsync());
        Assert.True(await site.Banco(db => db.Produtos.AnyAsync(p => p.Id == bolsa.Id)));
    }

    [Fact]
    public async Task Peca_de_vendedora_ja_na_loja_nao_pode_ser_excluida()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var casaco = await site.Produto("Casaco de Lã"); // da Ana, disponível

        var r = await funcionaria.Fetch($"/painel/produtos/{casaco.Id}/excluir");

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Contains("Ana Clara", await r.Content.ReadAsStringAsync());
        Assert.True(await site.Banco(db => db.Produtos.AnyAsync(p => p.Id == casaco.Id)));
    }

    [Fact]
    public async Task Envio_de_vendedora_em_avaliacao_pode_ser_recusado_e_excluido()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var jaqueta = await site.Produto("Jaqueta Jeans Oversized"); // da Camila, em avaliação

        var r = await funcionaria.Fetch($"/painel/produtos/{jaqueta.Id}/excluir");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.False(await site.Banco(db => db.Produtos.AnyAsync(p => p.Id == jaqueta.Id)));
        Assert.False(await site.Banco(db => db.ProdutoFotos.AnyAsync(f => f.ProdutoId == jaqueta.Id)));
    }

    [Fact]
    public async Task Peca_da_loja_fora_de_pedidos_pode_ser_excluida()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);
        var salto = await site.Produto("Salto Clássico");

        var r = await funcionaria.Fetch($"/painel/produtos/{salto.Id}/excluir");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.False(await site.Banco(db => db.Produtos.AnyAsync(p => p.Id == salto.Id)));
    }

    // ---------- Fotos recusadas ----------

    [Fact]
    public async Task Vender_avisa_quando_uma_foto_e_recusada()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfAna);
        await nav.Abrir("/vender");

        var r = await nav.EnviarArquivos("/vender",
            new() { ["nome"] = "Saia Plissada", ["categoria"] = "Roupas", ["preco"] = "60" },
            ("fotos", "boa.png", Png), ("fotos", "falsa.jpg", NaoEImagem));
        var (_, html) = await nav.Abrir(r.Destino()!);

        Assert.Contains("1 foto não foi aceita", html);
        var saia = await site.Banco(db => db.Produtos.Include(p => p.Fotos).SingleAsync(p => p.Nome == "Saia Plissada"));
        Assert.Single(saia.Fotos);
    }

    [Fact]
    public async Task Painel_avisa_quando_uma_foto_e_recusada()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        var r = await funcionaria.Fetch("/painel/produtos", Navegador.Multipart(
            new() { ["Nome"] = "Cinto de Couro", ["Categoria"] = "Acessórios", ["Preco"] = "35", ["Status"] = "disponivel" },
            ("Fotos", "falsa.png", NaoEImagem)));

        var salvo = JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("não foi aceita", salvo.GetProperty("aviso").GetString());
        Assert.Equal(0, salvo.GetProperty("qtdFotos").GetInt32());
    }

    [Fact]
    public async Task Painel_informa_quantas_fotos_a_peca_tem()
    {
        using var site = new SiteDeTeste();
        using var funcionaria = site.Navegador();
        await funcionaria.Entrar(SiteDeTeste.CpfJulia);

        var r = await funcionaria.Fetch("/painel/produtos", Navegador.Multipart(
            new() { ["Nome"] = "Cinto de Couro", ["Categoria"] = "Acessórios", ["Preco"] = "35", ["Status"] = "disponivel" },
            ("Fotos", "a.png", Png), ("Fotos", "b.png", Png)));

        var salvo = JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(2, salvo.GetProperty("qtdFotos").GetInt32());
        Assert.Equal(JsonValueKind.Null, salvo.GetProperty("aviso").ValueKind);
    }

    // ---------- Formulários mantêm o que foi digitado ----------

    [Fact]
    public async Task Vender_com_erro_mantem_o_que_foi_digitado()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfAna);
        await nav.Abrir("/vender");

        var r = await nav.EnviarArquivos("/vender",
            new() { ["nome"] = "Vestido de Festa Azul", ["descricao"] = "Usado uma vez", ["categoria"] = "Vestidos", ["preco"] = "0" });
        var html = await r.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("preço maior que zero", html);
        Assert.Contains("value=\"Vestido de Festa Azul\"", html);
        Assert.Contains(">Usado uma vez</textarea>", html);
        Assert.Contains("<option selected=\"selected\">Vestidos</option>", html);
        Assert.False(await site.Banco(db => db.Produtos.AnyAsync(p => p.Nome == "Vestido de Festa Azul")));
    }

    [Fact]
    public async Task Perfil_com_erro_mantem_o_que_foi_digitado_e_mostra_o_campo_de_senha()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Abrir("/perfil");

        var r = await nav.Enviar("/perfil/dados", new()
        {
            ["nome"] = "Mariana Souza Lima", ["email"] = "novo.email@exemplo.com", ["telefone"] = "(47) 97777-4321",
            ["cidade"] = "Joinville", ["estado"] = "SC",
        });
        var html = await r.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("confirme sua senha atual", html);
        Assert.Contains("value=\"Mariana Souza Lima\"", html);
        Assert.Contains("value=\"novo.email@exemplo.com\"", html);
        Assert.DoesNotContain("id=\"campo-senha-email\" hidden", html);
        Assert.False(await site.Banco(db => db.Usuarios.AnyAsync(u => u.Nome == "Mariana Souza Lima")));
    }

    [Fact]
    public async Task Perfil_salva_os_dados_e_o_estado()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Abrir("/perfil");

        var r = await nav.Enviar("/perfil/dados", new()
        {
            ["nome"] = "Mariana Souza Lima", ["email"] = "mariana.souza@email.com", ["telefone"] = "(47) 97777-4321",
            ["cidade"] = "Blumenau", ["estado"] = "Santa Catarina", // estado fora da lista é ignorado
        });

        Assert.Equal("/perfil", r.Destino());
        var u = await site.Banco(db => db.Usuarios.AsNoTracking().SingleAsync(x => x.Nome == "Mariana Souza Lima"));
        Assert.Equal("Blumenau", u.Cidade);
        Assert.Null(u.Estado);
    }
}
