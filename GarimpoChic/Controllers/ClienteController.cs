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
        ViewData["QtdSacola"] = await db.Sacola.CountAsync(s => s.UsuarioId == u.Id);
        await next();
    }

    IQueryable<ProdutoCard> Cards(IQueryable<Produto> q) =>
        q.Select(p => new ProdutoCard(
            p.Id, p.Nome, p.Descricao, p.Categoria, p.Preco, p.Status,
            p.Fotos.OrderBy(f => f.Id).Select(f => (int?)f.Id).FirstOrDefault(),
            db.Favoritos.Any(f => f.UsuarioId == Eu.Id && f.ProdutoId == p.Id),
            db.Sacola.Any(s => s.UsuarioId == Eu.Id && s.ProdutoId == p.Id)));

    IQueryable<Produto> Disponiveis => db.Produtos.Where(p => p.Status == StatusProduto.Disponivel);

    [HttpGet("/inicio")]
    public async Task<IActionResult> Inicio() =>
        View(await Cards(Disponiveis.OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id)).Take(3).ToListAsync());

    [HttpGet("/explorar")]
    public async Task<IActionResult> Explorar(string? q, string? categoria, string? ordem, int pagina = 1)
    {
        const int PorPagina = 24;

        // Busca, filtro e ordem são feitos no banco: a página só recebe as peças que vai mostrar
        var consulta = Disponiveis;

        var busca = (q ?? "").Trim();
        if (busca != "")
        {
            var termo = busca.ToLower();
            consulta = consulta.Where(p => p.Nome.ToLower().Contains(termo));
        }

        var cat = Categorias.Todas.FirstOrDefault(c => c.Slug == categoria);
        if (cat.Nome != null) consulta = consulta.Where(p => p.Categoria == cat.Nome);

        // (double): o SQLite do desenvolvimento não ordena decimal
        consulta = ordem switch
        {
            "menor" => consulta.OrderBy(p => (double)p.Preco).ThenBy(p => p.Id),
            "maior" => consulta.OrderByDescending(p => (double)p.Preco).ThenBy(p => p.Id),
            _ => consulta.OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id),
        };

        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)PorPagina));
        pagina = Math.Clamp(pagina, 1, paginas);
        var pecas = await Cards(consulta.Skip((pagina - 1) * PorPagina).Take(PorPagina)).ToListAsync();

        return View(new ExplorarPagina(pecas, busca, cat.Slug ?? "todos", ordem is "menor" or "maior" ? ordem : "relevancia",
            pagina, paginas, total));
    }

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

    // ---------- Sacola ----------

    [HttpGet("/sacola")]
    public async Task<IActionResult> Sacola()
    {
        var ids = db.Sacola.Where(s => s.UsuarioId == Eu.Id).Select(s => s.ProdutoId);
        return View(await Cards(db.Produtos.Where(p => ids.Contains(p.Id))).ToListAsync());
    }

    // Coloca ou tira uma peça da sacola (botão dos cards)
    [HttpPost("/sacola/{id:int}")]
    public async Task<IActionResult> AlternarSacola(int id)
    {
        var produto = await db.Produtos.FindAsync(id);
        if (produto == null) return NotFound();

        var item = await db.Sacola.FindAsync(Eu.Id, id);
        if (item != null) db.Sacola.Remove(item);
        else if (produto.Status != StatusProduto.Disponivel) return Conflict(new { erro = "Esta peça não está mais disponível." });
        else db.Sacola.Add(new SacolaItem { UsuarioId = Eu.Id, ProdutoId = id });

        await db.SaveChangesAsync();
        return Json(new
        {
            naSacola = item == null,
            total = await db.Sacola.CountAsync(s => s.UsuarioId == Eu.Id),
        });
    }

    [HttpPost("/sacola/{id:int}/remover")]
    public async Task<IActionResult> RemoverDaSacola(int id)
    {
        await db.Sacola.Where(s => s.UsuarioId == Eu.Id && s.ProdutoId == id).ExecuteDeleteAsync();
        return Redirect("/sacola");
    }

    // Transforma a sacola em pedido. Cada peça é única: ela só entra no pedido se ainda estiver
    // disponível no momento da compra, e passa a "vendido" na mesma operação.
    [HttpPost("/sacola/finalizar")]
    public async Task<IActionResult> Finalizar()
    {
        var itens = await db.Sacola.Include(s => s.Produto)
            .Where(s => s.UsuarioId == Eu.Id).ToListAsync();
        var pecas = itens.Select(s => s.Produto!).Where(p => p.Status == StatusProduto.Disponivel).ToList();

        if (pecas.Count == 0)
        {
            TempData["Erro"] = "Não há peças disponíveis na sua sacola.";
            return Redirect("/sacola");
        }

        await using var transacao = await db.Database.BeginTransactionAsync();

        var ids = pecas.Select(p => p.Id).ToList();
        var vendidas = await db.Produtos
            .Where(p => ids.Contains(p.Id) && p.Status == StatusProduto.Disponivel)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, StatusProduto.Vendido));

        // Outra pessoa comprou alguma dessas peças entre abrir a sacola e finalizar
        if (vendidas != ids.Count)
        {
            await transacao.RollbackAsync();
            TempData["Erro"] = "Uma das peças acabou de ser vendida. Confira sua sacola e tente de novo.";
            return Redirect("/sacola");
        }

        var pedido = new Pedido
        {
            ClienteId = Eu.Id,
            Itens = pecas.Select(p => new PedidoItem
            {
                ProdutoId = p.Id, Nome = p.Nome, Descricao = p.Descricao, Categoria = p.Categoria, Preco = p.Preco,
            }).ToList(),
        };
        db.Pedidos.Add(pedido);
        db.Sacola.RemoveRange(itens.Where(s => ids.Contains(s.ProdutoId)));

        await db.SaveChangesAsync();
        await transacao.CommitAsync();

        TempData["PedidoNovo"] = pedido.Numero;
        return Redirect("/compras");
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

    // LGPD (art. 18): a pessoa pode pedir uma cópia de tudo o que a loja guarda sobre ela
    [HttpGet("/perfil/meus-dados")]
    public async Task<IActionResult> MeusDados()
    {
        var dados = new
        {
            geradoEm = DateTime.UtcNow,
            conta = new
            {
                Eu.Nome, cpf = Formato.Cpf(Eu.Cpf), Eu.Email, Eu.Telefone, Eu.Cidade, Eu.Estado,
                vendedora = Eu.EhVendedora, funcionaria = Eu.EhFuncionaria,
                Eu.CriadoEm, Eu.AceitouPrivacidadeEm,
            },
            pedidos = await db.Pedidos.Where(p => p.ClienteId == Eu.Id).OrderBy(p => p.Data)
                .Select(p => new { p.Id, p.Data, p.Status, itens = p.Itens.Select(i => new { i.Nome, i.Categoria, i.Preco }) })
                .ToListAsync(),
            favoritos = await db.Favoritos.Where(f => f.UsuarioId == Eu.Id)
                .Select(f => new { f.Produto!.Nome, f.CriadoEm }).ToListAsync(),
            sacola = await db.Sacola.Where(s => s.UsuarioId == Eu.Id)
                .Select(s => new { s.Produto!.Nome, s.CriadoEm }).ToListAsync(),
            pecasEnviadasParaVenda = await db.Produtos.Where(p => p.VendedoraId == Eu.Id)
                .Select(p => new { p.Nome, p.Descricao, p.Categoria, p.Preco, p.Status, p.CriadoEm, fotos = p.Fotos.Count })
                .ToListAsync(),
            repassesRecebidos = await db.Repasses.Where(r => r.VendedoraId == Eu.Id)
                .Select(r => new { r.Data, r.Valor }).ToListAsync(),
        };

        var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dados, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        return File(json, "application/json", "meus-dados-garimpo-chic.json");
    }

    // LGPD: exclusão da conta. Os dados pessoais são apagados; os pedidos continuam no financeiro da loja,
    // mas ligados a uma conta sem nome, CPF, e-mail ou telefone (dado anonimizado deixa de ser dado pessoal).
    [HttpPost("/perfil/excluir")]
    public async Task<IActionResult> ExcluirConta(string? senha)
    {
        string? impedimento =
            !DadosIniciais.SenhaConfere(Eu, senha ?? "") ? "A senha não confere."
            : await db.Pedidos.AnyAsync(p => p.ClienteId == Eu.Id && p.Status != StatusPedido.Entregue)
                ? "Você tem pedidos que ainda não foram entregues. Depois da entrega, é só voltar aqui."
            : await db.Produtos.AnyAsync(p => p.VendedoraId == Eu.Id && p.Status != StatusProduto.Vendido)
                ? "Você ainda tem peças na loja. Combine com a loja a retirada delas antes de excluir a conta."
            : Eu.EhFuncionaria && !await db.Usuarios.AnyAsync(u => u.EhFuncionaria && u.Id != Eu.Id)
                ? "Você é a única funcionária. Dê acesso de funcionária a outra pessoa antes de excluir sua conta."
            : null;

        if (impedimento != null)
        {
            TempData["ErroExcluir"] = impedimento;
            return Redirect("/perfil#excluir");
        }

        await db.Favoritos.Where(f => f.UsuarioId == Eu.Id).ExecuteDeleteAsync();
        await db.Sacola.Where(s => s.UsuarioId == Eu.Id).ExecuteDeleteAsync();

        Eu.Nome = "Conta excluída";
        Eu.Cpf = ""; // também apaga o resumo usado no login
        Eu.Email = "";
        Eu.Telefone = "";
        Eu.Cidade = null;
        Eu.Estado = null;
        Eu.SenhaHash = "";
        Eu.EhVendedora = false;
        Eu.EhFuncionaria = false;
        Eu.TentativasFalhas = 0;
        Eu.BloqueadoAte = null;
        Eu.AceitouPrivacidadeEm = null;
        Eu.ExcluidaEm = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await Sessao.SairAsync(HttpContext);
        TempData["Ok"] = "Sua conta foi excluída e seus dados pessoais foram apagados.";
        return Redirect("/");
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
            // Os outros aparelhos saem sozinhos (o cookie deles tem o carimbo da senha antiga); este continua logado
            await Sessao.EntrarAsync(HttpContext, Eu);
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

        // O tipo é conferido de novo pelos bytes: fotos antigas, salvas antes desta verificação, podem ser SVG
        var tipo = foto == null ? null : Fotos.DetectarTipo(foto.Dados);
        if (tipo == null) return NotFound();

        // Mesmo que algo escape da verificação, o navegador não executa nada vindo deste endereço
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "default-src 'none'; sandbox";

        return File(foto!.Dados, tipo);
    }
}
