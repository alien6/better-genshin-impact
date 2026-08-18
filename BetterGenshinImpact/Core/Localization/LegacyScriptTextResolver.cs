using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BetterGenshinImpact.Core.Localization;

/// <summary>
/// Resolves historical Chinese literals embedded in legacy JavaScript scripts.
/// Semantic catalog entries are preferred; audited literal mappings are used as
/// a second source. Unknown text is preserved verbatim so the compatibility
/// layer is strictly additive and does not change legacy behavior.
/// </summary>
public static class LegacyScriptTextResolver
{
    public static IReadOnlyList<string> GetAll(string canonicalText, CultureInfo culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalText);

        foreach (var key in GameTextCatalog.Keys)
        {
            if (MatchesCanonicalText(key, canonicalText))
            {
                return GameTextCatalog.GetAll(key, culture);
            }
        }

        if (culture.Name.StartsWith("pt", StringComparison.OrdinalIgnoreCase)
            && GameLiteralCatalog.HasPortugueseMapping(canonicalText))
        {
            return GameLiteralCatalog.GetAll(canonicalText, culture);
        }

        return [canonicalText];
    }

    public static string Get(string canonicalText, CultureInfo culture) =>
        GetAll(canonicalText, culture)[0];

    private static bool MatchesCanonicalText(string key, string canonicalText)
    {
        var zhHans = GameTextCatalog.GetAll(key, CultureInfo.GetCultureInfo("zh-Hans"));
        if (zhHans.Contains(canonicalText, StringComparer.Ordinal))
        {
            return true;
        }

        var zhHant = GameTextCatalog.GetAll(key, CultureInfo.GetCultureInfo("zh-Hant"));
        return zhHant.Contains(canonicalText, StringComparer.Ordinal);
    }
}
