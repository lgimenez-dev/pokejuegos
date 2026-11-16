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
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("'Password' must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("'Password' must contain at least one digit.");
    }
}
