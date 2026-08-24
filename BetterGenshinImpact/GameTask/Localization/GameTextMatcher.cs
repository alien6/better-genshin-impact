using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BetterGenshinImpact.GameTask.Localization;

public sealed class GameTextMatcher : IGameTextMatcher
{
    private readonly IGameCultureProvider _cultureProvider;
    private readonly FrozenDictionary<string, GameTextCatalog> _catalogs;

    public GameTextMatcher(IGameCultureProvider cultureProvider, IGameTextCatalogProvider catalogProvider)
    {
        _cultureProvider = cultureProvider ?? throw new ArgumentNullException(nameof(cultureProvider));
        ArgumentNullException.ThrowIfNull(catalogProvider);
        _catalogs = catalogProvider.Catalogs.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null)
    {
        var requestedCulture = GetRequestedCulture(culture);
        var catalog = ResolveCatalog(requestedCulture);
        if (!catalog.TryGetAliases(key, out var aliases))
        {
            throw new KeyNotFoundException($"Game text key '{key}' was not found for culture '{requestedCulture.Name}'.");
        }

        return aliases;
    }

    public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null)
    {
        var normalizedText = GameTextNormalizer.Normalize(recognizedText);
        return normalizedText.Length > 0
            && GetNormalizedAliases(key, culture).Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
    }

    public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);

        var normalizedAliases = GetNormalizedAliases(key, culture);
        return recognizedTexts.Any(recognizedText =>
        {
            var normalizedText = GameTextNormalizer.Normalize(recognizedText);
            return normalizedText.Length > 0
                && normalizedAliases.Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
        });
    }

    public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);

        return IsMatch(string.Concat(recognizedTexts), key, culture);
    }

    private IReadOnlyList<string> GetNormalizedAliases(string key, CultureInfo? culture)
    {
        var requestedCulture = GetRequestedCulture(culture);
        var catalog = ResolveCatalog(requestedCulture);
        if (!catalog.TryGetNormalizedAliases(key, out var aliases))
        {
            throw new KeyNotFoundException($"Game text key '{key}' was not found for culture '{requestedCulture.Name}'.");
        }

        return aliases;
    }

    private CultureInfo GetRequestedCulture(CultureInfo? culture) =>
        culture ?? _cultureProvider.CurrentCulture ?? throw new InvalidOperationException("A game culture is required to resolve game text aliases.");

    private GameTextCatalog ResolveCatalog(CultureInfo requestedCulture)
    {
        if (_catalogs.TryGetValue(requestedCulture.Name, out var exactCatalog))
        {
            return exactCatalog;
        }

        var neutralCulture = requestedCulture.Parent;
        if (!Equals(neutralCulture, CultureInfo.InvariantCulture)
            && _catalogs.TryGetValue(neutralCulture.Name, out var neutralCatalog))
        {
            return neutralCatalog;
        }

        throw new InvalidOperationException($"No game text catalog is available for culture '{requestedCulture.Name}'.");
    }
}
