using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Validates <see cref="RegisterRequest"/> before it is passed to <see cref="IAuthService.RegisterAsync"/>.
/// </summary>
public sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="RegisterValidator"/> with the required rules.
    /// Password must be at least 8 characters and contain at least one uppercase letter and one digit.
    /// </summary>
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("'Password' must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("'Password' must contain at least one digit.");
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Username)
            .Matches("^[A-Za-z0-9._-]{3,32}$")
            .WithMessage("'Username' must be 3-32 characters and contain only letters, digits, dots, underscores, and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));
    }
}
