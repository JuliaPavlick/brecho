using GarimpoChic.Data;
using GarimpoChic.Models;
using GarimpoChic.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Controllers;

public class ContaController(BrechoContext db) : Controller
{
    [HttpGet("/")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect(User.IsInRole(Papeis.Funcionaria) ? "/perfis" : "/inicio");
        return View();
    }

    const int MaxTentativas = 5;
    static readonly TimeSpan TempoBloqueio = TimeSpan.FromMinutes(15);

    [HttpPost("/")]
    [EnableRateLimiting(Limites.Autenticacao)]
    public async Task<IActionResult> Login(string? cpf, string? senha)
    {
        var digitos = Formato.SoDigitos(cpf);
        var resumo = Protecao.ResumoCpf(digitos);
        var u = resumo == null ? null : await db.Usuarios.FirstOrDefaultAsync(x => x.CpfResumo == resumo);
        ViewData["Cpf"] = cpf;

        // Conta bloqueada: nem confere a senha, para não dar mais chances de adivinhar
        if (u?.BloqueadoAte > DateTime.UtcNow)
        {
            var minutos = (int)Math.Ceiling((u.BloqueadoAte.Value - DateTime.UtcNow).TotalMinutes);
            ViewData["Erro"] = $"Muitas tentativas erradas. Tente de novo em {minutos} min ou use \"Esqueci minha senha\".";
            return View();
        }

        if (u == null || !DadosIniciais.SenhaConfere(u, senha ?? ""))
        {
            if (u != null)
            {
                u.TentativasFalhas++;
                if (u.TentativasFalhas >= MaxTentativas)
                {
                    u.BloqueadoAte = DateTime.UtcNow + TempoBloqueio;
                    u.TentativasFalhas = 0;
                }
                await db.SaveChangesAsync();
            }

            ViewData["Erro"] = "CPF ou senha incorretos.";
            return View();
        }

        if (u.TentativasFalhas > 0 || u.BloqueadoAte != null)
        {
            u.TentativasFalhas = 0;
            u.BloqueadoAte = null;
            await db.SaveChangesAsync();
        }

        await Sessao.EntrarAsync(HttpContext, u);
        // Só a funcionária tem mais de uma área para escolher; as demais pessoas vão direto para a loja
        return Redirect(u.EhFuncionaria ? "/perfis" : "/inicio");
    }

    [HttpGet("/cadastro")]
    public IActionResult Cadastro() => View(new CadastroForm());

