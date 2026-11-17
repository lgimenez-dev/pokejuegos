using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.SmtpConfigs;

/// <summary>
/// Validates <see cref="UpdateSmtpConfigRequest"/> before the service layer is invoked.
/// </summary>
public class UpdateSmtpConfigValidator : AbstractValidator<UpdateSmtpConfigRequest>
{
    /// <summary>
    /// Initializes a new instance of <see cref="UpdateSmtpConfigValidator"/> with all field rules.
    /// </summary>
    public UpdateSmtpConfigValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired);

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage(ErrorMessages.InvalidPort);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired)
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmail);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired);

        RuleFor(x => x.SenderName)
            .NotEmpty().WithMessage(ErrorMessages.FieldRequired);
    }
}
