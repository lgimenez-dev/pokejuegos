namespace PokeJuegos.Application.Helpers;

/// <summary>
/// Generates the HTML bodies for transactional emails sent by the system.
/// </summary>
public static class EmailHelper
{
    /// <summary>
    /// Generates the HTML body for an account activation email.
    /// </summary>
    /// <param name="name">The recipient's full name.</param>
    /// <param name="activationLink">The frontend URL to activate the account and set a password.</param>
    /// <param name="appName">The application name shown in the header and footer.</param>
    /// <returns>An HTML string ready to be used as the email body.</returns>
    public static string AccountActivation(string name, string activationLink, string appName) =>
        Template(
            name,
            title: $"Activate your account in {appName}",
            message: "Your account has been created. To get started you need to activate it and set your password.",
            ctaText: "Activate my account",
            link: activationLink,
            expiration: "24 hours",
            appName: appName);

    /// <summary>
    /// Generates the HTML body for a password recovery email.
    /// </summary>
    /// <param name="name">The recipient's full name.</param>
    /// <param name="recoveryLink">The frontend URL to reset the password.</param>
    /// <param name="appName">The application name shown in the header and footer.</param>
    /// <returns>An HTML string ready to be used as the email body.</returns>
    public static string PasswordRecovery(string name, string recoveryLink, string appName) =>
        Template(
            name,
            title: $"Reset your password in {appName}",
            message: "We received a request to reset the password for your account. If this was you, click the button below.",
            ctaText: "Reset my password",
            link: recoveryLink,
            expiration: "30 minutes",
            appName: appName);

    /// <summary>
    /// Generates the HTML body for the password recovery email of an OAuth2 account that has no password yet.
    /// </summary>
    /// <param name="name">The recipient's full name.</param>
    /// <param name="creationLink">The frontend URL to set the password (same page as the recovery flow).</param>
    /// <param name="appName">The application name shown in the header and footer.</param>
    /// <returns>An HTML string ready to be used as the email body.</returns>
    public static string PasswordCreation(string name, string creationLink, string appName) =>
        Template(
            name,
            title: $"Create a password for {appName}",
            message: "You signed up with Google, so your account doesn't have a password yet. To also log in with your email and a password, click the button below.",
            ctaText: "Create my password",
            link: creationLink,
            expiration: "30 minutes",
            appName: appName);

    private static string Template(
        string name,
        string title,
        string message,
        string ctaText,
        string link,
        string expiration,
        string appName) => $"""
        <!DOCTYPE html>
        <html lang="en">
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
                      <p style="margin:0 0 16px;">Hi <strong>{name}</strong>,</p>
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
                        If the button doesn't work, copy and paste the following link into your browser:
                      </p>
                      <p style="margin:0 0 16px;padding-bottom:16px;border-bottom:1px solid #e2e2e2;font-size:14px;word-break:break-all;">
                        <a href="{link}" style="color:#F22539;">{link}</a>
                      </p>
                      <p style="margin:0;font-size:14px;color:#888888;">
                        This link will expire in {expiration}. If you did not request this email, you can safely ignore it.
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
}
