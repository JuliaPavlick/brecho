using System.IO.Compression;
using System.Text.Json;
using GarimpoChic.Data;
using GarimpoChic.Models;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Util;

// Cópia completa dos dados num arquivo .zip:
//   manifesto.json        versão do formato e data
//   <tabela>.json         uma lista por tabela, com os ids originais
//   fotos/<id>            os bytes de cada foto (fora do JSON, para o arquivo não inchar)
// As chaves do login (DataProtectionKeys) ficam de fora: depois de restaurar, todos entram de novo.
public static class Backup
{
    const int Versao = 1;

    static readonly JsonSerializerOptions Json = new()
    {
        // Ignora campos calculados (Total, Emoji...) e navegações vazias
        IgnoreReadOnlyProperties = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    record Manifesto(int Versao, DateTime CriadoEm);

    public static async Task ExportarAsync(BrechoContext db, Stream destino)
    {
        using var zip = new ZipArchive(destino, ZipArchiveMode.Create, leaveOpen: true);

        void Gravar<T>(string nome, T dados)
        {
            using var s = zip.CreateEntry(nome, CompressionLevel.Optimal).Open();
            JsonSerializer.Serialize(s, dados, Json);
        }

        Gravar("manifesto.json", new Manifesto(Versao, DateTime.UtcNow));
        Gravar("usuarios.json", await db.Usuarios.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        Gravar("produtos.json", await db.Produtos.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        Gravar("favoritos.json", await db.Favoritos.AsNoTracking().ToListAsync());
        Gravar("sacola.json", await db.Sacola.AsNoTracking().ToListAsync());
        Gravar("pedidos.json", await db.Pedidos.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        Gravar("pedido_itens.json", await db.PedidoItens.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        Gravar("eventos.json", await db.Eventos.AsNoTracking().OrderBy(x => x.Id).ToListAsync());
        Gravar("repasses.json", await db.Repasses.AsNoTracking().OrderBy(x => x.Id).ToListAsync());

        // Fotos: primeiro a lista sem os bytes, depois cada uma num arquivo, uma de cada vez (economiza memória)
        var fotos = await db.ProdutoFotos.AsNoTracking().OrderBy(f => f.Id)
            .Select(f => new ProdutoFoto { Id = f.Id, ProdutoId = f.ProdutoId, ContentType = f.ContentType })
            .ToListAsync();
        Gravar("fotos.json", fotos);

        foreach (var f in fotos)
        {
            var dados = await db.ProdutoFotos.Where(x => x.Id == f.Id).Select(x => x.Dados).FirstAsync();
            // Fotos já são comprimidas (JPEG/PNG/WebP); comprimir de novo só gastaria tempo
            await using var s = zip.CreateEntry($"fotos/{f.Id}", CompressionLevel.NoCompression).Open();
            await s.WriteAsync(dados);
        }
    }

    // Só restaura num banco "novo": sem produtos e sem pedidos. Assim não há risco de apagar dados por engano.
    public static async Task<bool> PodeRestaurarAsync(BrechoContext db) =>
        !await db.Produtos.AnyAsync() && !await db.Pedidos.AnyAsync();

    public static async Task RestaurarAsync(BrechoContext db, Stream origem)
    {
        using var zip = new ZipArchive(origem, ZipArchiveMode.Read);

        T Ler<T>(string nome)
        {
            var entrada = zip.GetEntry(nome) ?? throw new InvalidDataException($"O arquivo de backup não tem {nome}.");
            using var s = entrada.Open();
            return JsonSerializer.Deserialize<T>(s, Json) ?? throw new InvalidDataException($"{nome} está vazio.");
        }

        var manifesto = Ler<Manifesto>("manifesto.json");
        if (manifesto.Versao != Versao)
            throw new InvalidDataException("Este backup foi feito por outra versão do site.");

        // Esquece o que já estava carregado (ex.: a conta de quem está restaurando), que tem os mesmos ids do backup
        db.ChangeTracker.Clear();

        await using var transacao = await db.Database.BeginTransactionAsync();

        // Banco novo pode ter só a funcionária criada pelas variáveis ADMIN_*: ela é substituída pelos dados do backup
        await db.Favoritos.ExecuteDeleteAsync();
        await db.Sacola.ExecuteDeleteAsync();
        await db.Repasses.ExecuteDeleteAsync();
        await db.Eventos.ExecuteDeleteAsync();
        await db.Usuarios.ExecuteDeleteAsync();

        async Task Inserir<T>(IEnumerable<T> linhas) where T : class
        {
            db.AddRange(linhas);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }

        await Inserir(Ler<List<Usuario>>("usuarios.json"));
        await Inserir(Ler<List<Produto>>("produtos.json"));
        await Inserir(Ler<List<Favorito>>("favoritos.json"));
        await Inserir(Ler<List<SacolaItem>>("sacola.json"));
        await Inserir(Ler<List<Pedido>>("pedidos.json"));
        await Inserir(Ler<List<PedidoItem>>("pedido_itens.json"));
        await Inserir(Ler<List<Evento>>("eventos.json"));
        await Inserir(Ler<List<Repasse>>("repasses.json"));

        foreach (var f in Ler<List<ProdutoFoto>>("fotos.json"))
        {
            using var s = zip.GetEntry($"fotos/{f.Id}")!.Open();
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            f.Dados = ms.ToArray();
            await Inserir([f]);
        }

        // No Postgres, os próximos ids continuam depois dos ids restaurados
        if (db.Database.IsNpgsql())
        {
            // Os nomes das tabelas são fixos aqui no código (nada vem do usuário), então não há risco de injeção
            foreach (var tabela in new[] { "Usuarios", "Produtos", "ProdutoFotos", "Pedidos", "PedidoItens", "Eventos", "Repasses" })
            {
                var sql = "SELECT setval(pg_get_serial_sequence('\"" + tabela + "\"', 'Id'), " +
                          "COALESCE((SELECT MAX(\"Id\") FROM \"" + tabela + "\"), 0) + 1, false)";
                await db.Database.ExecuteSqlRawAsync(sql);
            }
        }

        await transacao.CommitAsync();
    }
}
