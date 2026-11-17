using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Validates <see cref="ActivateAccountRequest"/> before it is passed to <see cref="IAuthService.ActivateAsync"/>.
/// </summary>
public sealed class ActivateAccountValidator : AbstractValidator<ActivateAccountRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ActivateAccountValidator"/> with the required rules.
    /// </summary>
    public ActivateAccountValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(ErrorMessages.FieldRequired);
    }
}
