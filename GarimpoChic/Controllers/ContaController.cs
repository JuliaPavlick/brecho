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
    // Endereço principal: quem chega sem conta vê as peças da loja; quem já entrou vai para a sua área
    [HttpGet("/")]
    public IActionResult Raiz(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Destino(returnUrl, User.IsInRole(Papeis.Funcionaria));
        // Links antigos de login ("/?ReturnUrl=...") continuam funcionando
        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect("/entrar?ReturnUrl=" + Uri.EscapeDataString(returnUrl));
        return Redirect("/explorar");
    }

    [HttpGet("/entrar")]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Destino(returnUrl, User.IsInRole(Papeis.Funcionaria));
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Para onde ir depois de entrar: a página de onde a pessoa veio (ex.: a peça que ela queria comprar),
    // desde que seja deste site; senão, a escolha de área (funcionária) ou a loja
    IActionResult Destino(string? returnUrl, bool funcionaria) =>
        Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith("/sair", StringComparison.OrdinalIgnoreCase)
            ? LocalRedirect(returnUrl)
            : Redirect(funcionaria ? "/perfis" : "/inicio");

    // "/" também recebe o login, para formulários abertos antes da mudança de endereço
    [HttpPost("/entrar")]
    [HttpPost("/")]
    [EnableRateLimiting(Limites.Autenticacao)]
    public async Task<IActionResult> Login(string? cpf, string? senha, string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var digitos = Formato.SoDigitos(cpf);
        ViewData["Cpf"] = cpf;

        // Muitos erros para este CPF a partir deste endereço: nem confere a senha (veja Limites)
        if (Limites.LoginBloqueado(HttpContext, digitos))
        {
            ViewData["Erro"] = "Muitas tentativas erradas para este CPF. Aguarde ou use \"Esqueci minha senha\" para entrar agora.";
            return View();
        }

        var resumo = Protecao.ResumoCpf(digitos);
        var u = resumo == null ? null : await db.Usuarios.FirstOrDefaultAsync(x => x.CpfResumo == resumo);

        if (!DadosIniciais.LoginConfere(u, senha ?? ""))
        {
            Limites.RegistrarErroLogin(HttpContext, digitos);
            ViewData["Erro"] = "CPF ou senha incorretos.";
            return View();
        }

        Limites.LimparErrosLogin(HttpContext, digitos);
        await Sessao.EntrarAsync(HttpContext, u);
        // Só a funcionária tem mais de uma área para escolher; as demais pessoas vão direto para a loja
        return Destino(returnUrl, u.EhFuncionaria);
    }

    [HttpGet("/cadastro")]
    public IActionResult Cadastro(string? returnUrl) => View(new CadastroForm { ReturnUrl = returnUrl });

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
        return Destino(form.ReturnUrl, false);
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
        [FromServices] Email email, [FromServices] Avisos avisos, [FromServices] ILogger<ContaController> log)
    {
        var digitos = Formato.SoDigitos(cpf);
        var u = Formato.CpfValido(digitos) ? await db.Usuarios.FirstOrDefaultAsync(x => x.CpfResumo == Protecao.ResumoCpf(digitos)) : null;

        if (u != null && Formato.EmailValido(u.Email) && Limites.PodeEnviarRedefinicao(digitos))
        {
            var link = $"{avisos.EnderecoDoSite()}/redefinir-senha?codigo={Uri.EscapeDataString(redefinicao.GerarCodigo(u))}";
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
        Limites.LiberarCpf(u.Cpf);

        TempData["Ok"] = "Senha alterada! Agora é só entrar com a nova senha.";
        return Redirect("/entrar");
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
        if (u == null)
        {
            await Sessao.SairAsync(HttpContext);
            return Redirect("/");
        }
        return View(u);
    }

    // Abrir /sair não desloga: só pergunta. Se bastasse abrir o endereço, qualquer site poderia deslogar a pessoa.
    // Os links "Sair" do site enviam um POST (veja site.js).
    [HttpGet("/sair")]
    public IActionResult ConfirmarSaida() =>
        User.Identity?.IsAuthenticated == true ? View("Sair") : Redirect("/");

    [HttpPost("/sair")]
    public async Task<IActionResult> Sair()
    {
        await Sessao.SairAsync(HttpContext);
        return Redirect("/");
    }

    // Página de erro e de "não encontrada". Chamada pelo tratador de erros também em POSTs,
    // por isso aceita qualquer método.
    [Route("/erro/{codigo:int?}")]
    [IgnoreAntiforgeryToken]
    public IActionResult Erro(int? codigo)
    {
        var status = codigo is >= 400 and < 600 ? codigo.Value : 500;
        Response.StatusCode = status;
        return View("Erro", status);
    }
}

public class CadastroForm
{
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Senha { get; set; }
    public bool Aceito { get; set; }
    public string? ReturnUrl { get; set; }
}
