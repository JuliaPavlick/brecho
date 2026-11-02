using System.Security.Cryptography;
using System.Text;

namespace GarimpoChic.Util;

// Protege o CPF guardado no banco (LGPD). A chave vem da variável CHAVE_DADOS, que fica fora do banco:
// quem tiver só uma cópia do banco vê o CPF embaralhado.
//   - Cifrar/Decifrar: AES-GCM, para mostrar o CPF de volta (perfil, painel, backup)
//   - ResumoCpf: HMAC, sempre igual para o mesmo CPF, para achar a conta no login sem decifrar todo mundo
public static class Protecao
{
    const string Prefixo = "v1:";

    static byte[]? chaveCifra;
    static byte[]? chaveResumo;

    public static void Configurar(string? segredo)
    {
        if (string.IsNullOrEmpty(segredo) || segredo.Length < 32)
            throw new InvalidOperationException(
                "Defina a variável CHAVE_DADOS com pelo menos 32 caracteres aleatórios. " +
                "Ela protege os CPFs no banco: se for perdida, os CPFs só voltam por um backup.");

        var semente = Encoding.UTF8.GetBytes(segredo);
        chaveCifra = HKDF.DeriveKey(HashAlgorithmName.SHA256, semente, 32, info: "GarimpoChic.Cpf.Cifra"u8.ToArray());
        chaveResumo = HKDF.DeriveKey(HashAlgorithmName.SHA256, semente, 32, info: "GarimpoChic.Cpf.Resumo"u8.ToArray());
    }

    static byte[] Chave(byte[]? chave) =>
        chave ?? throw new InvalidOperationException("Protecao.Configurar não foi chamado.");

    public static string? ResumoCpf(string cpf) =>
        cpf == "" ? null : Convert.ToHexString(HMACSHA256.HashData(Chave(chaveResumo), Encoding.UTF8.GetBytes(cpf)));

    public static string Cifrar(string texto)
    {
        if (texto == "") return "";

        var claro = Encoding.UTF8.GetBytes(texto);
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var cifrado = new byte[claro.Length];

        using (var aes = new AesGcm(Chave(chaveCifra), tag.Length))
            aes.Encrypt(nonce, claro, cifrado, tag);

        return Prefixo + Convert.ToBase64String([.. nonce, .. tag, .. cifrado]);
    }

    public static string Decifrar(string valor)
    {
        // Contas antigas, de antes da criptografia: o valor ainda está em texto puro até o site convertê-lo ao iniciar
        if (!valor.StartsWith(Prefixo)) return valor;

        var dados = Convert.FromBase64String(valor[Prefixo.Length..]);
        var nonce = dados.AsSpan(0, AesGcm.NonceByteSizes.MaxSize);
        var tag = dados.AsSpan(nonce.Length, AesGcm.TagByteSizes.MaxSize);
        var cifrado = dados.AsSpan(nonce.Length + tag.Length);
        var claro = new byte[cifrado.Length];

        using (var aes = new AesGcm(Chave(chaveCifra), tag.Length))
            aes.Decrypt(nonce, cifrado, tag, claro);

        return Encoding.UTF8.GetString(claro);
    }
}
