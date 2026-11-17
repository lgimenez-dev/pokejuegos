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
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired)
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmail);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired)
            .MinimumLength(8).WithMessage(ErrorMessages.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(ErrorMessages.PasswordUppercaseRequired)
            .Matches("[0-9]").WithMessage(ErrorMessages.PasswordDigitRequired);
        RuleFor(x => x.Name).NotEmpty().WithMessage(ErrorMessages.FieldRequired);
        RuleFor(x => x.Username)
            .Matches("^[A-Za-z0-9._-]{3,32}$").WithMessage(ErrorMessages.InvalidUsername)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));
    }
}
