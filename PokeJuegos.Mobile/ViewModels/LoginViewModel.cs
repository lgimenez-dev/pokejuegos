using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

public partial class LoginViewModel(IAuthService authService, INavigationService navigation) : ObservableObject
{
	/// <summary>Username or email: the backend accepts either one in <see cref="LoginRequest"/>.</summary>
	[ObservableProperty]
	public partial string Identifier { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Password { get; set; } = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasError))]
	public partial string ErrorMessage { get; set; } = string.Empty;

	/// <summary>A sign-in (password or Google) is in progress: the other one stays disabled.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsIdle))]
	[NotifyCanExecuteChangedFor(nameof(LoginCommand), nameof(GoogleLoginCommand))]
	public partial bool IsBusy { get; set; }

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	public bool IsIdle => !IsBusy;

	partial void OnIdentifierChanged(string value) => ErrorMessage = string.Empty;

	partial void OnPasswordChanged(string value) => ErrorMessage = string.Empty;

	// AsyncRelayCommand disables the button while it runs, which also prevents double submits.
	[RelayCommand(CanExecute = nameof(IsIdle))]
	private async Task LoginAsync()
	{
		string identifier = Identifier.Trim();
		if (identifier.Length == 0 || Password.Length == 0)
		{
			ErrorMessage = "Enter your username and password.";
			return;
		}

		bool isEmail = identifier.Contains('@');
		LoginRequest request = new(
			Email: isEmail ? identifier : null,
			Username: isEmail ? null : identifier,
			Password: Password);

		IsBusy = true;
		try
		{
			AuthResult<LoginResponse> result = await authService.LoginAsync(request);
			if (!result.IsSuccess)
			{
				// Invalid credentials, pending/inactive account, rate limit, server down…
				ErrorMessage = result.Error ?? result.FieldErrors.Values.First();
				return;
			}

			Password = string.Empty;
			await navigation.GoToAsync($"//{Routes.General}");
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Google sign-in in the system browser. <c>GoogleLoginCancelCommand</c> stops the wait: on Windows
	/// the app can't tell when the user just closes the browser tab.
	/// </summary>
	[RelayCommand(CanExecute = nameof(IsIdle), IncludeCancelCommand = true)]
	private async Task GoogleLoginAsync(CancellationToken ct)
	{
		ErrorMessage = string.Empty;
		IsBusy = true;
		try
		{
			AuthResult<LoginResponse>? result = await authService.LoginWithGoogleAsync(ct);
			if (result is null || ct.IsCancellationRequested)
			{
				// Cancelled by the user: nothing to report.
				return;
			}

			if (!result.IsSuccess)
			{
				ErrorMessage = result.Error ?? result.FieldErrors.Values.First();
				return;
			}

			// First Google sign-in: the account has no username yet.
			await navigation.GoToAsync(result.Value!.ProfileComplete ? $"//{Routes.General}" : Routes.CompleteProfile);
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private Task GoToRegisterAsync() => navigation.GoToAsync(Routes.Register);

	[RelayCommand]
	private Task GoToForgotPasswordAsync() => navigation.GoToAsync(Routes.ForgotPassword);
}
