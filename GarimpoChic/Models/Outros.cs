using System.ComponentModel.DataAnnotations;

namespace GarimpoChic.Models;

public class Favorito
{
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

// Peça guardada na sacola. Não reserva a peça: ela só sai da loja quando a compra é finalizada.
public class SacolaItem
{
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

public class Evento
{
    public int Id { get; set; }

    public DateOnly Data { get; set; }

    public TimeOnly Hora { get; set; }

    [MaxLength(160)]
    public string Titulo { get; set; } = "";

    [MaxLength(20)]
    public string Tipo { get; set; } = TipoEvento.Outro;
}

public static class TipoEvento
{
    public const string Recebimento = "recebimento";
    public const string Avaliacao = "avaliacao";
    public const string Colecao = "colecao";
    public const string Outro = "outro";

    public static readonly string[] Todos = [Recebimento, Avaliacao, Colecao, Outro];
}

// Valor pago às vendedoras externas pelas peças vendidas
public class Repasse
{
    public int Id { get; set; }

    public int VendedoraId { get; set; }
    public Usuario? Vendedora { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
