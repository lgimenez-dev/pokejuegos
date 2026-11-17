namespace PokeJuegos.Application.Helpers;

/// <summary>
/// Generates the subject and HTML body of the transactional emails sent by the system, in English or Spanish.
/// </summary>
public static class EmailHelper
{
    private static readonly EmailTexts English = new(
        Greeting: "Hi",
        FallbackHint: "If the button doesn't work, copy and paste the following link into your browser:",
        ExpirationNotice: "This link will expire in {0}. If you did not request this email, you can safely ignore it.",
        ActivationTitle: "Activate your account in {0}",
        ActivationMessage: "Your account has been created. To get started you need to activate it.",
        ActivationCta: "Activate my account",
        ActivationExpiration: "24 hours",
        RecoveryTitle: "Reset your password in {0}",
        RecoveryMessage: "We received a request to reset the password for your account. If this was you, click the button below.",
        RecoveryCta: "Reset my password",
        CreationTitle: "Create a password for {0}",
        CreationMessage: "You signed up with Google, so your account doesn't have a password yet. To also log in with your email and a password, click the button below.",
        CreationCta: "Create my password",
        RecoveryExpiration: "30 minutes");

    private static readonly EmailTexts Spanish = new(
        Greeting: "Hola",
        FallbackHint: "Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:",
        ExpirationNotice: "Este enlace vence en {0}. Si no pediste este email, puedes ignorarlo.",
        ActivationTitle: "Activa tu cuenta en {0}",
        ActivationMessage: "Tu cuenta fue creada. Para empezar, necesitas activarla.",
        ActivationCta: "Activar mi cuenta",
        ActivationExpiration: "24 horas",
        RecoveryTitle: "Restablece tu contraseña en {0}",
        RecoveryMessage: "Recibimos una solicitud para restablecer la contraseña de tu cuenta. Si fuiste tú, haz clic en el botón.",
        RecoveryCta: "Restablecer mi contraseña",
        CreationTitle: "Crea una contraseña para {0}",
        CreationMessage: "Te registraste con Google, así que tu cuenta todavía no tiene contraseña. Para ingresar también con tu email y una contraseña, haz clic en el botón.",
        CreationCta: "Crear mi contraseña",
        RecoveryExpiration: "30 minutos");

    /// <summary>
    /// Generates the account activation email.
    /// </summary>
    /// <param name="name">The recipient's full name.</param>
    /// <param name="activationLink">The frontend URL to activate the account.</param>
    /// <param name="appName">The application name shown in the title and footer.</param>
    /// <param name="language">Two-letter language code (<c>en</c> or <c>es</c>); anything else falls back to English.</param>
    /// <returns>The subject and the HTML body.</returns>
    public static (string Subject, string Body) AccountActivation(string name, string activationLink, string appName, string language)
    {
        EmailTexts texts = TextsFor(language);
        string title = string.Format(texts.ActivationTitle, appName);
        return (title, Template(texts, LanguageCode(language), name, title, texts.ActivationMessage, texts.ActivationCta, activationLink, texts.ActivationExpiration, appName));
    }

    /// <summary>
    /// Generates the password recovery email.
    /// </summary>
    /// <param name="name">The recipient's full name.</param>
    /// <param name="recoveryLink">The frontend URL to reset the password.</param>
    /// <param name="appName">The application name shown in the title and footer.</param>
    /// <param name="language">Two-letter language code (<c>en</c> or <c>es</c>); anything else falls back to English.</param>
    /// <returns>The subject and the HTML body.</returns>
    public static (string Subject, string Body) PasswordRecovery(string name, string recoveryLink, string appName, string language)
    {
        EmailTexts texts = TextsFor(language);
        string title = string.Format(texts.RecoveryTitle, appName);
        return (title, Template(texts, LanguageCode(language), name, title, texts.RecoveryMessage, texts.RecoveryCta, recoveryLink, texts.RecoveryExpiration, appName));
    }

