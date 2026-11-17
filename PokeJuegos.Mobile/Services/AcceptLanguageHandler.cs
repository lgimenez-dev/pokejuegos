using System.Net.Http.Headers;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Sends the app's language as <c>Accept-Language</c> on every API call, so the emails the API sends
/// (activation, password recovery) come in the language the user sees.
/// </summary>
public class AcceptLanguageHandler() : DelegatingHandler(new HttpClientHandler())
{
	/// <inheritdoc />
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.Headers.AcceptLanguage.Clear();
		request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(AppLanguage.Current));
		return base.SendAsync(request, cancellationToken);
	}
}
