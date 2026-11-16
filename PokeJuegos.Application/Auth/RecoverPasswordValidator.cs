using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Validates <see cref="RecoverPasswordRequest"/> before it is passed to <see cref="IAuthService.RecoverPasswordAsync"/>.
/// </summary>
public sealed class RecoverPasswordValidator : AbstractValidator<RecoverPasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="RecoverPasswordValidator"/> with the required rules.
    /// </summary>
    public RecoverPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();    }
}
