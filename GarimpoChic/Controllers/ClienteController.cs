using GarimpoChic.Data;
using GarimpoChic.Models;
using GarimpoChic.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Controllers;

[Authorize]
public class ClienteController(BrechoContext db) : Controller
{
    Usuario Eu = null!;

    // Carrega a pessoa logada antes de cada ação (o layout usa o nome e a inicial dela)
    public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var u = await db.UsuarioAtualAsync(User);
        if (u == null)
        {
            ctx.Result = Redirect("/sair");
            return;
        }

        Eu = u;
        ViewData["Usuario"] = u;
        await next();
    }

    IQueryable<ProdutoCard> Cards(IQueryable<Produto> q) =>
        q.Select(p => new ProdutoCard(
            p.Id, p.Nome, p.Descricao, p.Categoria, p.Preco,
            p.Fotos.OrderBy(f => f.Id).Select(f => (int?)f.Id).FirstOrDefault(),
            db.Favoritos.Any(f => f.UsuarioId == Eu.Id && f.ProdutoId == p.Id)));

    IQueryable<Produto> Disponiveis => db.Produtos.Where(p => p.Status == StatusProduto.Disponivel);

    [HttpGet("/inicio")]
    public async Task<IActionResult> Inicio() =>
        View(await Cards(Disponiveis.OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id)).Take(3).ToListAsync());

    [HttpGet("/explorar")]
    public async Task<IActionResult> Explorar() =>
        View(await Cards(Disponiveis.OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id)).ToListAsync());

    [HttpGet("/favoritos")]
    public async Task<IActionResult> Favoritos()
    {
        var ids = db.Favoritos.Where(f => f.UsuarioId == Eu.Id).Select(f => f.ProdutoId);
        return View(await Cards(db.Produtos.Where(p => ids.Contains(p.Id))).ToListAsync());
    }

    // Marca ou desmarca o coração de uma peça
    [HttpPost("/favoritos/{id:int}")]
    public async Task<IActionResult> AlternarFavorito(int id)
    {
        if (!await db.Produtos.AnyAsync(p => p.Id == id)) return NotFound();

        var fav = await db.Favoritos.FindAsync(Eu.Id, id);
        if (fav == null) db.Favoritos.Add(new Favorito { UsuarioId = Eu.Id, ProdutoId = id });
        else db.Favoritos.Remove(fav);

        await db.SaveChangesAsync();
        return Json(new { favorito = fav == null });
    }

    [HttpGet("/compras")]
    public async Task<IActionResult> Compras() =>
        View(await db.Pedidos.Include(p => p.Itens)
            .Where(p => p.ClienteId == Eu.Id)
            .OrderByDescending(p => p.Data).ToListAsync());

    [HttpGet("/perfil")]
    public async Task<IActionResult> Perfil()
    {
        ViewData["TotalPedidos"] = await db.Pedidos.CountAsync(p => p.ClienteId == Eu.Id);
        ViewData["TotalFavoritos"] = await db.Favoritos.CountAsync(f => f.UsuarioId == Eu.Id);
        return View(Eu);
    }

    [HttpPost("/perfil/dados")]
    public async Task<IActionResult> SalvarDados(string? nome, string? email, string? telefone, string? cidade, string? estado)
    {
        if (string.IsNullOrWhiteSpace(nome) || !Formato.EmailValido(email) || Formato.SoDigitos(telefone).Length < 10)
        {
            TempData["ErroDados"] = "Confira nome, e-mail e telefone.";
            return Redirect("/perfil");
        }

        Eu.Nome = nome.Trim();
        Eu.Email = email!.Trim();
        Eu.Telefone = telefone!.Trim();
        Eu.Cidade = string.IsNullOrWhiteSpace(cidade) ? null : cidade.Trim();
        Eu.Estado = string.IsNullOrWhiteSpace(estado) ? null : estado.Trim().ToUpper();
        await db.SaveChangesAsync();

        // Atualiza o nome guardado no cookie
        await Sessao.EntrarAsync(HttpContext, Eu);

        TempData["OkDados"] = true;
        return Redirect("/perfil");
    }

    [HttpPost("/perfil/senha")]
    public async Task<IActionResult> AlterarSenha(string? atual, string? nova)
    {
        if (!DadosIniciais.SenhaConfere(Eu, atual ?? ""))
            TempData["ErroSenha"] = "A senha atual não confere.";
        else if ((nova ?? "").Length < 8)
            TempData["ErroSenha"] = "A nova senha precisa ter pelo menos 8 caracteres.";
        else
        {
            Eu.SenhaHash = DadosIniciais.HashSenha(Eu, nova!);
            await db.SaveChangesAsync();
            TempData["OkSenha"] = true;
        }

        return Redirect("/perfil");
    }

    [HttpGet("/vender")]
    public async Task<IActionResult> Vender() =>
        View(await db.Produtos.Where(p => p.VendedoraId == Eu.Id)
            .OrderByDescending(p => p.CriadoEm).ToListAsync());

    // A vendedora externa envia uma peça; ela entra como "Em avaliação" para a funcionária revisar
    [HttpPost("/vender")]
    public async Task<IActionResult> Vender(string? nome, string? descricao, string? categoria, string? preco, List<IFormFile>? fotos)
    {
        var valor = Precos.Ler(preco);
        if (string.IsNullOrWhiteSpace(nome) || !Categorias.Existe(categoria ?? "") || valor <= 0)
        {
            TempData["Erro"] = "Preencha nome, categoria e um preço maior que zero.";
            return Redirect("/vender");
        }

        var produto = new Produto
        {
            Nome = nome.Trim(),
            Descricao = (descricao ?? "").Trim(),
            Categoria = categoria!,
            Preco = valor,
            Status = StatusProduto.Avaliacao,
            VendedoraId = Eu.Id,
        };
        produto.Fotos.AddRange(await Fotos.LerAsync(fotos));
        db.Produtos.Add(produto);

        var virouVendedora = !Eu.EhVendedora;
        Eu.EhVendedora = true;
        await db.SaveChangesAsync();

        if (virouVendedora) await Sessao.EntrarAsync(HttpContext, Eu);

        TempData["Ok"] = true;
        return Redirect("/vender");
    }
}

// Fotos das peças são públicas, então ficam fora da área que exige login
public class FotosController(BrechoContext db) : Controller
{
    [HttpGet("/fotos/{id:int}")]
    [ResponseCache(Duration = 86400)]
    public async Task<IActionResult> Foto(int id)
    {
        var foto = await db.ProdutoFotos.FindAsync(id);
        return foto == null ? NotFound() : File(foto.Dados, foto.ContentType);
    }
}
