using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Validates <see cref="LoginRequest"/> before it is passed to <see cref="IAuthService.LoginAsync"/>.
/// </summary>
public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="LoginValidator"/> with the required rules.
    /// </summary>
    public LoginValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) ^ !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("Provide either 'Email' or 'Username', not both or neither.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Username)
            .Matches("^[A-Za-z0-9._-]{3,32}$")
            .WithMessage("'Username' must be 3-32 characters and contain only letters, digits, dots, underscores, and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));
        RuleFor(x => x.Password).NotEmpty();
    }
}
