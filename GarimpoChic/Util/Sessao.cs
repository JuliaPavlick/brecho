using System.Security.Claims;
using GarimpoChic.Data;
using GarimpoChic.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GarimpoChic.Util;

public static class Papeis
{
    public const string Cliente = "Cliente";
    public const string Vendedora = "Vendedora";
    public const string Funcionaria = "Funcionaria";
}

public static class Sessao
{
    public static async Task EntrarAsync(HttpContext http, Usuario u)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Nome),
            new(ClaimTypes.Role, Papeis.Cliente),
        };
        if (u.EhVendedora) claims.Add(new(ClaimTypes.Role, Papeis.Vendedora));
        if (u.EhFuncionaria) claims.Add(new(ClaimTypes.Role, Papeis.Funcionaria));

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));
    }

    public static Task SairAsync(HttpContext http) =>
        http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    public static int UsuarioId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static async Task<Usuario?> UsuarioAtualAsync(this BrechoContext db, ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true ? await db.Usuarios.FindAsync(user.UsuarioId()) : null;
}
