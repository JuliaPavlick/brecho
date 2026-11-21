using System.ComponentModel.DataAnnotations;

namespace GarimpoChic.Models;

public class Pedido
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Usuario? Cliente { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = StatusPedido.Preparo;

    public List<PedidoItem> Itens { get; set; } = [];

    public decimal Total => Itens.Sum(i => i.Preco);

    public string Numero => "#" + Id.ToString("0000");
}

// Guarda uma cópia do nome e do preço: o pedido não muda se a peça for editada ou excluída depois
public class PedidoItem
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    [MaxLength(120)]
    public string Nome { get; set; } = "";

    [MaxLength(500)]
    public string Descricao { get; set; } = "";

    [MaxLength(30)]
    public string Categoria { get; set; } = "";

    public decimal Preco { get; set; }

    // Peça de vendedora externa: quem é ela e o percentual combinado no dia da venda.
    // Fica copiado aqui para que mudar o percentual depois não altere o que já foi vendido.
    public int? VendedoraId { get; set; }

    public decimal? PercentualVendedora { get; set; }

    // Quanto a vendedora recebe por esta peça (zero se a peça é da própria loja)
    public decimal ValorVendedora => PercentualVendedora is decimal pct ? Util.Comissao.Valor(Preco, pct) : 0;

    public string Emoji => Categorias.Emoji(Categoria);
}

public static class StatusPedido
{
    public const string Preparo = "preparo";
    public const string Caminho = "caminho";
    public const string Entregue = "entregue";
    public const string Cancelado = "cancelado";

    public static readonly Dictionary<string, string> Nomes = new()
    {
        [Preparo] = "Em preparo",
        [Caminho] = "A caminho",
        [Entregue] = "Entregue",
        [Cancelado] = "Cancelado",
    };

    // Entregue e cancelado são finais: depois deles o status não muda mais
    public static bool Final(string status) => status is Entregue or Cancelado;

    public static bool PodeMudar(string de, string para) => Nomes.ContainsKey(para) && (de == para || !Final(de));
}
