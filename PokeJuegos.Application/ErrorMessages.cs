namespace PokeJuegos.Application;

/// <summary>
/// Error codes returned by the API in <c>message</c> (business rules and validation failures).
/// They are UPPER_SNAKE_CASE keys, not sentences: each client (app, web) translates them to the user's language.
/// </summary>
public static class ErrorMessages
{
    // Auth
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
    public const string UsernameAlreadyTaken = "USERNAME_ALREADY_TAKEN";
    public const string AccountPending = "ACCOUNT_PENDING";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string InvalidOrExpiredToken = "INVALID_OR_EXPIRED_TOKEN";
    public const string ProfileAlreadyComplete = "PROFILE_ALREADY_COMPLETE";

    // OAuth2
    public const string ProviderMismatch = "PROVIDER_MISMATCH";
    public const string InvalidReturnUrl = "INVALID_RETURN_URL";

    // Sessions
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string SessionNotFound = "SESSION_NOT_FOUND";

    // SMTP config
    public const string SmtpConfigNotFound = "SMTP_CONFIG_NOT_FOUND";
    public const string SmtpConfigAlreadyExists = "SMTP_CONFIG_ALREADY_EXISTS";

    // Validation (the response also carries the field name)
    public const string FieldRequired = "FIELD_REQUIRED";
    public const string InvalidEmail = "INVALID_EMAIL";
    public const string PasswordTooShort = "PASSWORD_TOO_SHORT";
    public const string PasswordUppercaseRequired = "PASSWORD_UPPERCASE_REQUIRED";
    public const string PasswordDigitRequired = "PASSWORD_DIGIT_REQUIRED";
    public const string InvalidUsername = "INVALID_USERNAME";
    public const string EmailOrUsernameRequired = "EMAIL_OR_USERNAME_REQUIRED";
    public const string InvalidPort = "INVALID_PORT";
    public const string InvalidLanguage = "INVALID_LANGUAGE";

    // Unexpected
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
}
