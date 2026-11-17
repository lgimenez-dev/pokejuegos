using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

/// <summary>
/// After the first Google sign-in: the account exists but has no username yet, which it needs to appear in the rankings.
/// </summary>
public partial class CompleteProfileViewModel(IAuthService authService, ISessionService session, INavigationService navigation) : ObservableObject
{
	[ObservableProperty]
	public partial string Name { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Username { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string NameError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string UsernameError { get; set; } = string.Empty;

	/// <summary>Errors not tied to a field (server unreachable, expired session…).</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasError))]
	public partial string ErrorMessage { get; set; } = string.Empty;

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	partial void OnNameChanged(string value) => NameError = ErrorMessage = string.Empty;

	partial void OnUsernameChanged(string value) => UsernameError = ErrorMessage = string.Empty;

	/// <summary>Prefills the name Google returned; runs when the page appears.</summary>
	[RelayCommand]
	private async Task LoadAsync()
	{
		if (Name.Length > 0)
		{
			return;
		}

		AuthResult<MeResponse> me = await authService.GetMeAsync();
		if (me.IsSuccess)
		{
			Name = me.Value!.Name;
		}
	}

	[RelayCommand]
	private async Task ContinueAsync()
	{
		ErrorMessage = string.Empty;
		NameError = AuthValidation.Name(Name);
		UsernameError = AuthValidation.Username(Username);
		if (NameError.Length > 0 || UsernameError.Length > 0)
		{
			return;
		}

		AuthResult<bool> result = await authService.CompleteProfileAsync(new CompleteProfileRequest(Username.Trim(), Name.Trim()));
		if (result.IsSuccess)
		{
			await navigation.GoToAsync($"//{Routes.General}");
			return;
		}

		foreach ((string field, string message) in result.FieldErrors)
		{
			switch (field)
			{
				case nameof(CompleteProfileRequest.Name): NameError = message; break;
				case nameof(CompleteProfileRequest.Username): UsernameError = message; break;
				default: ErrorMessage = message; break;
			}
		}

		switch (result.ErrorCode)
		{
			case null: break;
			case ErrorText.UsernameAlreadyTaken: UsernameError = result.Error!; break;
			default: ErrorMessage = result.Error!; break;
		}
	}

	/// <summary>
	/// Back to the login, signed out: the app never keeps a session without a username.
	/// Also runs on Android's back button.
	/// </summary>
	[RelayCommand]
	private Task CancelAsync()
	{
		session.Clear();
		return navigation.GoBackAsync();
	}
}
