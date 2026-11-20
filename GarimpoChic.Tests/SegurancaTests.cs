using System.Diagnostics;
using System.Net;
using GarimpoChic.Tests.Infra;
using GarimpoChic.Util;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Tests;

// Proteções que não aparecem na tela, mas seguram ataques
public class SegurancaTests
{
    [Fact]
    public async Task Formulario_sem_token_anti_falsificacao_e_recusado()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Abrir("/perfil");

        var r = await nav.Enviar("/perfil/dados", new()
        {
            ["nome"] = "Nome Trocado Por Outro Site", ["email"] = "atacante@exemplo.com", ["telefone"] = "(47) 90000-0000",
        }, comToken: false);

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.False(await site.Banco(db => db.Usuarios.AnyAsync(u => u.Nome == "Nome Trocado Por Outro Site")));
    }

    [Fact]
    public async Task Trocar_o_email_exige_a_senha()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        await nav.Abrir("/perfil");

        await nav.Enviar("/perfil/dados", new()
        {
            ["nome"] = "Mariana Souza", ["email"] = "atacante@exemplo.com", ["telefone"] = "(47) 97777-4321",
        });

        Assert.False(await site.Banco(db => db.Usuarios.AnyAsync(u => u.Email == "atacante@exemplo.com")));
    }

    [Fact]
    public async Task Paginas_vao_com_os_cabecalhos_de_seguranca()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (r, _) = await nav.Abrir("/");

        var csp = string.Join(" ", r.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("script-src 'self' 'nonce-", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
        Assert.Equal("nosniff", r.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", r.Headers.GetValues("X-Frame-Options").Single());
    }

    [Fact]
    public async Task Endereco_inexistente_mostra_a_pagina_de_nao_encontrada()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();

        var (r, html) = await nav.Abrir("/pagina-que-nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Contains("Página não encontrada", html);
    }

    // Se o CPF sem conta respondesse bem mais rápido (sem conferir a senha), daria para descobrir pelo tempo quem é cliente
    [Fact]
    public async Task Login_leva_o_mesmo_tempo_com_ou_sem_conta()
    {
        using var site = new SiteDeTeste();

        async Task<double> Medir(string cpf)
        {
            var tempos = new List<double>();
            for (var i = 0; i < 3; i++)
            {
                using var nav = site.Navegador();
                await nav.Abrir("/");
                var relogio = Stopwatch.StartNew();
                await nav.Enviar("/", new() { ["cpf"] = cpf, ["senha"] = "senha-errada" });
                tempos.Add(relogio.Elapsed.TotalMilliseconds);
            }
            return tempos.Order().ElementAt(1); // mediana
        }

        await Medir(SiteDeTeste.CpfMariana); // aquece
        var comConta = await Medir(SiteDeTeste.CpfMariana);
        var semConta = await Medir("714.287.938-60");

        // Conferir a senha custa dezenas de milissegundos; sem a proteção, "sem conta" sairia perto de zero
        Assert.True(semConta > comConta * 0.5, $"com conta: {comConta:0} ms, sem conta: {semConta:0} ms");
    }

    [Fact]
    public async Task No_site_publicado_a_conta_de_demonstracao_nao_e_funcionaria()
    {
        using var site = new SiteDeTeste("Production");
        using var nav = site.Navegador();

        var r = await nav.Entrar(SiteDeTeste.CpfJulia);

        Assert.Equal("/inicio", r.Destino());
        Assert.False(await site.Banco(db => db.Usuarios.AnyAsync(u => u.EhFuncionaria)));
    }

    [Fact]
    public void Fotos_so_aceitam_imagens_de_verdade()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0 };
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>"u8.ToArray();
        var html = "<html><script>alert(1)</script></html>"u8.ToArray();

        Assert.Equal("image/png", Fotos.DetectarTipo(png));
        Assert.Equal("image/jpeg", Fotos.DetectarTipo(jpeg));
        Assert.Null(Fotos.DetectarTipo(svg));
        Assert.Null(Fotos.DetectarTipo(html));
    }
}
