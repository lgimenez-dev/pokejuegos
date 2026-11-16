using PokeJuegos.Domain.Entities;

namespace PokeJuegos.Application.Services;

/// <summary>
/// Defines SMTP email sending using a stored <see cref="SmtpConfig"/>.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an HTML email to the specified recipient.
    /// </summary>
    /// <param name="smtpConfig">The SMTP configuration to use for sending.</param>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML body of the email.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync(SmtpConfig smtpConfig, string to, string subject, string htmlBody, CancellationToken ct = default);
}
