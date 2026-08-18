namespace BetterGenshinImpact.Core.Localization;

/// <summary>
/// Cultures that BetterGI exposes in both the UI-language and game-language selectors.
/// Keep this list centralized so OCR, UI translation and script localization can evolve together.
/// </summary>
public static class SupportedCultures
{
    public static readonly string[] Names = ["zh-Hans", "zh-Hant", "en", "ja", "pt-BR"];
}
