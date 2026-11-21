using System.Net;
using System.Threading.Channels;
using GarimpoChic.Models;

namespace GarimpoChic.Util;

public record EmailPendente(string Para, string Assunto, string Html);

// Fila dos e-mails de aviso. A tela só coloca o e-mail na fila e segue; o envio acontece em segundo plano
// (EnvioDeEmails), para a funcionária não esperar o servidor de e-mail nem ver erro se ele estiver fora do ar.
// Se o site reiniciar com e-mails na fila, eles se perdem: são avisos, não registros.
public class FilaDeEmails
{
    readonly Channel<EmailPendente> fila = Channel.CreateBounded<EmailPendente>(
        new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });

    public ChannelReader<EmailPendente> Leitor => fila.Reader;

    // Virtual: os testes automáticos trocam por uma versão que só guarda os e-mails
    public virtual void Enfileirar(EmailPendente email) => fila.Writer.TryWrite(email);
}

public class EnvioDeEmails(FilaDeEmails fila, Email email, ILogger<EnvioDeEmails> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken parar)
    {
        await foreach (var e in fila.Leitor.ReadAllAsync(parar))
        {
            // Uma segunda tentativa depois de meio minuto cobre quedas rápidas do servidor de e-mail
            for (var tentativa = 1; tentativa <= 2; tentativa++)
            {
                try
                {
                    await email.EnviarAsync(e.Para, e.Assunto, e.Html);
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.LogWarning(ex, "Falha ao enviar aviso por e-mail ({Assunto}), tentativa {Tentativa}", e.Assunto, tentativa);
                    if (tentativa == 1) await Task.Delay(TimeSpan.FromSeconds(30), parar);
                }
            }
        }
    }
}

// Os avisos que o site manda para clientes e vendedoras. Cada método monta o texto e põe na fila.
// Conta sem e-mail válido (ex.: conta excluída) não recebe nada.
public class Avisos(FilaDeEmails fila, IConfiguration config, IHttpContextAccessor acesso)
{
    static string H(string? texto) => WebUtility.HtmlEncode(texto ?? "");

    // Endereço público do site, para os links. Na Render vem de RENDER_EXTERNAL_URL; não confiamos no cabeçalho Host.
    public string EnderecoDoSite()
    {
        var r = acesso.HttpContext?.Request;
        return (Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL")
                ?? config["SITE_URL"]
                ?? (r == null ? "" : $"{r.Scheme}://{r.Host}")).TrimEnd('/');
    }

    void Enviar(Usuario? para, string assunto, string corpoHtml, string? botao = null, string? caminho = null)
    {
        if (para == null || !Formato.EmailValido(para.Email)) return;

        var link = botao != null && caminho != null
            ? $"""<p style="margin:24px 0"><a href="{H(EnderecoDoSite() + caminho)}" style="background:#8a6d1f;color:#fff;padding:12px 20px;border-radius:10px;text-decoration:none">{H(botao)}</a></p>"""
            : "";

        var html = $"""
            <div style="font-family:Arial,sans-serif;color:#3d3426;max-width:520px">
                <p>Olá, {H(para.PrimeiroNome)}!</p>
                {corpoHtml}
                {link}
                <p style="color:#776843;font-size:13px">Garimpo Chic — garimpado com carinho</p>
            </div>
            """;

        // Quebra de linha no assunto é recusada pelo envio (e poderia injetar cabeçalhos)
        var assuntoLimpo = assunto.ReplaceLineEndings(" ");
        fila.Enfileirar(new EmailPendente(para.Email.Trim(), assuntoLimpo + " — Garimpo Chic", html));
    }

    static string ListaDeItens(IEnumerable<PedidoItem> itens) =>
        "<ul>" + string.Concat(itens.Select(i => $"<li>{H(i.Nome)} — {H(Formato.Moeda(i.Preco))}</li>")) + "</ul>";

    // ---------- Cliente ----------

    public void PedidoRecebido(Usuario cliente, Pedido pedido) => Enviar(cliente,
        $"Recebemos seu pedido {pedido.Numero}",
        $"""
        <p>Recebemos seu pedido <strong>{H(pedido.Numero)}</strong>:</p>
        {ListaDeItens(pedido.Itens)}
        <p>Total: <strong>{H(Formato.Moeda(pedido.Total))}</strong></p>
        <p>Entrega: {H(pedido.DescricaoEntrega)}</p>
        <p>A loja vai entrar em contato para combinar o pagamento e a entrega.</p>
        """,
        "Ver minhas compras", "/compras");

    public void PedidoMudou(Usuario cliente, Pedido pedido)
    {
        var (assunto, texto) = pedido.Status switch
        {
            StatusPedido.Caminho => ($"Seu pedido {pedido.Numero} está a caminho", "está a caminho! Logo ele chega até você."),
            StatusPedido.Entregue => ($"Seu pedido {pedido.Numero} foi entregue", "foi entregue. Esperamos que você ame suas peças ✦"),
            StatusPedido.Cancelado => ($"Seu pedido {pedido.Numero} foi cancelado",
                "foi cancelado. Se você não esperava por isso, fale com a loja."),
            _ => (null, null),
        };
        if (assunto == null) return;

        Enviar(cliente, assunto, $"<p>Seu pedido <strong>{H(pedido.Numero)}</strong> {texto}</p>{ListaDeItens(pedido.Itens)}",
            "Ver minhas compras", "/compras");
    }

    // ---------- Vendedora ----------

    public void PecaAprovada(Usuario vendedora, Produto peca) => Enviar(vendedora,
        $"Sua peça \"{peca.Nome}\" está à venda",
        $"""
        <p>A loja avaliou e aprovou sua peça <strong>{H(peca.Nome)}</strong>. Ela já está à venda por
        <strong>{H(Formato.Moeda(peca.Preco))}</strong>.</p>
        """,
        "Ver a peça na loja", $"/peca/{peca.Id}");

    public void PecaRecusada(Usuario vendedora, Produto peca) => Enviar(vendedora,
        $"Sobre sua peça \"{peca.Nome}\"",
        $"""
        <p>A loja avaliou sua peça <strong>{H(peca.Nome)}</strong> e, desta vez, ela não vai entrar à venda.</p>
        <p>Se ela ficou com a loja, fale com a equipe para combinar a devolução.</p>
        """,
        "Ver minhas peças", "/vender");

    public void PecasEntregues(Usuario vendedora, IReadOnlyList<PedidoItem> itens, decimal saldo) => Enviar(vendedora,
        itens.Count == 1 ? $"Sua peça \"{itens[0].Nome}\" foi vendida" : $"{itens.Count} peças suas foram vendidas",
        $"""
        <p>{(itens.Count == 1 ? "Uma peça sua foi vendida e entregue" : "Peças suas foram vendidas e entregues")}:</p>
        <ul>{string.Concat(itens.Select(i => $"<li>{H(i.Nome)} — você recebe {H(Formato.Moeda(i.ValorVendedora))}</li>"))}</ul>
        <p>Seu saldo a receber agora é <strong>{H(Formato.Moeda(Math.Max(0, saldo)))}</strong>.</p>
        """,
        "Ver meus ganhos", "/vender");

    public void RepasseRegistrado(Usuario vendedora, decimal valor, DateTime data) => Enviar(vendedora,
        $"Pagamento de {Formato.Moeda(valor)} registrado",
        $"""
        <p>A loja registrou um pagamento de <strong>{H(Formato.Moeda(valor))}</strong> para você,
        em {H(Formato.Data(data))}.</p>
        """,
        "Ver meus ganhos", "/vender");
}
