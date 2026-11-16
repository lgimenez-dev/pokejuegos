using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Offline <see cref="IAuthService"/>: accepts any non-empty credentials without calling the backend.
/// Swap it in for <see cref="HttpAuthService"/> in <c>MauiProgram</c> to work on screens without the API running.
/// </summary>
public class MockAuthService : IAuthService
{
	// Short delay so the loading state is visible, like a real request.
	private static readonly TimeSpan FakeLatency = TimeSpan.FromMilliseconds(600);

	private const string MockEmail = "ash@pokejuegos.local";

	/// <inheritdoc />
	public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
	{
		await Task.Delay(FakeLatency, ct);

		string? identifier = request.Email ?? request.Username;
		if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrEmpty(request.Password))
		{
			return AuthResult<LoginResponse>.Failure("Invalid email or password.");
		}

		string email = request.Email ?? $"{request.Username}@pokejuegos.local";
		return AuthResult<LoginResponse>.Success(new LoginResponse("mock-token", email, "Member", ProfileComplete: true));
	}

	/// <inheritdoc />
	public async Task<AuthResult<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
	{
		await Task.Delay(FakeLatency, ct);
		return AuthResult<bool>.Success(true);
	}

	/// <inheritdoc />
	public async Task<AuthResult<LoginResponse>?> LoginWithGoogleAsync(CancellationToken ct = default)
	{
		await Task.Delay(FakeLatency, ct);

		// Like a first Google sign-in: the profile still needs a username.
		return AuthResult<LoginResponse>.Success(new LoginResponse("mock-token", MockEmail, "Member", ProfileComplete: false));
	}

	/// <inheritdoc />
	public async Task<AuthResult<MeResponse>> GetMeAsync(CancellationToken ct = default)
	{
		await Task.Delay(FakeLatency, ct);
		return AuthResult<MeResponse>.Success(new MeResponse("mock-user", "Ash Ketchum", MockEmail, "Member"));
	}

	/// <inheritdoc />
	public async Task<AuthResult<bool>> CompleteProfileAsync(CompleteProfileRequest request, CancellationToken ct = default)
	{
		await Task.Delay(FakeLatency, ct);
		return AuthResult<bool>.Success(true);
	}

	/// <inheritdoc />
	public async Task<AuthResult<bool>> RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken ct = default)
	{
		await Task.Delay(FakeLatency, ct);
		return AuthResult<bool>.Success(true);
	}
}
