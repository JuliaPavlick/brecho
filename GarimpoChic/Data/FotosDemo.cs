using GarimpoChic.Models;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Data;

// Coloca fotos (de domínio público, veja Data/FotosDemo/CREDITOS.md) nas peças fictícias da demonstração.
// Só mexe em peças com o nome E a descrição exatos da demonstração e que ainda não têm foto,
// para nunca tocar numa peça real da loja.
public static class FotosDemo
{
    static readonly (string Nome, string Descricao, string Arquivo)[] Pecas =
    [
        ("Blusa Vintage", "Peça exclusiva", "blusa-vintage.jpg"),
        ("Vestido Midi Floral", "Tamanho M", "vestido-midi-floral.jpg"),
        ("Bolsa Retrô", "Nova coleção", "bolsa-retro.jpg"),
        ("Salto Clássico", "Última unidade", "salto-classico.jpg"),
        ("Casaco de Lã", "Tamanho G", "casaco-de-la.jpg"),
        ("Calça Jeans Reta", "Tamanho 38", "calca-jeans-reta.jpg"),
        ("Chapéu Palha", "Peça exclusiva", "chapeu-palha.jpg"),
        ("Óculos Vintage", "Anos 80", "oculos-vintage.jpg"),
        ("Vestido Preto Clássico", "Tamanho P", "vestido-preto-classico.jpg"),
        ("Jaqueta Jeans Oversized", "Enviada para avaliação", "jaqueta-jeans-oversized.jpg"),
        ("Lenço de Seda", "Anos 70", "lenco-de-seda.jpg"),
    ];

    public static async Task AnexarAsync(BrechoContext db)
    {
        var nomes = Pecas.Select(p => p.Nome).ToList();
        var semFoto = await db.Produtos
            .Where(p => nomes.Contains(p.Nome) && !p.Fotos.Any())
            .Select(p => new { p.Id, p.Nome, p.Descricao })
            .ToListAsync();

        var assembly = typeof(FotosDemo).Assembly;
        foreach (var peca in semFoto)
        {
            var arquivo = Pecas.FirstOrDefault(x => x.Nome == peca.Nome && x.Descricao == peca.Descricao).Arquivo;
            if (arquivo == null) continue;

            await using var recurso = assembly.GetManifestResourceStream("FotosDemo/" + arquivo);
            if (recurso == null) continue;

            using var ms = new MemoryStream();
            await recurso.CopyToAsync(ms);
            db.ProdutoFotos.Add(new ProdutoFoto { ProdutoId = peca.Id, ContentType = "image/jpeg", Dados = ms.ToArray() });
        }

        await db.SaveChangesAsync();
    }
}
