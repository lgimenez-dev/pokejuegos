using Android.App;
using Android.Content;
using Android.Content.PM;

namespace PokeJuegos.Mobile;

/// <summary>
/// Receives the <c>pokejuegos://oauth?...</c> redirect that ends the Google sign-in and hands it to
/// <see cref="WebAuthenticator"/>, which completes the pending <c>AuthenticateAsync</c> call.
/// </summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
	[Intent.ActionView],
	Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
	DataScheme = "pokejuegos",
	DataHost = "oauth")]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity;
