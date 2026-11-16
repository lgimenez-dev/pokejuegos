using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokeJuegos.Web.Services;

namespace PokeJuegos.Web.Pages;

/// <summary>
/// Target of the password-recovery email link (<c>/reset-password?token=…</c>).
/// Password rules are enforced by the API; only the confirmation match is checked here.
/// </summary>
/// <param name="api">The client for <c>PokeJuegos.Api</c>.</param>
public class ResetPasswordModel(AuthApiClient api) : PageModel
{
    /// <summary>The recovery token, from the link's query string (and the form's hidden field on POST).</summary>
    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    /// <summary>The new password.</summary>
    [BindProperty]
    public string? Password { get; set; }

    /// <summary>The new password, typed again.</summary>
    [BindProperty]
    public string? ConfirmPassword { get; set; }

    /// <summary>Whether the API accepted the new password.</summary>
    public bool Completed { get; private set; }

    /// <summary>A general error shown above the form (invalid/expired token, API unreachable…).</summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Saves the new password through the API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task OnPostAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            return;
        }

        if (Password != ConfirmPassword)
        {
            ModelState.AddModelError(nameof(ConfirmPassword), "Passwords don't match.");
            return;
        }

        ApiResult result = await api.ChangePasswordAsync(Token, Password ?? string.Empty, ct);
        if (result.IsSuccess)
        {
            Completed = true;
            return;
        }

        Error = result.Error;
        foreach ((string field, string message) in result.FieldErrors)
        {
            if (field == nameof(Password))
            {
                ModelState.AddModelError(nameof(Password), message);
            }
            else
            {
                // e.g. Token: not an input of this form.
                Error ??= message;
            }
        }
    }
}
