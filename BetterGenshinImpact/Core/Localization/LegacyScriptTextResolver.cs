using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace BetterGenshinImpact.Core.Localization;

/// <summary>
/// Resolves historical Chinese literals embedded in legacy JavaScript scripts.
/// Semantic catalog entries are preferred; audited literal mappings are used as
/// a second source. Unknown text is preserved verbatim so the compatibility
/// layer is strictly additive and does not change legacy behavior.
/// </summary>
public static class LegacyScriptTextResolver
{
    private static readonly FrozenDictionary<string, string> SemanticKeyByCanonicalText = BuildSemanticIndex();

    public static IReadOnlyList<string> GetAll(string canonicalText, CultureInfo culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalText);

        if (SemanticKeyByCanonicalText.TryGetValue(canonicalText, out var semanticKey))
        {
            return GameTextCatalog.GetAll(semanticKey, culture);
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

    private static FrozenDictionary<string, string> BuildSemanticIndex()
    {
        var index = new Dictionary<string, string>(StringComparer.Ordinal);
        var zhHans = CultureInfo.GetCultureInfo("zh-Hans");
        var zhHant = CultureInfo.GetCultureInfo("zh-Hant");

        foreach (var key in GameTextCatalog.Keys)
        {
            foreach (var canonical in GameTextCatalog.GetAll(key, zhHans))
            {
                index.TryAdd(canonical, key);
            }

            foreach (var canonical in GameTextCatalog.GetAll(key, zhHant))
            {
                index.TryAdd(canonical, key);
            }
        }

        return index.ToFrozenDictionary(StringComparer.Ordinal);
    }
}
