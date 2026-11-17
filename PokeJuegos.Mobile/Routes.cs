namespace PokeJuegos.Mobile;

/// <summary>
/// Shell route names. <c>Welcome</c> and <c>General</c> are root <see cref="ShellContent"/>s
/// (navigate with <c>//</c>); the rest are pushed on top via <c>Routing.RegisterRoute</c> in <see cref="AppShell"/>.
/// </summary>
public static class Routes
{
	public const string Welcome = "welcome";
	public const string General = "general";
	public const string Login = "login";
	public const string Register = "register";
	public const string ForgotPassword = "forgot-password";
	public const string CompleteProfile = "complete-profile";
	public const string Pokedex = "pokedex";
	public const string Roulette = "roulette";
	public const string Settings = "settings";
}
