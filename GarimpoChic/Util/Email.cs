using System.Net;
using System.Net.Mail;

namespace GarimpoChic.Util;

// Envia e-mails por SMTP. Configuração pelas variáveis de ambiente:
// SMTP_HOST, SMTP_PORTA (padrão 587), SMTP_USUARIO, SMTP_SENHA e SMTP_REMETENTE.
// Sem SMTP_HOST, o e-mail não é enviado. No desenvolvimento o conteúdo vai para o log, para testar;
// em produção, não: o e-mail pode ter um link de redefinir senha, e quem lê os logs não deve vê-lo.
public class Email(IConfiguration config, IHostEnvironment env, ILogger<Email> log)
{
    public bool Configurado => !string.IsNullOrWhiteSpace(config["SMTP_HOST"]);

    public async Task EnviarAsync(string para, string assunto, string html)
    {
        if (!Configurado)
        {
            if (env.IsDevelopment())
                log.LogWarning("SMTP não configurado. E-mail para {Para} não enviado. Assunto: {Assunto}\n{Html}", para, assunto, html);
            else
                log.LogWarning("SMTP não configurado: um e-mail ({Assunto}) não foi enviado.", assunto);
            return;
        }

        var remetente = config["SMTP_REMETENTE"] ?? config["SMTP_USUARIO"]!;

        using var cliente = new SmtpClient(config["SMTP_HOST"], config.GetValue("SMTP_PORTA", 587))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(config["SMTP_USUARIO"], config["SMTP_SENHA"]),
        };

        using var mensagem = new MailMessage(new MailAddress(remetente, "Garimpo Chic"), new MailAddress(para))
        {
            Subject = assunto,
            Body = html,
            IsBodyHtml = true,
        };

        await cliente.SendMailAsync(mensagem);
    }
}