    /// <summary>
    /// Generates the password recovery email of an OAuth2 account that has no password yet.
    /// </summary>
    /// <param name="name">The recipient's full name.</param>
    /// <param name="creationLink">The frontend URL to set the password (same page as the recovery flow).</param>
    /// <param name="appName">The application name shown in the title and footer.</param>
    /// <param name="language">Two-letter language code (<c>en</c> or <c>es</c>); anything else falls back to English.</param>
    /// <returns>The subject and the HTML body.</returns>
    public static (string Subject, string Body) PasswordCreation(string name, string creationLink, string appName, string language)
    {
        EmailTexts texts = TextsFor(language);
        string title = string.Format(texts.CreationTitle, appName);
        return (title, Template(texts, LanguageCode(language), name, title, texts.CreationMessage, texts.CreationCta, creationLink, texts.RecoveryExpiration, appName));
    }

    /// <summary>
    /// Normalizes a language code to one the emails and the web support.
    /// </summary>
    /// <param name="language">Two-letter language code.</param>
    /// <returns><c>es</c> for Spanish, <c>en</c> for anything else.</returns>
    public static string LanguageCode(string language) => language == "es" ? "es" : "en";

    private static EmailTexts TextsFor(string language) => LanguageCode(language) == "es" ? Spanish : English;

    private static string Template(
        EmailTexts texts,
        string lang,
        string name,
        string title,
        string message,
        string ctaText,
        string link,
        string expiration,
        string appName) => $"""
        <!DOCTYPE html>
        <html lang="{lang}">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>{title}</title>
          <!-- Lato where the client loads web fonts (Apple Mail, iOS); Gmail/Outlook fall back to Arial. -->
          <link href="https://fonts.googleapis.com/css2?family=Lato:wght@400;700&display=swap" rel="stylesheet" />
        </head>
        <!-- Plain white page, no card: centered title over a divider and the app's red pill button. -->
        <body style="margin:0;padding:0;background-color:#ffffff;font-family:'Lato',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="background-color:#ffffff;padding:32px 16px 24px;">
            <tr>
              <td align="center">
                <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="max-width:440px;">
                  <!-- Header -->
                  <tr>
                    <td style="padding:16px 20px;border-bottom:1px solid #e2e2e2;text-align:center;">
                      <span style="font-size:20px;font-weight:700;color:#333333;">{title}</span>
                    </td>
                  </tr>
                  <!-- Body -->
                  <tr>
                    <td style="padding:20px;font-size:17px;line-height:1.5;color:#4a4a4a;">
                      <p style="margin:0 0 16px;">{texts.Greeting} <strong>{name}</strong>,</p>
                      <p style="margin:0 0 24px;">{message}</p>
                      <table cellpadding="0" cellspacing="0" width="100%" role="presentation">
                        <tr>
                          <td align="center" style="padding:0 0 24px;">
                            <a href="{link}"
                               style="display:block;background-color:#F22539;color:#ffffff;font-size:18px;text-decoration:none;padding:14px 24px;border-radius:999px;text-align:center;">
                              {ctaText}
                            </a>
                          </td>
                        </tr>
                      </table>
                      <p style="margin:0 0 8px;font-size:14px;color:#888888;">
                        {texts.FallbackHint}
                      </p>
                      <p style="margin:0 0 16px;padding-bottom:16px;border-bottom:1px solid #e2e2e2;font-size:14px;word-break:break-all;">
                        <a href="{link}" style="color:#F22539;">{link}</a>
                      </p>
                      <p style="margin:0;font-size:14px;color:#888888;">
                        {string.Format(texts.ExpirationNotice, expiration)}
                      </p>
                    </td>
                  </tr>
                </table>
                <p style="margin:24px 0 0;font-size:13px;color:#aaaaaa;">© 2026 {appName}</p>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    /// <summary>The texts of every email in one language. Titles and the expiration notice are format strings.</summary>
    private sealed record EmailTexts(
        string Greeting,
        string FallbackHint,
        string ExpirationNotice,
        string ActivationTitle,
        string ActivationMessage,
        string ActivationCta,
        string ActivationExpiration,
        string RecoveryTitle,
        string RecoveryMessage,
        string RecoveryCta,
        string CreationTitle,
        string CreationMessage,
        string CreationCta,
        string RecoveryExpiration);
}
