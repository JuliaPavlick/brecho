using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace GarimpoChic.Util;

// Limita quantos envios um mesmo endereço de internet faz nas telas de login, cadastro e senha.
// Isso freia quem tenta muitas senhas ou muitos CPFs; o bloqueio por conta fica no ContaController.
public static class Limites
{
    public const string Autenticacao = "autenticacao";

    // O cadastro precisa avisar quando o CPF já tem conta, e isso permite testar CPFs para descobrir quem é cliente.
    // Para frear isso sem atrapalhar uma turma inteira se cadastrando na mesma rede, contamos só os CPFs repetidos:
    // depois de 5 numa hora, o endereço fica sem poder usar o cadastro (para qualquer CPF) até a hora passar.
    static readonly PartitionedRateLimiter<string> CpfRepetido = PartitionedRateLimiter.Create<string, string>(ip =>
        RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
        }));

    public static bool CadastroBloqueado(HttpContext http) =>
        CpfRepetido.GetStatistics(EnderecoDoVisitante(http))?.CurrentAvailablePermits == 0;

    public static void RegistrarCpfRepetido(HttpContext http) =>
        CpfRepetido.AttemptAcquire(EnderecoDoVisitante(http)).Dispose();

    // Senha errada: depois de 5 erros para o mesmo CPF vindos do mesmo endereço, esse par fica bloqueado por 15 min.
    // O bloqueio é por CPF + endereço (e não pela conta toda) para que ninguém consiga travar a conta de outra
    // pessoa de longe; e vale igual para CPF com ou sem cadastro, para a mensagem não revelar quem é cliente.
    const int MaxErrosLogin = 5;
    static readonly TimeSpan JanelaLogin = TimeSpan.FromMinutes(15);

    // Link de "esqueci minha senha": no máximo 3 e-mails por hora para a mesma conta, para não lotar a caixa dela
    const int MaxEmailsRedefinicao = 3;

    static readonly MemoryCache Contadores = new(new MemoryCacheOptions { SizeLimit = 200_000 });

    static int Contar(string chave) => Contadores.TryGetValue(chave, out int n) ? n : 0;

    static void Somar(string chave, TimeSpan validade) =>
        Contadores.Set(chave, Contar(chave) + 1, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = validade });

    // Segundo freio, que não depende do endereço: 20 senhas erradas para o mesmo CPF em 1 hora, vindas de onde for,
    // pausam o login desse CPF. Assim, mesmo quem consegue trocar de endereço a cada tentativa (ou forjar o
    // cabeçalho do IP) não testa senhas sem parar. Vale igual para CPF com ou sem conta (não revela quem é cliente),
    // e "Esqueci minha senha" libera na hora, para a dona da conta não ficar presa.
    const int MaxErrosPorCpf = 20;
    static readonly TimeSpan JanelaPorCpf = TimeSpan.FromHours(1);

    static string ChaveLogin(HttpContext http, string cpf) => $"login|{cpf}|{EnderecoDoVisitante(http)}";

    static string ChaveCpf(string cpf) => $"login-cpf|{cpf}";

    public static bool LoginBloqueado(HttpContext http, string cpf) =>
        Contar(ChaveLogin(http, cpf)) >= MaxErrosLogin || Contar(ChaveCpf(cpf)) >= MaxErrosPorCpf;

    public static void RegistrarErroLogin(HttpContext http, string cpf)
    {
        Somar(ChaveLogin(http, cpf), JanelaLogin);
        Somar(ChaveCpf(cpf), JanelaPorCpf);
    }

    public static void LimparErrosLogin(HttpContext http, string cpf) => Contadores.Remove(ChaveLogin(http, cpf));

    // Senha redefinida pelo link do e-mail: a dona da conta provou quem é, então o CPF volta a poder entrar
    public static void LiberarCpf(string cpf) => Contadores.Remove(ChaveCpf(cpf));

    public static bool PodeEnviarRedefinicao(string cpf)
    {
        var chave = "redefinir|" + cpf;
        if (Contar(chave) >= MaxEmailsRedefinicao) return false;
        Somar(chave, TimeSpan.FromHours(1));
        return true;
    }

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
    public static string EnderecoDoVisitante(HttpContext http) =>
        http.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
        ?? http.Connection.RemoteIpAddress?.ToString()
        ?? "desconhecido";
}
