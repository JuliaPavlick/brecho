using System.Net;
using System.Text.RegularExpressions;

namespace GarimpoChic.Tests.Infra;

// Faz o papel do navegador: guarda os cookies, pega o token anti-falsificação da página
// e envia formulários e chamadas fetch() como as telas do site fazem.
public partial class Navegador(HttpClient http) : IDisposable
{
    // Cada navegador finge vir de um endereço de internet diferente, para os limites de tentativas
    // (que contam por endereço) de um teste não atrapalharem outro
    readonly string ip = $"10.{Random.Shared.Next(256)}.{Random.Shared.Next(256)}.{Random.Shared.Next(1, 255)}";

    string? token;

    [GeneratedRegex("name=\"csrf\" content=\"([^\"]+)\"")]
    private static partial Regex Csrf();

    HttpRequestMessage Pedido(HttpMethod metodo, string url)
    {
        var p = new HttpRequestMessage(metodo, url);
        p.Headers.Add("CF-Connecting-IP", ip);
        return p;
    }

    // Abre uma página e guarda o token que vem nela
    public async Task<(HttpResponseMessage Resposta, string Html)> Abrir(string url)
    {
        var resposta = await http.SendAsync(Pedido(HttpMethod.Get, url));
        var html = await resposta.Content.ReadAsStringAsync();
        var achado = Csrf().Match(html);
        if (achado.Success) token = WebUtility.HtmlDecode(achado.Groups[1].Value);
        return (resposta, html);
    }

    async Task<string> Token()
    {
        if (token == null) await Abrir("/explorar");
        return token!;
    }

    // Envio de formulário comum (a página recarrega)
    public async Task<HttpResponseMessage> Enviar(string url, Dictionary<string, string>? campos = null, bool comToken = true)
    {
        var dados = new Dictionary<string, string>(campos ?? []);
        if (comToken) dados["__RequestVerificationToken"] = await Token();

        var p = Pedido(HttpMethod.Post, url);
        p.Content = new FormUrlEncodedContent(dados);
        return await http.SendAsync(p);
    }

    // Formulário com arquivos (multipart), como "Vender" e o modal de produtos
    public static MultipartFormDataContent Multipart(Dictionary<string, string> campos, params (string Campo, string Nome, byte[] Dados)[] arquivos)
    {
        var corpo = new MultipartFormDataContent();
        foreach (var (k, v) in campos) corpo.Add(new StringContent(v), k);
        foreach (var (campo, nome, dados) in arquivos) corpo.Add(new ByteArrayContent(dados), campo, nome);
        return corpo;
    }

    public async Task<HttpResponseMessage> EnviarArquivos(string url, Dictionary<string, string> campos,
        params (string Campo, string Nome, byte[] Dados)[] arquivos)
    {
        var corpo = Multipart(campos, arquivos);
        corpo.Add(new StringContent(await Token()), "__RequestVerificationToken");

        var p = Pedido(HttpMethod.Post, url);
        p.Content = corpo;
        return await http.SendAsync(p);
    }

    // Chamada feita por fetch() no site.js (postar): token no cabeçalho e marcada como AJAX
    public async Task<HttpResponseMessage> Fetch(string url, HttpContent? corpo = null)
    {
        var p = Pedido(HttpMethod.Post, url);
        p.Headers.Add("RequestVerificationToken", await Token());
        p.Headers.Add("X-Requested-With", "XMLHttpRequest");
        p.Content = corpo;
        return await http.SendAsync(p);
    }

    public async Task<HttpResponseMessage> Entrar(string cpf, string senha = SiteDeTeste.Senha, string? returnUrl = null)
    {
        await Abrir("/");
        var campos = new Dictionary<string, string> { ["cpf"] = cpf, ["senha"] = senha };
        if (returnUrl != null) campos["returnUrl"] = returnUrl;
        var resposta = await Enviar("/", campos);
        // Depois de entrar, o token muda (fica ligado à conta): pega o novo
        token = null;
        return resposta;
    }

    public void Dispose() => http.Dispose();
}

public static class RespostaExtensions
{
    // Para onde a resposta manda o navegador (só o caminho, sem o domínio)
    public static string? Destino(this HttpResponseMessage r) =>
        r.Headers.Location is { } l ? (l.IsAbsoluteUri ? l.PathAndQuery : l.OriginalString) : null;
}
