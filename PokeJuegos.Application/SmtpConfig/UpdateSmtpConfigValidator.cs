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
            .NotEmpty().WithMessage("Host is required.");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.SenderName)
            .NotEmpty().WithMessage("Sender name is required.");
    }
}
