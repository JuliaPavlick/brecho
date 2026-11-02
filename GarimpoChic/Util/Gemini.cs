using System.Net;
using System.Text;
using System.Text.Json;
using GarimpoChic.Models;

namespace GarimpoChic.Util;

// O que o Gemini sugere a partir da foto de uma peça
public record SugestaoPeca(string Nome, string Categoria, string Descricao, decimal Preco);

// Resultado de uma tentativa: a sugestão, ou o motivo de não ter vindo (para mostrar na tela)
// Limite = o plano gratuito recusou por excesso de pedidos; vale esperar um pouco e tentar de novo
public record ResultadoGemini(SugestaoPeca? Sugestao, string? Erro, bool Limite = false);

// Preenche nome, categoria, descrição e preço de uma peça a partir da foto, usando a API do Gemini (Google).
// Configuração (na Render):
//   GEMINI_API_KEY  chave criada no Google AI Studio. Sem ela, o cadastro em lote funciona sem preenchimento automático.
//   GEMINI_MODELO   opcional; padrão gemini-3.5-flash-lite (estável e no plano gratuito)
// No plano gratuito não há cobrança: quando o limite acaba, o Google só recusa (HTTP 429) até renovar.
public class Gemini(HttpClient http, IConfiguration config, ILogger<Gemini> log)
{
    const string ModeloPadrao = "gemini-3.5-flash-lite";

    string? Chave => config["GEMINI_API_KEY"];

    public bool Configurado => !string.IsNullOrWhiteSpace(Chave);

    static readonly string Instrucoes = $$"""
        Você ajuda um brechó brasileiro a cadastrar peças usadas a partir de uma foto.
        Olhe a foto e responda só com o JSON pedido, em português do Brasil:
        - nome: curto e vendedor, até 60 caracteres (ex.: "Vestido Midi Floral Manga Bufante"). Inclua a marca só se ela aparecer na foto.
        - categoria: exatamente uma destas: {{string.Join(", ", Categorias.Todas.Select(c => c.Nome))}}.
        - descricao: 1 ou 2 frases com cor, tecido ou material aparente, estilo e estado visível.
          Mencione tamanho e marca apenas se estiverem legíveis numa etiqueta. Não invente o que não dá para ver.
        - preco: preço sugerido em reais para um brechó no Brasil, considerando que a peça é usada. Só o número.
        Se a foto não mostrar uma peça de roupa, calçado, bolsa ou acessório, use categoria "{{Categorias.Roupas}}",
        nome "Peça sem identificação" e preco 0.
        """;

    // Formato da resposta: o Gemini devolve exatamente estes campos
    static readonly object Esquema = new
    {
        type = "OBJECT",
        properties = new
        {
            nome = new { type = "STRING" },
            categoria = new { type = "STRING", @enum = Categorias.Todas.Select(c => c.Nome).ToArray() },
            descricao = new { type = "STRING" },
            preco = new { type = "NUMBER" },
        },
        required = new[] { "nome", "categoria", "descricao", "preco" },
    };

    public async Task<ResultadoGemini> SugerirAsync(byte[] foto, string tipo, CancellationToken ct = default)
    {
        if (!Configurado) return new(null, "O preenchimento automático não está ligado (falta a chave do Gemini).");

        var modelo = config["GEMINI_MODELO"] is { Length: > 0 } m ? m : ModeloPadrao;
        var endereco = (config["GEMINI_ENDERECO"] ?? "https://generativelanguage.googleapis.com").TrimEnd('/');

        var corpo = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { inline_data = new { mime_type = tipo, data = Convert.ToBase64String(foto) } },
                        new { text = Instrucoes },
                    },
                },
            },
            generationConfig = new { responseMimeType = "application/json", responseSchema = Esquema },
        };

        using var pedido = new HttpRequestMessage(HttpMethod.Post, $"{endereco}/v1beta/models/{modelo}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(corpo), Encoding.UTF8, "application/json"),
        };
        pedido.Headers.Add("x-goog-api-key", Chave);

        try
        {
            using var resposta = await http.SendAsync(pedido, ct);
            var texto = await resposta.Content.ReadAsStringAsync(ct);

            if (resposta.StatusCode == HttpStatusCode.TooManyRequests)
                return new(null, "O limite gratuito do Gemini acabou por agora. Tente de novo daqui a pouco.", Limite: true);
            if (!resposta.IsSuccessStatusCode)
            {
                // Não registra o corpo do pedido (tem a foto); a resposta de erro do Google não tem dados sensíveis
                log.LogWarning("Gemini respondeu {Status}: {Resposta}", (int)resposta.StatusCode, texto[..Math.Min(texto.Length, 500)]);
                return new(null, resposta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest
                    ? "O Gemini recusou o pedido. Confira a chave GEMINI_API_KEY e o modelo na Render."
                    : "O Gemini não respondeu agora. Tente de novo daqui a pouco.");
            }

            return new(Ler(texto), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            log.LogWarning(ex, "Falha ao consultar o Gemini");
            return new(null, "Não foi possível ler a resposta do Gemini. Tente de novo.");
        }
    }

    // A resposta vem em candidates[0].content.parts[*].text, com o JSON pedido dentro do texto
    static SugestaoPeca Ler(string respostaJson)
    {
        using var doc = JsonDocument.Parse(respostaJson);
        var partes = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts");
        var texto = string.Concat(partes.EnumerateArray()
            .Where(p => p.TryGetProperty("text", out _))
            .Select(p => p.GetProperty("text").GetString()));

        // Às vezes o modelo embrulha o JSON em ```json ... ```
        var inicio = texto.IndexOf('{');
        var fim = texto.LastIndexOf('}');
        if (inicio < 0 || fim < inicio) throw new JsonException("Resposta sem JSON");

        using var dados = JsonDocument.Parse(texto[inicio..(fim + 1)]);
        var r = dados.RootElement;

        string Campo(string nome) => r.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()!.Trim() : "";

        var categoria = Campo("categoria");
        if (!Categorias.Existe(categoria)) categoria = Categorias.Roupas;

        var preco = 0m;
        if (r.TryGetProperty("preco", out var p))
        {
            if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var numero)) preco = numero;
            else if (p.ValueKind == JsonValueKind.String) preco = Precos.Ler(p.GetString());
        }

        return new SugestaoPeca(
            Limitar(Campo("nome"), 120) is { Length: > 0 } nome ? nome : "Peça sem nome",
            categoria,
            Limitar(Campo("descricao"), 500),
            Math.Clamp(Math.Round(preco, 2), 0, 100_000));
    }

    static string Limitar(string texto, int max) => texto.Length <= max ? texto : texto[..max].TrimEnd();
}
