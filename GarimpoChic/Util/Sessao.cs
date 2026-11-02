using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GarimpoChic.Data;
using GarimpoChic.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Util;

public static class Papeis
{
    public const string Cliente = "Cliente";
    public const string Vendedora = "Vendedora";
    public const string Funcionaria = "Funcionaria";
}

public static class Sessao
{
    const string TipoCarimbo = "carimbo";

    // Resumo da senha atual guardado no cookie: quando a senha muda, o resumo deixa de bater
    // e os logins feitos com a senha antiga (em outros aparelhos) caem
    public static string CarimboSenha(Usuario u) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(u.SenhaHash ?? "")))[..16];

    static ClaimsPrincipal Principal(Usuario u)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Nome),
            new(TipoCarimbo, CarimboSenha(u)),
            new(ClaimTypes.Role, Papeis.Cliente),
        };
        if (u.EhVendedora) claims.Add(new(ClaimTypes.Role, Papeis.Vendedora));
        if (u.EhFuncionaria) claims.Add(new(ClaimTypes.Role, Papeis.Funcionaria));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static Task EntrarAsync(HttpContext http, Usuario u) =>
        http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Principal(u));

    // Roda a cada requisição de quem está logado: confere o cookie com o banco
    public static async Task ValidarAsync(CookieValidatePrincipalContext ctx)
    {
        var id = int.TryParse(ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var n) ? n : 0;
        var db = ctx.HttpContext.RequestServices.GetRequiredService<BrechoContext>();
        var u = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        // Conta apagada ou senha trocada depois deste login
        if (u == null || ctx.Principal!.FindFirstValue(TipoCarimbo) != CarimboSenha(u))
        {
            ctx.RejectPrincipal();
            await SairAsync(ctx.HttpContext);
            return;
        }

        // Nome ou papéis mudaram (ex.: virou funcionária ou perdeu o acesso): atualiza o cookie
        var atual = Principal(u);
        static string Resumo(ClaimsPrincipal p) =>
            string.Join("|", p.Claims.Select(c => c.Type + "=" + c.Value).Order());
        if (Resumo(atual) != Resumo(ctx.Principal!))
        {
            ctx.ReplacePrincipal(atual);
            ctx.ShouldRenew = true;
        }
    }

    public static Task SairAsync(HttpContext http) =>
        http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    public static int UsuarioId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static async Task<Usuario?> UsuarioAtualAsync(this BrechoContext db, ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true ? await db.Usuarios.FindAsync(user.UsuarioId()) : null;
}
