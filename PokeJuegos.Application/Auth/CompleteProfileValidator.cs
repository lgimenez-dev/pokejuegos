using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Validates <see cref="CompleteProfileRequest"/> before it is passed to <see cref="IAuthService.CompleteProfileAsync"/>.
/// </summary>
public sealed class CompleteProfileValidator : AbstractValidator<CompleteProfileRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CompleteProfileValidator"/> with the required rules.
    /// Unlike in <see cref="RegisterValidator"/>, the username is mandatory: completing it is the point of this request.
    /// </summary>
    public CompleteProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(ErrorMessages.FieldRequired);
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired)
            .Matches("^[A-Za-z0-9._-]{3,32}$").WithMessage(ErrorMessages.InvalidUsername);
    }
}
