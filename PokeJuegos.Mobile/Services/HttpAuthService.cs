using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="IAuthService"/> against <c>/api/auth</c> of <c>PokeJuegos.Api</c>.
/// The API answers errors as codes; <see cref="AuthResult{T}"/> translates them.
/// </summary>
public class HttpAuthService(ISessionService session, IOAuthBrowser browser) : IAuthService
{
	private const string ServerUnreachable = ErrorText.ServerUnreachable;
	private const string UnexpectedError = ErrorText.UnexpectedError;
	private const string TooManyAttempts = ErrorText.TooManyAttempts;
	private const string SessionExpired = ErrorText.SessionExpired;

	private readonly HttpClient http = new(new AcceptLanguageHandler())
	{
		BaseAddress = new Uri(ApiSettings.BaseUrl),
		Timeout = TimeSpan.FromSeconds(15),
	};

	/// <inheritdoc />
	public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
	{
		try
		{
			using HttpResponseMessage response = await http.PostAsJsonAsync("api/auth/login", request, AuthJsonContext.Default.LoginRequest, ct);
			if (!response.IsSuccessStatusCode)
			{
				return await ReadFailureAsync<LoginResponse>(response, ct);
			}

			LoginResponse? login = await response.Content.ReadFromJsonAsync(AuthJsonContext.Default.LoginResponse, ct);
			if (login is null)
			{
				return AuthResult<LoginResponse>.Failure(UnexpectedError);
			}

			await session.SetAsync(login.Token, login.Email);
			return AuthResult<LoginResponse>.Success(login);
		}
		catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
		{
			return AuthResult<LoginResponse>.Failure(ServerUnreachable);
		}
	}

	/// <inheritdoc />
	public async Task<AuthResult<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
	{
		try
		{
			using HttpResponseMessage response = await http.PostAsJsonAsync("api/auth/register", request, AuthJsonContext.Default.RegisterRequest, ct);
			return response.IsSuccessStatusCode
				? AuthResult<bool>.Success(true)
				: await ReadFailureAsync<bool>(response, ct);
		}
		catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
		{
			return AuthResult<bool>.Failure(ServerUnreachable);
		}
	}

