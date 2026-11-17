using Microsoft.Extensions.Localization;

namespace PokeJuegos.Web;

/// <summary>
/// Marker type for the page texts in <c>Resources/SharedResource.resx</c> (English) and <c>SharedResource.es.resx</c>.
/// </summary>
public sealed class SharedResource
{
}

/// <summary>
/// Helpers to show error codes (from the API or this site) as translated messages.
/// </summary>
public static class LocalizerExtensions
{
    /// <summary>
    /// Translates an error code (e.g. <c>INVALID_OR_EXPIRED_TOKEN</c>) through its <c>Error_&lt;CODE&gt;</c> resource.
    /// </summary>
    /// <param name="localizer">The page texts.</param>
    /// <param name="code">The error code.</param>
    /// <returns>The message in the current language, or the generic error if the code has no text.</returns>
    public static string Error(this IStringLocalizer<SharedResource> localizer, string code)
    {
        LocalizedString text = localizer["Error_" + code];
        return text.ResourceNotFound ? localizer["Error_UNEXPECTED_ERROR"] : text;
    }
}
