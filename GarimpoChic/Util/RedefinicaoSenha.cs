using System.Security.Cryptography;
using System.Text;
using GarimpoChic.Models;
using Microsoft.AspNetCore.DataProtection;

namespace GarimpoChic.Util;

// Gera e confere o código do link "redefinir senha".
// O código é assinado, vale 1 hora e deixa de valer assim que a senha é trocada
// (ele carrega um resumo da senha atual), então só pode ser usado uma vez.
public class RedefinicaoSenha(IDataProtectionProvider provider)
{
    static readonly TimeSpan Validade = TimeSpan.FromHours(1);

    readonly ITimeLimitedDataProtector protetor =
        provider.CreateProtector("GarimpoChic.RedefinirSenha").ToTimeLimitedDataProtector();

    static string Carimbo(Usuario u) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(u.SenhaHash)))[..16];

    public string GerarCodigo(Usuario u) => protetor.Protect($"{u.Id}:{Carimbo(u)}", Validade);

    // Devolve o id da pessoa se o código for válido e ainda não tiver vencido
    public int? LerUsuarioId(string? codigo)
    {
        try
        {
            var partes = protetor.Unprotect(codigo ?? "").Split(':');
            return int.Parse(partes[0]);
        }
        catch
        {
            return null;
        }
    }

    public bool CodigoConfere(string? codigo, Usuario u)
    {
        try { return protetor.Unprotect(codigo ?? "") == $"{u.Id}:{Carimbo(u)}"; }
        catch { return false; }
    }
}
