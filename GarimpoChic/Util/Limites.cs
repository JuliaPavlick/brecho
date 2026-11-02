using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace GarimpoChic.Util;

// Limita quantos envios um mesmo endereço de internet faz nas telas de login, cadastro e senha.
// Isso freia quem tenta muitas senhas ou muitos CPFs; o bloqueio por conta fica no ContaController.
public static class Limites
{
    public const string Autenticacao = "autenticacao";

    public static void Configurar(RateLimiterOptions o)
    {
        o.AddPolicy(Autenticacao, http => RateLimitPartition.GetSlidingWindowLimiter(
            EnderecoDoVisitante(http),
            _ => new SlidingWindowRateLimiterOptions
            {
                // Folgado o bastante para uma sala de aula na mesma rede testar o site ao mesmo tempo
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(5),
                SegmentsPerWindow = 5,
                QueueLimit = 0,
            }));

        o.OnRejected = async (ctx, ct) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            ctx.HttpContext.Response.ContentType = "text/html; charset=utf-8";
            await ctx.HttpContext.Response.WriteAsync(
                "<p style=\"font-family:sans-serif;text-align:center;margin-top:4rem\">" +
                "Muitas tentativas em pouco tempo. Aguarde alguns minutos e tente de novo.<br><br>" +
                "<a href=\"/\">Voltar</a></p>", ct);
        };
    }

    // Na Render o site fica atrás da Cloudflare, que informa o IP real do visitante neste cabeçalho
    // (e o sobrescreve, então o visitante não consegue forjá-lo). Fora da Render, usa o IP da conexão.
    static string EnderecoDoVisitante(HttpContext http) =>
        http.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
        ?? http.Connection.RemoteIpAddress?.ToString()
        ?? "desconhecido";
}
