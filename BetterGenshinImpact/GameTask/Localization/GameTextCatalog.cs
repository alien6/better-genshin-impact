using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace BetterGenshinImpact.GameTask.Localization;

public sealed class GameTextCatalog
{
    private readonly FrozenDictionary<string, CatalogEntry> _entries;

    public GameTextCatalog(string cultureName, IReadOnlyDictionary<string, IReadOnlyList<string>> entries)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            throw new ArgumentException("A catalog culture is required.", nameof(cultureName));
        }

        ArgumentNullException.ThrowIfNull(entries);

        CultureName = cultureName;
        _entries = entries.ToFrozenDictionary(
            pair => ValidateKey(pair.Key),
            pair => CreateEntry(pair.Key, pair.Value),
            StringComparer.Ordinal);
    }

    public string CultureName { get; }

    internal bool TryGetAliases(string key, out IReadOnlyList<string> aliases) =>
        TryGetEntry(key, out var entry, out aliases, out _);

    internal bool TryGetNormalizedAliases(string key, out IReadOnlyList<string> normalizedAliases) =>
        TryGetEntry(key, out var entry, out _, out normalizedAliases);

    private bool TryGetEntry(
        string key,
        out CatalogEntry? entry,
        out IReadOnlyList<string> aliases,
        out IReadOnlyList<string> normalizedAliases)
    {
        if (_entries.TryGetValue(key, out entry))
        {
            aliases = entry.Aliases;
            normalizedAliases = entry.NormalizedAliases;
            return true;
        }

        aliases = Array.Empty<string>();
        normalizedAliases = Array.Empty<string>();
        return false;
    }

    private static string ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Catalog keys cannot be empty.", nameof(key));
        }

        return key;
    }

    private static CatalogEntry CreateEntry(string key, IReadOnlyList<string> aliases)
    {
        ArgumentNullException.ThrowIfNull(aliases);

        var rawAliases = new List<string>(aliases.Count);
        var normalizedAliases = new List<string>(aliases.Count);
        var seenRawAliases = new HashSet<string>(StringComparer.Ordinal);
        var seenNormalizedAliases = new HashSet<string>(StringComparer.Ordinal);

        foreach (var alias in aliases)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException($"Catalog key '{key}' contains an empty alias.", nameof(aliases));
            }

            var normalizedAlias = GameTextNormalizer.Normalize(alias);
            if (normalizedAlias.Length == 0)
            {
                throw new ArgumentException($"Catalog key '{key}' contains an alias without comparable text.", nameof(aliases));
            }

            if (!seenRawAliases.Add(alias))
            {
                throw new ArgumentException($"Catalog key '{key}' contains duplicate raw alias '{alias}'.", nameof(aliases));
            }

            rawAliases.Add(alias);
            if (seenNormalizedAliases.Add(normalizedAlias))
            {
                normalizedAliases.Add(normalizedAlias);
            }
        }

        if (rawAliases.Count == 0)
        {
            throw new ArgumentException($"Catalog key '{key}' must contain at least one alias.", nameof(aliases));
        }

        return new CatalogEntry(Array.AsReadOnly(rawAliases.ToArray()), Array.AsReadOnly(normalizedAliases.ToArray()));
    }

    private sealed record CatalogEntry(IReadOnlyList<string> Aliases, IReadOnlyList<string> NormalizedAliases);
}
