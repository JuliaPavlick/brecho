using System.Net;
using GarimpoChic.Tests.Infra;
using GarimpoChic.Util;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Login, cadastro e saída da conta
public class ContaTests
{
    [Fact]
    public async Task Login_com_senha_certa_entra_e_vai_para_a_loja()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Entrar(SiteDeTeste.CpfMariana);

        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.Equal("/inicio", r.Destino());
        Assert.Equal(HttpStatusCode.OK, (await nav.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Funcionaria_escolhe_a_area_depois_de_entrar()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Entrar(SiteDeTeste.CpfJulia);

        Assert.Equal("/perfis", r.Destino());
    }

    [Fact]
    public async Task Login_com_senha_errada_nao_entra()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Entrar(SiteDeTeste.CpfMariana, "senha-errada");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("CPF ou senha incorretos", await r.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await nav.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Cinco_senhas_erradas_bloqueiam_o_cpf_mesmo_com_a_senha_certa_depois()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        for (var i = 0; i < 5; i++) await nav.Entrar(SiteDeTeste.CpfAna, "senha-errada");
        var r = await nav.Entrar(SiteDeTeste.CpfAna);

        Assert.Contains("Muitas tentativas erradas", await r.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await nav.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Login_nao_redireciona_para_outro_site()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Entrar(SiteDeTeste.CpfMariana, returnUrl: "https://site-malicioso.com/");

        Assert.Equal("/inicio", r.Destino());
    }

    [Fact]
    public async Task Login_volta_para_a_pagina_de_onde_a_pessoa_veio()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var r = await nav.Entrar(SiteDeTeste.CpfMariana, returnUrl: "/favoritos");

        Assert.Equal("/favoritos", r.Destino());
    }

    static Dictionary<string, string> Cadastro(string cpf) => new()
    {
        ["Nome"] = "Teste Automático",
        ["Cpf"] = cpf,
        ["Email"] = "teste@exemplo.com",
        ["Telefone"] = "(47) 99999-0000",
        ["Senha"] = "senha-forte-123",
        ["Aceito"] = "true",
    };

    [Fact]
    public async Task Cadastro_cria_a_conta_com_cpf_criptografado_e_ja_entra()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Abrir("/cadastro");

        var r = await nav.Enviar("/cadastro", Cadastro("862.883.667-57"));

        Assert.Equal("/inicio", r.Destino());
        var noBanco = await site.Banco(db => db.Database
            .SqlQueryRaw<string>("SELECT \"Cpf\" AS \"Value\" FROM \"Usuarios\" WHERE \"Nome\" = 'Teste Automático'")
            .SingleAsync());
        Assert.StartsWith("v1:", noBanco);
        Assert.DoesNotContain("86288366757", noBanco);
    }

    [Fact]
    public async Task Cadastro_recusa_cpf_que_ja_tem_conta()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Abrir("/cadastro");

        var r = await nav.Enviar("/cadastro", Cadastro(SiteDeTeste.CpfMariana));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("Já existe uma conta com este CPF", await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Cadastro_recusa_cpf_invalido()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Abrir("/cadastro");

        var r = await nav.Enviar("/cadastro", Cadastro("111.111.111-11"));

        Assert.Contains("Informe um CPF válido", await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Abrir_o_endereco_sair_nao_desloga()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);

        var (r, html) = await nav.Abrir("/sair");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("Sair da conta?", html);
        Assert.Equal(HttpStatusCode.OK, (await nav.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Sair_pelo_botao_desloga()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);

        var r = await nav.Enviar("/sair");

        Assert.Equal("/", r.Destino());
        Assert.Equal(HttpStatusCode.Redirect, (await nav.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Trocar_a_senha_derruba_o_login_dos_outros_aparelhos()
    {
        using var site = new SiteDeTeste();
        using var celular = site.Navegador();
        using var computador = site.Navegador();
        await celular.Entrar(SiteDeTeste.CpfMariana);
        await computador.Entrar(SiteDeTeste.CpfMariana);
        await computador.Abrir("/perfil");

        await computador.Enviar("/perfil/senha", new() { ["atual"] = SiteDeTeste.Senha, ["nova"] = "nova-senha-123" });

        Assert.Equal(HttpStatusCode.OK, (await computador.Abrir("/perfil")).Resposta.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await celular.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Excluir_a_conta_apaga_os_dados_pessoais()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        // A exclusão é bloqueada com pedido em aberto: marca os pedidos de demonstração da Mariana como entregues
        await site.Banco(async db =>
        {
            var id = (await db.Usuarios.ToListAsync()).Single(u => u.Cpf == Formato.SoDigitos(SiteDeTeste.CpfMariana)).Id;
            await db.Pedidos.Where(p => p.ClienteId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, Models.StatusPedido.Entregue));
            return id;
        });
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Abrir("/perfil");

        var r = await nav.Enviar("/perfil/excluir", new() { ["senha"] = SiteDeTeste.Senha });

        Assert.Equal("/entrar", r.Destino());
        var conta = await site.Banco(db => db.Usuarios.AsNoTracking().SingleAsync(u => u.ExcluidaEm != null));
        Assert.Equal("Conta excluída", conta.Nome);
        Assert.Equal("", conta.Cpf);
        Assert.Equal("", conta.Email);
        Assert.Equal("", conta.Telefone);
        Assert.Equal(HttpStatusCode.OK, (await nav.Entrar(SiteDeTeste.CpfMariana)).StatusCode); // não entra mais
    }
}
