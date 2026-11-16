using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

/// <summary>
/// Requests the password-recovery email. The link opens <c>PokeJuegos.Web</c>'s <c>/reset-password</c>, which also
/// lets an account that signed up with Google create its first password.
/// </summary>
public partial class ForgotPasswordViewModel(IAuthService authService, INavigationService navigation) : ObservableObject
{
	[ObservableProperty]
	public partial string Email { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string EmailError { get; set; } = string.Empty;

	/// <summary>Errors not tied to a field (server unreachable, too many attempts…).</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasError))]
	public partial string ErrorMessage { get; set; } = string.Empty;

	/// <summary>The "check your email" modal after the request.</summary>
	[ObservableProperty]
	public partial bool IsSuccessOpen { get; set; }

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	partial void OnEmailChanged(string value) => EmailError = ErrorMessage = string.Empty;

	[RelayCommand]
	private async Task SendAsync()
	{
		ErrorMessage = string.Empty;
		EmailError = AuthValidation.Email(Email);
		if (EmailError.Length > 0)
		{
			return;
		}

		AuthResult<bool> result = await authService.RecoverPasswordAsync(new RecoverPasswordRequest(Email.Trim()));
		if (result.IsSuccess)
		{
			// The API answers the same whether or not the account exists, so the message can't promise an email.
			IsSuccessOpen = true;
			return;
		}

		if (result.FieldErrors.TryGetValue(nameof(RecoverPasswordRequest.Email), out string? emailError))
		{
			EmailError = emailError;
		}
		else
		{
			ErrorMessage = result.Error ?? result.FieldErrors.Values.First();
		}
	}

	[RelayCommand]
	private Task BackToLoginAsync() => navigation.GoBackAsync();

	[RelayCommand]
	private Task CloseSuccessAsync()
	{
		IsSuccessOpen = false;
		return navigation.GoBackAsync();
	}
}
