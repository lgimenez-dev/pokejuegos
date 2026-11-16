using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokeJuegos.Web.Services;

namespace PokeJuegos.Web.Pages;

/// <summary>
/// What <see cref="ActivateModel"/> shows.
/// </summary>
public enum ActivationState
{
    /// <summary>Token present, waiting for the user to press the button.</summary>
    Ready,

    /// <summary>The API activated the account.</summary>
    Activated,

    /// <summary>Missing, invalid or expired token, or the API couldn't be reached.</summary>
    Failed,
}

/// <summary>
/// Target of the registration email link (<c>/activate?token=…</c>).
/// Activation happens on the button's POST, not on page load: mail scanners that open links by themselves
/// (e.g. Outlook Safe Links) would otherwise consume the token before the user gets here.
/// </summary>
/// <param name="api">The client for <c>PokeJuegos.Api</c>.</param>
public class ActivateModel(AuthApiClient api) : PageModel
{
    private const string MissingToken = "This activation link is incomplete. Open it again from the email.";

    /// <summary>The activation token, from the link's query string (and the form's hidden field on POST).</summary>
    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    /// <summary>What the page shows.</summary>
    public ActivationState State { get; private set; }

    /// <summary>The error shown when <see cref="State"/> is <see cref="ActivationState.Failed"/>.</summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Shows the activation button, or the error if the link has no token.
    /// </summary>
    public void OnGet()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            Fail(MissingToken);
            return;
        }

        State = ActivationState.Ready;
    }

    /// <summary>
    /// Activates the account through the API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task OnPostAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            Fail(MissingToken);
            return;
        }

        ApiResult result = await api.ActivateAsync(Token, ct);
        if (result.IsSuccess)
        {
            State = ActivationState.Activated;
            return;
        }

        // The only validated field is the token itself: shown as a general error.
        Fail(result.Error ?? result.FieldErrors.Values.FirstOrDefault() ?? MissingToken);
    }

    private void Fail(string error)
    {
        State = ActivationState.Failed;
        Error = error;
    }
}
