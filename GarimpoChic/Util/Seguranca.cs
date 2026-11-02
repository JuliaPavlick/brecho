using System.Security.Cryptography;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GarimpoChic.Util;

// Cabeçalhos que dizem ao navegador o que a página pode fazer.
// O principal é a CSP: só roda JavaScript do próprio site ou dos <script> que o servidor marcou com o "nonce"
// desta resposta. Assim, mesmo que alguém consiga injetar um <script> numa página, o navegador não executa.
public static class Seguranca
{
    const string ChaveNonce = "csp-nonce";

    public static string? Nonce(HttpContext http) => http.Items[ChaveNonce] as string;

    public static IApplicationBuilder UseCabecalhosDeSeguranca(this IApplicationBuilder app) =>
        app.Use(async (http, proximo) =>
        {
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            http.Items[ChaveNonce] = nonce;

            var h = http.Response.Headers;
            // Estilos inline continuam liberados: as páginas usam muitos style="..." e CSS não executa código
            h.ContentSecurityPolicy =
                "default-src 'self'; " +
                $"script-src 'self' 'nonce-{nonce}'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: blob:; " +
                "connect-src 'self'; " +
                "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
            h.XContentTypeOptions = "nosniff";
            // Navegadores antigos não entendem frame-ancestors
            h.XFrameOptions = "DENY";
            h["Referrer-Policy"] = "strict-origin-when-cross-origin";
            h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

            await proximo();
        });
}

// Coloca o nonce da resposta em todo <script> das views, sem precisar lembrar de escrever em cada uma
[HtmlTargetElement("script")]
public class NonceTagHelper(IHttpContextAccessor acesso) : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var nonce = acesso.HttpContext == null ? null : Seguranca.Nonce(acesso.HttpContext);
        if (nonce != null) output.Attributes.SetAttribute("nonce", nonce);
    }
}
