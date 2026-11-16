namespace PokeJuegos.Application;

/// <summary>
/// Business error message constants used across all application services.
/// </summary>
public static class ErrorMessages
{
    // Auth
    public const string InvalidCredentials = "Invalid email or password.";
    public const string EmailAlreadyRegistered = "An account with this email address already exists.";
    public const string UsernameAlreadyRegistered = "This username is already taken.";
    public const string AccountPending = "Your account has not been activated yet. Please check your email for the activation link.";
    public const string AccountInactive = "Your account has been deactivated. Please contact support.";
    public const string InvalidOrExpiredToken = "The token is invalid or has expired.";
    public const string ProfileAlreadyComplete = "Your profile is already complete.";

    // OAuth2
    public const string ProviderMismatch = "An account already exists with this email address. Please sign in with your password.";
    public const string InvalidReturnUrl = "The return URL is not allowed.";

    // Sessions
    public const string UserNotFound = "User not found.";
    public const string SessionNotFound = "Session not found or already revoked.";

    // SMTP config
    public const string SmtpConfigNotFound = "SMTP configuration has not been set up yet.";
    public const string SmtpConfigAlreadyExists = "An SMTP configuration already exists. Use the update endpoint to modify it.";
}
