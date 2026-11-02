using System.ComponentModel.DataAnnotations;

namespace GarimpoChic.Models;

public class Produto
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Nome { get; set; } = "";

    [MaxLength(500)]
    public string Descricao { get; set; } = "";

    [MaxLength(30)]
    public string Categoria { get; set; } = Categorias.Roupas;

    public decimal Preco { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = StatusProduto.Avaliacao;

    // Preenchido quando a peça veio de uma vendedora externa
    public int? VendedoraId { get; set; }
    public Usuario? Vendedora { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public List<ProdutoFoto> Fotos { get; set; } = [];

    public string Emoji => Categorias.Emoji(Categoria);
}

public class ProdutoFoto
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    [MaxLength(100)]
    public string ContentType { get; set; } = "image/jpeg";

    // A Render apaga o disco a cada deploy, então a foto fica no próprio banco
    public byte[] Dados { get; set; } = [];
}

public static class StatusProduto
{
    public const string Disponivel = "disponivel";
    public const string Reservado = "reservado";
    public const string Avaliacao = "avaliacao";
    public const string Vendido = "vendido";

    public static readonly Dictionary<string, string> Nomes = new()
    {
        [Disponivel] = "Disponível",
        [Reservado] = "Reservado",
        [Avaliacao] = "Em avaliação",
        [Vendido] = "Vendido",
    };
}

public static class Categorias
{
    public const string Roupas = "Roupas";

    // Nome exibido, identificador usado nos filtros e emoji usado quando a peça não tem foto
    public static readonly (string Nome, string Slug, string Emoji)[] Todas =
    [
        ("Roupas", "roupas", "👚"),
        ("Vestidos", "vestidos", "👗"),
        ("Bolsas", "bolsas", "👜"),
        ("Calçados", "calcados", "👠"),
        ("Acessórios", "acessorios", "👒"),
    ];

    public static bool Existe(string nome) => Todas.Any(c => c.Nome == nome);

    public static string Slug(string nome) => Todas.FirstOrDefault(c => c.Nome == nome).Slug ?? "";

    public static string Emoji(string nome) => Todas.FirstOrDefault(c => c.Nome == nome).Emoji ?? "✦";
}
