using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Resources.Strings;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

public partial class RegisterViewModel(IAuthService authService, INavigationService navigation) : ObservableObject
{
	[ObservableProperty]
	public partial string Name { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Username { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Email { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Password { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string ConfirmPassword { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string NameError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string UsernameError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string EmailError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string PasswordError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string ConfirmPasswordError { get; set; } = string.Empty;

	/// <summary>Errors not tied to a field (server unreachable, unexpected response…).</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasError))]
	public partial string ErrorMessage { get; set; } = string.Empty;

	/// <summary>The "check your email" modal after a successful registration.</summary>
	[ObservableProperty]
	public partial bool IsSuccessOpen { get; set; }

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	partial void OnNameChanged(string value) => NameError = ErrorMessage = string.Empty;

	partial void OnUsernameChanged(string value) => UsernameError = ErrorMessage = string.Empty;

	partial void OnEmailChanged(string value) => EmailError = ErrorMessage = string.Empty;

	partial void OnPasswordChanged(string value) => PasswordError = ErrorMessage = string.Empty;

	partial void OnConfirmPasswordChanged(string value) => ConfirmPasswordError = ErrorMessage = string.Empty;

	[RelayCommand]
	private async Task RegisterAsync()
	{
		ErrorMessage = string.Empty;
		if (!Validate())
		{
			return;
		}

		RegisterRequest request = new(
			Email: Email.Trim(),
			Password: Password,
			Name: Name.Trim(),
			Username: Username.Trim(),
			NationalId: null,
			Phone: null,
			BirthDate: null);

		AuthResult<bool> result = await authService.RegisterAsync(request);
		if (result.IsSuccess)
		{
			IsSuccessOpen = true;
			return;
		}

		ShowServerErrors(result);
	}

	[RelayCommand]
	private Task BackToLoginAsync() => navigation.GoBackAsync();

	[RelayCommand]
	private Task CloseSuccessAsync()
	{
		IsSuccessOpen = false;
		return navigation.GoBackAsync();
	}

	/// <summary>
	/// Same rules as <c>RegisterValidator</c> in the API, plus a required username
	/// (needed to appear in the rankings) and the password confirmation.
	/// </summary>
	private bool Validate()
	{
		NameError = AuthValidation.Name(Name);
		UsernameError = AuthValidation.Username(Username);
		EmailError = AuthValidation.Email(Email);
		PasswordError = AuthValidation.Password(Password);
		ConfirmPasswordError = ConfirmPassword == Password ? string.Empty : AppResources.Register_PasswordsDontMatch;

		return NameError.Length == 0
			&& UsernameError.Length == 0
			&& EmailError.Length == 0
			&& PasswordError.Length == 0
			&& ConfirmPasswordError.Length == 0;
	}

	private void ShowServerErrors(AuthResult<bool> result)
	{
		foreach ((string field, string message) in result.FieldErrors)
		{
			switch (field)
			{
				case nameof(RegisterRequest.Name): NameError = message; break;
				case nameof(RegisterRequest.Username): UsernameError = message; break;
				case nameof(RegisterRequest.Email): EmailError = message; break;
				case nameof(RegisterRequest.Password): PasswordError = message; break;
				default: ErrorMessage = message; break;
			}
		}

		switch (result.ErrorCode)
		{
			case null: break;
			case ErrorText.EmailAlreadyRegistered: EmailError = result.Error!; break;
			case ErrorText.UsernameAlreadyTaken: UsernameError = result.Error!; break;
			default: ErrorMessage = result.Error!; break;
		}
	}
}
