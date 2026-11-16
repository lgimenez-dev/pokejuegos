namespace PokeJuegos.Api.Helpers;

/// <summary>
/// Validates the <c>returnUrl</c> that native clients pass to the Google OAuth2 flow,
/// so the endpoint can't be used to send a freshly issued token to an arbitrary site.
/// </summary>
public static class OAuthReturnUrl
{
    /// <summary>The key under which the validated URL travels in the OAuth2 <c>state</c>.</summary>
    public const string ItemKey = "returnUrl";

    private const string LoopbackPath = "/callback";

    /// <summary>
    /// Returns whether <paramref name="returnUrl"/> is either exactly <c>App:MobileRedirectUri</c>
    /// (the Android app's custom scheme) or <c>http://127.0.0.1:&lt;port&gt;/callback</c>
    /// (the Windows app's loopback listener, RFC 8252 §7.3).
    /// </summary>
    /// <param name="returnUrl">The URL requested by the client.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns><see langword="true"/> if the URL is allowed.</returns>
    public static bool IsAllowed(string returnUrl, IConfiguration configuration)
    {
        string? mobileRedirectUri = configuration["App:MobileRedirectUri"];
        if (!string.IsNullOrEmpty(mobileRedirectUri) && string.Equals(returnUrl, mobileRedirectUri, StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(returnUrl, UriKind.Absolute, out Uri? uri)
            && uri.Scheme == Uri.UriSchemeHttp
            && uri.Host == "127.0.0.1"
            && !uri.IsDefaultPort
            && uri.AbsolutePath == LoopbackPath
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment)
            && string.IsNullOrEmpty(uri.UserInfo);
    }
}
