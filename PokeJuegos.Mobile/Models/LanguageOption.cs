namespace PokeJuegos.Mobile.Models;

/// <summary>One choice of the language picker in the settings.</summary>
/// <param name="Code"><c>en</c>, <c>es</c>, or <c>null</c> for the device language.</param>
/// <param name="Name">The text shown in the picker.</param>
public record LanguageOption(string? Code, string Name);
