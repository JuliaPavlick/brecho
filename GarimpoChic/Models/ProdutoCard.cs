namespace GarimpoChic.Models;

// O que os cards de peça da área do cliente precisam mostrar
public record ProdutoCard(int Id, string Nome, string Descricao, string Categoria, decimal Preco, int? FotoId, bool Favorito)
{
    public string Emoji => Categorias.Emoji(Categoria);
    public string CategoriaSlug => Categorias.Slug(Categoria);
}
