namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Route-based navigation, so ViewModels don't depend on <see cref="Shell"/> directly.
/// </summary>
public interface INavigationService
{
	/// <summary>
	/// Navigates to a Shell route (see <see cref="Routes"/>).
	/// </summary>
	/// <param name="route">A registered route; prefix it with <c>//</c> to reset the navigation stack.</param>
	/// <returns>A task that completes when the navigation has finished.</returns>
	Task GoToAsync(string route);

	/// <summary>
	/// Pops the current page off the navigation stack.
	/// </summary>
	/// <returns>A task that completes when the navigation has finished.</returns>
	Task GoBackAsync();
}
