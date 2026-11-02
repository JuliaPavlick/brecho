using GarimpoChic.Data;
using GarimpoChic.Models;
using GarimpoChic.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Controllers;

public class ContaController(BrechoContext db) : Controller
{
    [HttpGet("/")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect("/perfis");
        return View();
    }

    [HttpPost("/")]
    public async Task<IActionResult> Login(string? cpf, string? senha)
    {
        var digitos = Formato.SoDigitos(cpf);
        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Cpf == digitos);

        if (u == null || !DadosIniciais.SenhaConfere(u, senha ?? ""))
        {
            ViewData["Erro"] = "CPF ou senha incorretos.";
            ViewData["Cpf"] = cpf;
            return View();
        }

        await Sessao.EntrarAsync(HttpContext, u);
        return Redirect("/perfis");
    }

    [HttpGet("/cadastro")]
    public IActionResult Cadastro() => View(new CadastroForm());

    [HttpPost("/cadastro")]
    public async Task<IActionResult> Cadastro(CadastroForm form)
    {
        var cpf = Formato.SoDigitos(form.Cpf);
        var nome = (form.Nome ?? "").Trim();

        if (nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            ModelState.AddModelError("nome", "Informe seu nome completo.");
        if (!Formato.CpfValido(cpf))
            ModelState.AddModelError("cpf", "Informe um CPF válido.");
        else if (await db.Usuarios.AnyAsync(u => u.Cpf == cpf))
            ModelState.AddModelError("cpf", "Já existe uma conta com este CPF.");
        if (!Formato.EmailValido(form.Email))
            ModelState.AddModelError("email", "Informe um e-mail válido.");
        if (Formato.SoDigitos(form.Telefone).Length < 10)
            ModelState.AddModelError("telefone", "Informe um telefone válido.");
        if ((form.Senha ?? "").Length < 8)
            ModelState.AddModelError("senha", "Use pelo menos 8 caracteres.");

        if (!ModelState.IsValid) return View(form);

        var u = new Usuario
        {
            Nome = nome,
            Cpf = cpf,
            Email = form.Email!.Trim(),
            Telefone = form.Telefone!.Trim(),
        };
        u.SenhaHash = DadosIniciais.HashSenha(u, form.Senha!);

        db.Usuarios.Add(u);
        await db.SaveChangesAsync();

        await Sessao.EntrarAsync(HttpContext, u);
        return Redirect("/perfis");
    }

    [HttpGet("/recuperar-senha")]
    public IActionResult RecuperarSenha() => View();

    // Ainda não há envio de e-mail configurado: a tela só confirma o pedido,
    // sem revelar se o CPF existe ou não.
    [HttpPost("/recuperar-senha")]
    public IActionResult RecuperarSenha(string? cpf)
    {
        ViewData["Enviado"] = Formato.CpfValido(cpf);
        return View();
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
}
