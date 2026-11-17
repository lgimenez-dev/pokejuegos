using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PokeJuegos.Web.Pages;

/// <summary>
/// Landing page: the bare <c>App:FrontendUrl</c>, also where the API's Google sign-in redirects browsers on failure.
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>The error code the API's Google callback appends as <c>?oauth_error=…</c>, if any.</summary>
    [BindProperty(SupportsGet = true, Name = "oauth_error")]
    public string? OAuthError { get; set; }
}
