using PokeJuegos.Application.Services;
using PokeJuegos.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace PokeJuegos.Infrastructure.Services;

/// <summary>
/// Sends transactional emails via SMTP using MailKit.
/// </summary>
internal sealed class EmailService : IEmailService
{
    /// <inheritdoc/>
    public async Task SendAsync(
        SmtpConfig smtpConfig,
        string to,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpConfig.SenderName, smtpConfig.Email));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using SmtpClient client = new SmtpClient();
        SecureSocketOptions security = smtpConfig.UseTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(smtpConfig.Host, smtpConfig.Port, security, ct);
        await client.AuthenticateAsync(smtpConfig.Email, smtpConfig.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