	/// <inheritdoc />
	public async Task<AuthResult<bool>> RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken ct = default)
	{
		try
		{
			using HttpResponseMessage response = await http.PostAsJsonAsync("api/auth/recover-password", request, AuthJsonContext.Default.RecoverPasswordRequest, ct);
			return response.IsSuccessStatusCode
				? AuthResult<bool>.Success(true)
				: await ReadFailureAsync<bool>(response, ct);
		}
		catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
		{
			return AuthResult<bool>.Failure(ServerUnreachable);
		}
	}

	/// <inheritdoc />
	public async Task<AuthResult<LoginResponse>?> LoginWithGoogleAsync(CancellationToken ct = default)
	{
		IReadOnlyDictionary<string, string>? redirect = await browser.AuthenticateAsync(
			returnUrl => new Uri(http.BaseAddress!, $"api/auth/oauth2/google?returnUrl={Uri.EscapeDataString(returnUrl)}"),
			ct);
		if (redirect is null)
		{
			return null;
		}

		// e.g. the email already belongs to a password account (PROVIDER_MISMATCH).
		if (redirect.TryGetValue("error", out string? error))
		{
			return AuthResult<LoginResponse>.Failure(error);
		}

		if (!redirect.TryGetValue("access_token", out string? token) || string.IsNullOrEmpty(token))
		{
			return AuthResult<LoginResponse>.Failure(UnexpectedError);
		}

		// The redirect only carries the token: the email and role come from /me.
		AuthResult<MeResponse> me = await GetMeAsync(token, ct);
		if (!me.IsSuccess)
		{
			return AuthResult<LoginResponse>.Failure(me.ErrorCode ?? UnexpectedError);
		}

		bool profileComplete = redirect.TryGetValue("profile_complete", out string? flag) && flag == "true";
		await session.SetAsync(token, me.Value!.Email);
		return AuthResult<LoginResponse>.Success(new LoginResponse(token, me.Value.Email, me.Value.Role, profileComplete));
	}

	/// <inheritdoc />
	public async Task<AuthResult<MeResponse>> GetMeAsync(CancellationToken ct = default)
	{
		string? token = await session.GetTokenAsync();
		return token is null
			? AuthResult<MeResponse>.Failure(SessionExpired)
			: await GetMeAsync(token, ct);
	}

	/// <inheritdoc />
	public async Task<AuthResult<bool>> CompleteProfileAsync(CompleteProfileRequest request, CancellationToken ct = default)
	{
		string? token = await session.GetTokenAsync();
		if (token is null)
		{
			return AuthResult<bool>.Failure(SessionExpired);
		}

		try
		{
			using HttpRequestMessage message = new(HttpMethod.Put, "api/auth/profile")
			{
				Content = JsonContent.Create(request, AuthJsonContext.Default.CompleteProfileRequest),
			};
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using HttpResponseMessage response = await http.SendAsync(message, ct);
			return response.IsSuccessStatusCode
				? AuthResult<bool>.Success(true)
				: await ReadFailureAsync<bool>(response, ct);
		}
		catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
		{
			return AuthResult<bool>.Failure(ServerUnreachable);
		}
	}

	private async Task<AuthResult<MeResponse>> GetMeAsync(string token, CancellationToken ct)
	{
		try
		{
			using HttpRequestMessage message = new(HttpMethod.Get, "api/auth/me");
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using HttpResponseMessage response = await http.SendAsync(message, ct);
			if (!response.IsSuccessStatusCode)
			{
				return await ReadFailureAsync<MeResponse>(response, ct);
			}

			MeResponse? me = await response.Content.ReadFromJsonAsync(AuthJsonContext.Default.MeResponse, ct);
			return me is null
				? AuthResult<MeResponse>.Failure(UnexpectedError)
				: AuthResult<MeResponse>.Success(me);
		}
		catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
		{
			return AuthResult<MeResponse>.Failure(ServerUnreachable);
		}
	}

	/// <summary>
	/// Maps an error response: <c>{ errors: [{ field, message }] }</c> (validation) or <c>{ message }</c> (business rule).
	/// </summary>
	private static async Task<AuthResult<T>> ReadFailureAsync<T>(HttpResponseMessage response, CancellationToken ct)
	{
		if (response.StatusCode == HttpStatusCode.TooManyRequests)
		{
			return AuthResult<T>.Failure(TooManyAttempts);
		}

		try
		{
			string body = await response.Content.ReadAsStringAsync(ct);
			using JsonDocument document = JsonDocument.Parse(body);
			JsonElement root = document.RootElement;

			// ASP.NET's own ProblemDetails also has "errors", but as an object: only the API's array shape is read.
			if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Array)
			{
				ValidationErrorResponse? validation = root.Deserialize(AuthJsonContext.Default.ValidationErrorResponse);
				return ToInvalid<T>(validation?.Errors ?? []);
			}

			if (root.TryGetProperty("message", out _))
			{
				BusinessErrorResponse? business = root.Deserialize(AuthJsonContext.Default.BusinessErrorResponse);
				if (!string.IsNullOrEmpty(business?.Message))
				{
					return AuthResult<T>.Failure(business.Message);
				}
			}
		}
		catch (JsonException)
		{
			// Not one of the API's error shapes (empty body, HTML from a proxy…).
		}

		// [Authorize] endpoints answer 401 with an empty body when the token is missing or expired.
		return AuthResult<T>.Failure(response.StatusCode == HttpStatusCode.Unauthorized ? SessionExpired : UnexpectedError);
	}

	private static AuthResult<T> ToInvalid<T>(IReadOnlyList<FieldError> errors)
	{
		Dictionary<string, string> fieldErrors = [];
		string? general = null;
		foreach (FieldError error in errors)
		{
			if (string.IsNullOrEmpty(error.Field))
			{
				general ??= error.Message;
			}
			else
			{
				fieldErrors.TryAdd(error.Field, error.Message);
			}
		}

		return fieldErrors.Count == 0
			? AuthResult<T>.Failure(general ?? UnexpectedError)
			: AuthResult<T>.Invalid(fieldErrors, general);
	}
}
