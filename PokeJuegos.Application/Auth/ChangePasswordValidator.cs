using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Validates <see cref="ChangePasswordRequest"/> before it is passed to <see cref="IAuthService.ChangePasswordAsync"/>.
/// Password must be at least 8 characters and contain at least one uppercase letter and one digit.
/// </summary>
public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ChangePasswordValidator"/> with the required rules.
    /// </summary>
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(ErrorMessages.FieldRequired);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired)
            .MinimumLength(8).WithMessage(ErrorMessages.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(ErrorMessages.PasswordUppercaseRequired)
            .Matches("[0-9]").WithMessage(ErrorMessages.PasswordDigitRequired);
    }
}
