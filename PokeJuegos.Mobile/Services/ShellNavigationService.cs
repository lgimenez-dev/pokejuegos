namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="INavigationService"/> backed by the app's <see cref="Shell"/>.
/// </summary>
public class ShellNavigationService : INavigationService
{
	/// <inheritdoc />
	public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

	/// <inheritdoc />
	public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
