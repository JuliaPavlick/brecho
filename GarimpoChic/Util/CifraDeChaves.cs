using System.Xml.Linq;
using GarimpoChic.Data;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Util;

// As chaves que assinam o cookie de login (e os links de redefinir senha) ficam no banco, na tabela
// DataProtectionKeys. Sem criptografia, quem tivesse uma cópia do banco poderia fabricar o login de qualquer
// pessoa, inclusive de funcionária. Aqui elas são cifradas com a mesma CHAVE_DADOS dos CPFs (veja Protecao),
// que fica fora do banco.
public sealed class CifraDeChaves : IXmlEncryptor
{
    public EncryptedXmlInfo Encrypt(XElement plaintextElement) =>
        new(new XElement("chaveCifrada", Protecao.CifrarChaveDeLogin(plaintextElement.ToString(SaveOptions.DisableFormatting))),
            typeof(DecifraDeChaves));

    // Chaves gravadas antes desta criptografia: o segredo aparece em texto puro, dentro de <masterKey><value>
    public static bool EmTextoPuro(string xml) => xml.Contains("<masterKey") && !xml.Contains("<chaveCifrada");

    // Apaga as chaves antigas, sem criptografia. Quem estava logado precisa entrar de novo uma vez,
    // e links de redefinir senha já enviados deixam de valer; em troca, o segredo some do banco.
    public static async Task<int> ApagarChavesEmTextoPuroAsync(BrechoContext db)
    {
        var antigas = (await db.DataProtectionKeys.ToListAsync()).Where(k => EmTextoPuro(k.Xml ?? "")).ToList();
        db.DataProtectionKeys.RemoveRange(antigas);
        await db.SaveChangesAsync();
        return antigas.Count;
    }
}

// Criada pelo próprio ASP.NET ao ler uma chave cifrada (por isso o construtor vazio)
public sealed class DecifraDeChaves : IXmlDecryptor
{
    public XElement Decrypt(XElement encryptedElement) =>
        XElement.Parse(Protecao.DecifrarChaveDeLogin(encryptedElement.Value));
}
