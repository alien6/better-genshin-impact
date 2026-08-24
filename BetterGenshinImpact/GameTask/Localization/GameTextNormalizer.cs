using System;
using System.Globalization;
using System.Text;

namespace BetterGenshinImpact.GameTask.Localization;

internal static class GameTextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormKC)
            .Normalize(NormalizationForm.FormD)
            .ToLowerInvariant();
        var builder = new StringBuilder(decomposed.Length);

        foreach (var rune in decomposed.EnumerateRunes())
        {
            var category = Rune.GetUnicodeCategory(rune);
            if (category is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            if (Rune.IsLetterOrDigit(rune))
            {
                builder.Append(rune.ToString());
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool Contains(string? recognizedText, string normalizedAlias) =>
        normalizedAlias.Length > 0
        && Normalize(recognizedText).Contains(normalizedAlias, StringComparison.Ordinal);
}
