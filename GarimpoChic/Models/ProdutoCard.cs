namespace GarimpoChic.Models;

// O que os cards de peça da área do cliente precisam mostrar
public record ProdutoCard(int Id, string Nome, string Descricao, string Categoria, decimal Preco, string Status,
    int? FotoId, bool Favorito, bool NaSacola)
{
    public string Emoji => Categorias.Emoji(Categoria);
    public string CategoriaSlug => Categorias.Slug(Categoria);
    public bool Disponivel => Status == StatusProduto.Disponivel;
}

// Uma página do Explorar, com os filtros escolhidos (para os links de página e de categoria manterem a busca)
public record ExplorarPagina(List<ProdutoCard> Pecas, string Busca, string Categoria, string Ordem,
    int Pagina, int Paginas, int Total);