    [HttpPost("/cadastro")]
    [EnableRateLimiting(Limites.Autenticacao)]
    public async Task<IActionResult> Cadastro(CadastroForm form)
    {
        if (Limites.CadastroBloqueado(HttpContext))
        {
            ModelState.AddModelError("cpf", "Muitas tentativas com CPFs já cadastrados. Aguarde uma hora e tente de novo.");
            return View(form);
        }

        var cpf = Formato.SoDigitos(form.Cpf);
        var nome = (form.Nome ?? "").Trim();

        if (nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            ModelState.AddModelError("nome", "Informe seu nome completo.");
        if (!Formato.CpfValido(cpf))
            ModelState.AddModelError("cpf", "Informe um CPF válido.");
        else if (await db.Usuarios.AnyAsync(u => u.CpfResumo == Protecao.ResumoCpf(cpf)))
        {
            Limites.RegistrarCpfRepetido(HttpContext);
            ModelState.AddModelError("cpf", "Já existe uma conta com este CPF. Entre ou use \"Esqueci minha senha\".");
        }
        if (!Formato.EmailValido(form.Email))
            ModelState.AddModelError("email", "Informe um e-mail válido.");
        if (Formato.SoDigitos(form.Telefone).Length < 10)
            ModelState.AddModelError("telefone", "Informe um telefone válido.");
        if ((form.Senha ?? "").Length < 8)
            ModelState.AddModelError("senha", "Use pelo menos 8 caracteres.");
        if (!form.Aceito)
            ModelState.AddModelError("aceito", "Para criar a conta, é preciso concordar com a Política de Privacidade.");

        if (!ModelState.IsValid) return View(form);

        var u = new Usuario
        {
            Nome = nome,
            Cpf = cpf,
            Email = form.Email!.Trim(),
            Telefone = form.Telefone!.Trim(),
            AceitouPrivacidadeEm = DateTime.UtcNow,
        };
        u.SenhaHash = DadosIniciais.HashSenha(u, form.Senha!);

        db.Usuarios.Add(u);
        await db.SaveChangesAsync();

        await Sessao.EntrarAsync(HttpContext, u);
        return Redirect("/inicio");
    }

    [HttpGet("/privacidade")]
    public IActionResult Privacidade() => View();

    [HttpGet("/recuperar-senha")]
    public IActionResult RecuperarSenha() => View();

    // Manda o link de redefinição para o e-mail da conta. A resposta é sempre a mesma,
    // para a tela não revelar quais CPFs estão cadastrados.
    [HttpPost("/recuperar-senha")]
    [EnableRateLimiting(Limites.Autenticacao)]
    public async Task<IActionResult> RecuperarSenha(string? cpf, [FromServices] RedefinicaoSenha redefinicao,
        [FromServices] Email email, [FromServices] ILogger<ContaController> log)
    {
        var digitos = Formato.SoDigitos(cpf);
        var u = Formato.CpfValido(digitos) ? await db.Usuarios.FirstOrDefaultAsync(x => x.CpfResumo == Protecao.ResumoCpf(digitos)) : null;

        if (u != null && Formato.EmailValido(u.Email))
        {
            var link = $"{EnderecoDoSite()}/redefinir-senha?codigo={Uri.EscapeDataString(redefinicao.GerarCodigo(u))}";
            var html = $"""
                <p>Olá, {System.Net.WebUtility.HtmlEncode(u.PrimeiroNome)}!</p>
                <p>Recebemos um pedido para redefinir a senha da sua conta no Garimpo Chic.</p>
                <p><a href="{link}">Clique aqui para criar uma nova senha</a>. O link vale por 1 hora.</p>
                <p>Se não foi você, é só ignorar este e-mail: sua senha continua a mesma.</p>
                """;

            try
            {
                await email.EnviarAsync(u.Email, "Redefinir sua senha — Garimpo Chic", html);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Falha ao enviar e-mail de redefinição de senha");
            }
        }

        ViewData["Enviado"] = Formato.CpfValido(digitos);
        return View();
    }

    // Endereço público do site para montar o link do e-mail.
    // Na Render vem de RENDER_EXTERNAL_URL; não confiamos no cabeçalho Host, que pode ser forjado.
    string EnderecoDoSite() =>
        (Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL")
         ?? HttpContext.RequestServices.GetRequiredService<IConfiguration>()["SITE_URL"]
         ?? $"{Request.Scheme}://{Request.Host}").TrimEnd('/');

    [HttpGet("/redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha(string? codigo, [FromServices] RedefinicaoSenha redefinicao)
    {
        ViewData["Codigo"] = codigo;
        ViewData["Valido"] = await CodigoValidoAsync(codigo, redefinicao) != null;
        return View();
    }

    [HttpPost("/redefinir-senha")]
    [EnableRateLimiting(Limites.Autenticacao)]
    public async Task<IActionResult> RedefinirSenha(string? codigo, string? senha, [FromServices] RedefinicaoSenha redefinicao)
    {
        var u = await CodigoValidoAsync(codigo, redefinicao);
        ViewData["Codigo"] = codigo;
        ViewData["Valido"] = u != null;

        if (u == null) return View();

        if ((senha ?? "").Length < 8)
        {
            ViewData["Erro"] = "Use pelo menos 8 caracteres.";
            return View();
        }

        u.SenhaHash = DadosIniciais.HashSenha(u, senha!);
        u.TentativasFalhas = 0;
        u.BloqueadoAte = null;
        await db.SaveChangesAsync();

        TempData["Ok"] = "Senha alterada! Agora é só entrar com a nova senha.";
        return Redirect("/");
    }

    async Task<Usuario?> CodigoValidoAsync(string? codigo, RedefinicaoSenha redefinicao)
    {
        var id = redefinicao.LerUsuarioId(codigo);
        var u = id == null ? null : await db.Usuarios.FindAsync(id.Value);
        return u != null && redefinicao.CodigoConfere(codigo, u) ? u : null;
    }

    [Authorize]
    [HttpGet("/perfis")]
    public async Task<IActionResult> SelecaoPerfil()
    {
        var u = await db.UsuarioAtualAsync(User);
        if (u == null) return await Sair();
        return View(u);
    }

    [HttpGet("/sair")]
    public async Task<IActionResult> Sair()
    {
        await Sessao.SairAsync(HttpContext);
        return Redirect("/");
    }

    // Chamada pelo tratador de erros também em POSTs, por isso aceita qualquer método
    [Route("/erro")]
    [IgnoreAntiforgeryToken]
    public IActionResult Erro() => Content("Ops! Algo deu errado. Tente novamente em instantes.");
}

public class CadastroForm
{
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Senha { get; set; }
    public bool Aceito { get; set; }
}
