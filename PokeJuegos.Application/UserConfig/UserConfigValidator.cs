using PokeJuegos.DTO;
using FluentValidation;

namespace PokeJuegos.Application.UserConfigs;

/// <summary>
/// Validates <see cref="UserConfigRequest"/> before it is passed to <see cref="IUserConfigService.SyncAsync"/>.
/// </summary>
public sealed class UserConfigValidator : AbstractValidator<UserConfigRequest>
{
    private static readonly string[] SupportedLanguages = ["en", "es"];

    /// <summary>
    /// Initializes a new instance of <see cref="UserConfigValidator"/> with the required rules.
    /// The language is <c>en</c>, <c>es</c> or <see langword="null"/> (follow the device), and the change date is required.
    /// </summary>
    public UserConfigValidator()
    {
        RuleFor(x => x.Language)
            .Must(language => language is null || SupportedLanguages.Contains(language))
            .WithMessage(ErrorMessages.InvalidLanguage);
        RuleFor(x => x.UpdatedAt).NotEmpty().WithMessage(ErrorMessages.FieldRequired);
    }
}
