using GarimpoChic.Data;
using GarimpoChic.Models;
using GarimpoChic.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Controllers;

// Área administrativa: só quem tem o papel de funcionária entra aqui
[Authorize(Roles = Papeis.Funcionaria)]
[Route("painel")]
public class FuncionariaController(BrechoContext db) : Controller
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var u = await db.UsuarioAtualAsync(User);
        if (u == null || !u.EhFuncionaria)
        {
            ctx.Result = Redirect("/sair");
            return;
        }

        ViewData["Usuario"] = u;
        await next();
    }

    // ---------- Dashboard ----------

    [HttpGet("")]
    public async Task<IActionResult> Dashboard()
    {
        var rel = await Relatorios.CarregarAsync(db);
        var inicioMes = Formato.ParaUtc(new DateTime(Formato.Agora.Year, Formato.Agora.Month, 1));

        ViewData["Relatorios"] = rel;
        ViewData["ProdutosAtivos"] = await db.Produtos.CountAsync(p => p.Status == StatusProduto.Disponivel);
        ViewData["NovasVendedoras"] = await db.Usuarios.CountAsync(u => u.EhVendedora && u.CriadoEm >= inicioMes);
        ViewData["Eventos"] = await db.Eventos.Where(e => e.Data >= Formato.Hoje)
            .OrderBy(e => e.Data).ThenBy(e => e.Hora).Take(4).ToListAsync();

        return View();
    }

    // ---------- Produtos ----------

    object ProdutoJson(Produto p) => new
    {
        id = p.Id,
        nome = p.Nome,
        descricao = p.Descricao,
        categoria = p.Categoria,
        preco = p.Preco,
        status = p.Status,
        vendedora = p.Vendedora?.Nome,
        foto = p.Fotos.OrderBy(f => f.Id).Select(f => "/fotos/" + f.Id).FirstOrDefault(),
    };

    // Carrega a peça com os ids das fotos, sem trazer as imagens em si
    Task<Produto?> BuscarProduto(int id) =>
        db.Produtos.Include(p => p.Vendedora)
            .Select(p => new Produto
            {
                Id = p.Id, Nome = p.Nome, Descricao = p.Descricao, Categoria = p.Categoria, Preco = p.Preco,
                Status = p.Status, Vendedora = p.Vendedora, CriadoEm = p.CriadoEm,
                Fotos = p.Fotos.Select(f => new ProdutoFoto { Id = f.Id }).ToList(),
            })
            .FirstOrDefaultAsync(p => p.Id == id);

    [HttpGet("produtos")]
    public async Task<IActionResult> Produtos()
    {
        var produtos = await db.Produtos.Include(p => p.Vendedora)
            .OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id)
            .Select(p => new Produto
            {
                Id = p.Id, Nome = p.Nome, Descricao = p.Descricao, Categoria = p.Categoria, Preco = p.Preco,
                Status = p.Status, Vendedora = p.Vendedora,
                Fotos = p.Fotos.Select(f => new ProdutoFoto { Id = f.Id }).ToList(),
            })
            .ToListAsync();

        return View(produtos.Select(ProdutoJson).ToList());
    }

    public class ProdutoForm
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Categoria { get; set; }
        public string? Preco { get; set; }
        public string? Status { get; set; }
        public List<IFormFile>? Fotos { get; set; }
    }

    [HttpPost("produtos")]
    public Task<IActionResult> CriarProduto([FromForm] ProdutoForm form) => SalvarProduto(null, form);

    [HttpPost("produtos/{id:int}")]
    public Task<IActionResult> EditarProduto(int id, [FromForm] ProdutoForm form) => SalvarProduto(id, form);

    async Task<IActionResult> SalvarProduto(int? id, ProdutoForm form)
    {
        var preco = Precos.Ler(form.Preco);
        if (string.IsNullOrWhiteSpace(form.Nome) || preco <= 0 ||
            !Categorias.Existe(form.Categoria ?? "") || !StatusProduto.Nomes.ContainsKey(form.Status ?? ""))
            return BadRequest(new { erro = "Dados inválidos." });

        Produto? p;
        if (id == null)
        {
            p = new Produto();
            db.Produtos.Add(p);
        }
        else
        {
            p = await db.Produtos.FindAsync(id);
            if (p == null) return NotFound();
        }

        p.Nome = form.Nome.Trim();
        p.Descricao = (form.Descricao ?? "").Trim();
        p.Categoria = form.Categoria!;
        p.Preco = preco;
        p.Status = form.Status!;

        // Fotos novas substituem as antigas
        var novas = await Fotos.LerAsync(form.Fotos);
        if (novas.Count > 0)
        {
            if (id != null) await db.ProdutoFotos.Where(f => f.ProdutoId == id).ExecuteDeleteAsync();
            p.Fotos.AddRange(novas);
        }

        await db.SaveChangesAsync();
        return Json(ProdutoJson((await BuscarProduto(p.Id))!));
    }

    [HttpPost("produtos/{id:int}/excluir")]
    public async Task<IActionResult> ExcluirProduto(int id)
    {
        await db.Produtos.Where(p => p.Id == id).ExecuteDeleteAsync();
        return Ok();
    }

    // ---------- Pedidos ----------

    [HttpGet("pedidos")]
    public async Task<IActionResult> Pedidos()
    {
        var pedidos = await db.Pedidos.Include(p => p.Itens).Include(p => p.Cliente)
            .OrderByDescending(p => p.Data).ToListAsync();

        return View(pedidos.Select(p => new
        {
            id = p.Id,
            cliente = p.Cliente?.Nome ?? "—",
            data = Formato.Data(p.Data),
            status = p.Status,
            itens = p.Itens.Select(i => new { nome = i.Nome, preco = i.Preco }),
        }).ToList());
    }

    public record StatusForm(string Status);

    [HttpPost("pedidos/{id:int}/status")]
    public async Task<IActionResult> AlterarStatusPedido(int id, [FromBody] StatusForm form)
    {
        if (!StatusPedido.Nomes.ContainsKey(form.Status)) return BadRequest();

        var linhas = await db.Pedidos.Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, form.Status));

        return linhas == 0 ? NotFound() : Ok();
    }

    // ---------- Calendário ----------

    static object EventoJson(Evento e) => new
    {
        id = e.Id,
        data = e.Data.ToString("yyyy-MM-dd"),
        hora = e.Hora.ToString("HH:mm"),
        titulo = e.Titulo,
        tipo = e.Tipo,
    };

    [HttpGet("calendario")]
    public async Task<IActionResult> Calendario() =>
        View((await db.Eventos.ToListAsync()).Select(EventoJson).ToList());

    public record EventoForm(string Titulo, string Data, string? Hora, string Tipo);

    [HttpPost("calendario")]
    public async Task<IActionResult> CriarEvento([FromBody] EventoForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Titulo) || !DateOnly.TryParse(form.Data, out var data) ||
            !TipoEvento.Todos.Contains(form.Tipo))
            return BadRequest();

        var evento = new Evento
        {
            Titulo = form.Titulo.Trim(),
            Data = data,
            Hora = TimeOnly.TryParse(form.Hora, out var hora) ? hora : new TimeOnly(0, 0),
            Tipo = form.Tipo,
        };
        db.Eventos.Add(evento);
        await db.SaveChangesAsync();

        return Json(EventoJson(evento));
    }

    [HttpPost("calendario/{id:int}/excluir")]
    public async Task<IActionResult> ExcluirEvento(int id)
    {
        await db.Eventos.Where(e => e.Id == id).ExecuteDeleteAsync();
        return Ok();
    }

    // ---------- Financeiro ----------

    [HttpGet("financeiro")]
    public async Task<IActionResult> Financeiro()
    {
        var rel = await Relatorios.CarregarAsync(db);

        var desde = DateTime.UtcNow.AddDays(-60);
        var vendas = await db.Pedidos.Include(p => p.Itens).Where(p => p.Data >= desde).ToListAsync();

        var movimentacoes = vendas
            .Select(p => (data: p.Data, descricao: "Venda — " + string.Join(" e ", p.Itens.Select(i => i.Nome)), tipo: "venda", valor: p.Total))
            .Concat(rel.Repasses.Where(r => r.Data >= desde)
                .Select(r => (data: r.Data, descricao: "Repasse — " + (r.Vendedora?.Nome ?? "vendedora"), tipo: "repasse", valor: -r.Valor)))
            .OrderByDescending(m => m.data)
            .Take(30)
            .Select(m => new { data = Formato.Data(m.data), m.descricao, m.tipo, m.valor })
            .ToList();

        ViewData["Relatorios"] = rel;
        return View(movimentacoes);
    }

    // ---------- Backup ----------

    [HttpGet("backup")]
    public async Task<IActionResult> Backup()
    {
        ViewData["PodeRestaurar"] = await Util.Backup.PodeRestaurarAsync(db);
        ViewData["Totais"] = new (string, int)[]
        {
            ("Pessoas", await db.Usuarios.CountAsync()),
            ("Produtos", await db.Produtos.CountAsync()),
            ("Fotos", await db.ProdutoFotos.CountAsync()),
            ("Pedidos", await db.Pedidos.CountAsync()),
            ("Eventos", await db.Eventos.CountAsync()),
        };
        return View();
    }

    // Gera o .zip num arquivo temporário (apagado assim que o download termina) e envia
    [HttpGet("backup/baixar")]
    public async Task<IActionResult> BaixarBackup()
    {
        var caminho = Path.GetTempFileName();
        await using (var arquivo = System.IO.File.Create(caminho))
            await Util.Backup.ExportarAsync(db, arquivo);

        var leitura = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.DeleteOnClose);
        return File(leitura, "application/zip", $"garimpo-backup-{Formato.Agora:yyyy-MM-dd-HHmm}.zip");
    }

    [HttpPost("backup/restaurar")]
    [RequestSizeLimit(1_000_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 1_000_000_000)]
    public async Task<IActionResult> RestaurarBackup(IFormFile? arquivo)
    {
        if (!await Util.Backup.PodeRestaurarAsync(db))
        {
            TempData["Erro"] = "Este banco já tem produtos ou pedidos. A restauração só funciona num banco novo.";
            return Redirect("/painel/backup");
        }
        if (arquivo == null || arquivo.Length == 0)
        {
            TempData["Erro"] = "Escolha o arquivo .zip do backup.";
            return Redirect("/painel/backup");
        }

        try
        {
            await using var s = arquivo.OpenReadStream();
            await Util.Backup.RestaurarAsync(db, s);
        }
        catch (Exception ex) when (ex is InvalidDataException or System.Text.Json.JsonException)
        {
            TempData["Erro"] = "Não foi possível ler este arquivo de backup: " + ex.Message;
            return Redirect("/painel/backup");
        }

        // As contas foram trocadas pelas do backup: todos (inclusive quem restaurou) entram de novo
        await Sessao.SairAsync(HttpContext);
        TempData["Ok"] = "Backup restaurado! Entre com uma conta que existia no backup.";
        return Redirect("/");
    }

    // ---------- Clientes ----------

    [HttpGet("clientes")]
    public async Task<IActionResult> Clientes()
    {
        var pessoas = await db.Usuarios.OrderBy(u => u.Nome).ToListAsync();
        var pedidos = await db.Pedidos.Include(p => p.Itens).ToListAsync();
        var enviadas = await db.Produtos.Where(p => p.VendedoraId != null)
            .GroupBy(p => p.VendedoraId).Select(g => new { g.Key, Total = g.Count() })
            .ToDictionaryAsync(g => g.Key!.Value, g => g.Total);

        return View(pessoas.Select(u =>
        {
            var meus = pedidos.Where(p => p.ClienteId == u.Id).ToList();
            var papeis = new List<string> { "cliente" };
            if (u.EhVendedora) papeis.Add("vendedora");
            if (u.EhFuncionaria) papeis.Add("funcionaria");

            return new
            {
                id = u.Id,
                nome = u.Nome,
                email = u.Email,
                telefone = u.Telefone,
                cpf = Formato.CpfMascarado(u.Cpf),
                cidade = u.Cidade ?? "—",
                desde = Formato.Data(u.CriadoEm),
                papeis,
                pedidos = meus.Count,
                gasto = meus.Sum(p => p.Total),
                pecasEnviadas = enviadas.GetValueOrDefault(u.Id),
            };
        }).ToList());
    }
}
