namespace GarimpoChic.Models;

// O que os cards de peça da área do cliente precisam mostrar
public record ProdutoCard(int Id, string Nome, string Descricao, string Categoria, decimal Preco, string Status,
    int? FotoId, bool Favorito, bool NaSacola)
{
    public string Emoji => Categorias.Emoji(Categoria);
    public string CategoriaSlug => Categorias.Slug(Categoria);
    public bool Disponivel => Status == StatusProduto.Disponivel;
}
