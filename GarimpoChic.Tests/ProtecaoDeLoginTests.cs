using GarimpoChic.Models;
using GarimpoChic.Tests.Infra;
using GarimpoChic.Util;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GarimpoChic.Tests;

// Freios contra quem tenta adivinhar senhas e proteção das chaves que assinam o login
public class ProtecaoDeLoginTests
{
    const string CpfBeatriz = "987.654.321-00"; // conta de demonstração usada só nestes testes

    // Cada Navegador finge vir de um endereço diferente: é o que faria quem forja o cabeçalho do IP
    static async Task ErrarDeVariosEnderecos(SiteDeTeste site, string cpf, int vezes)
    {
        for (var i = 0; i < vezes; i++)
        {
            using var nav = site.Navegador();
            await nav.Entrar(cpf, "senha-errada");
        }
    }

    [Fact]
    public async Task Vinte_erros_de_enderecos_diferentes_pausam_o_login_do_cpf()
    {
        using var site = new SiteDeTeste();
        await ErrarDeVariosEnderecos(site, CpfBeatriz, 20);
        using var nav = site.Navegador();

        var r = await nav.Entrar(CpfBeatriz); // senha certa, de um endereço novo

        Assert.Contains("Muitas tentativas erradas", await r.Content.ReadAsStringAsync());
        Assert.Null(r.Destino());
    }

    [Fact]
    public async Task Cpf_sem_conta_recebe_a_mesma_pausa_e_nao_revela_quem_e_cliente()
    {
        using var site = new SiteDeTeste();
        const string semConta = "456.894.770-59";
        await ErrarDeVariosEnderecos(site, semConta, 20);
        using var nav = site.Navegador();

        var r = await nav.Entrar(semConta);

        Assert.Contains("Muitas tentativas erradas", await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Redefinir_a_senha_pelo_email_libera_o_cpf()
    {
        using var site = new SiteDeTeste();
        const string cpfCamila = "390.533.447-05";
        await ErrarDeVariosEnderecos(site, cpfCamila, 20);
        var camila = await site.Banco(async db =>
            (await db.Usuarios.AsNoTracking().ToListAsync()).Single(u => u.Cpf == Formato.SoDigitos(cpfCamila)));
        var codigo = site.Services.GetRequiredService<RedefinicaoSenha>().GerarCodigo(camila);

        using var nav = site.Navegador();
        await nav.Abrir("/redefinir-senha?codigo=" + Uri.EscapeDataString(codigo));
        await nav.Enviar("/redefinir-senha", new() { ["codigo"] = codigo, ["senha"] = "senha-nova-123" });
        var r = await nav.Entrar(cpfCamila, "senha-nova-123");

        Assert.Equal("/inicio", r.Destino());
    }

    [Fact]
    public async Task Chaves_do_login_ficam_cifradas_no_banco()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana); // o login cria a chave

        var chaves = await site.Banco(db => db.DataProtectionKeys.Select(k => k.Xml!).ToListAsync());

        Assert.NotEmpty(chaves);
        Assert.All(chaves, xml =>
        {
            Assert.Contains("<chaveCifrada", xml);
            Assert.DoesNotContain("<masterKey", xml);
        });
        // E o login continua funcionando com a chave cifrada
        Assert.Equal(System.Net.HttpStatusCode.OK, (await nav.Abrir("/perfil")).Resposta.StatusCode);
    }

    [Fact]
    public async Task Chaves_antigas_sem_criptografia_sao_apagadas()
    {
        using var site = new SiteDeTeste();
        using var nav = site.Navegador();
        await nav.Entrar(SiteDeTeste.CpfMariana);
        const string antiga = """
            <key id="00000000-0000-0000-0000-000000000001" version="1"><descriptor><descriptor>
            <masterKey p4:requiresEncryption="true" xmlns:p4="http://schemas.asp.net/2015/03/dataProtection">
            <value>segredo-em-texto-puro</value></masterKey></descriptor></descriptor></key>
            """;
        await site.Banco(async db =>
        {
            db.DataProtectionKeys.Add(new DataProtectionKey { FriendlyName = "antiga", Xml = antiga });
            return await db.SaveChangesAsync();
        });

        var apagadas = await site.Banco(CifraDeChaves.ApagarChavesEmTextoPuroAsync);

        Assert.Equal(1, apagadas);
        Assert.False(await site.Banco(db => db.DataProtectionKeys.AnyAsync(k => k.FriendlyName == "antiga")));
        Assert.True(await site.Banco(db => db.DataProtectionKeys.AnyAsync())); // a chave cifrada continua
    }

    [Fact]
    public void Chave_de_login_cifrada_volta_igual()
    {
        Protecao.Configurar("chave-dos-testes-automaticos-com-mais-de-32-caracteres");
        const string xml = "<masterKey><value>abc</value></masterKey>";

        var cifrada = Protecao.CifrarChaveDeLogin(xml);

        Assert.DoesNotContain("abc", cifrada);
        Assert.Equal(xml, Protecao.DecifrarChaveDeLogin(cifrada));
        // Chave própria: o que foi cifrado para o login não abre como CPF
        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() => Protecao.Decifrar(cifrada));
    }
}
