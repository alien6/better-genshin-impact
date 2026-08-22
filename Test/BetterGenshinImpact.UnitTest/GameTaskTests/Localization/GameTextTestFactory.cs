using System.Collections.Frozen;
using System.Globalization;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

internal static class GameTextTestFactory
{
    public static GameTextMatcher Create(string cultureName, params (string Key, string Alias)[] entries) =>
        CreateForCultures(cultureName, entries.Select(entry => (cultureName, entry.Key, entry.Alias)).ToArray());

    public static GameTextMatcher CreateForCultures(
        string defaultCultureName,
        params (string CultureName, string Key, string Alias)[] entries)
    {
        var catalogs = entries
            .GroupBy(entry => entry.CultureName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new GameTextCatalog(
                group.Key,
                group.GroupBy(entry => entry.Key, StringComparer.Ordinal)
                    .ToDictionary(
                        keyGroup => keyGroup.Key,
                        keyGroup => (IReadOnlyList<string>)keyGroup.Select(entry => entry.Alias).ToArray(),
                        StringComparer.Ordinal)))
            .ToDictionary(catalog => catalog.CultureName, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        return new GameTextMatcher(
            new TestGameCultureProvider(CultureInfo.GetCultureInfo(defaultCultureName)),
            new TestGameTextCatalogProvider(catalogs));
    }

    private sealed class TestGameCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }

    private sealed class TestGameTextCatalogProvider(
        IReadOnlyDictionary<string, GameTextCatalog> catalogs) : IGameTextCatalogProvider
    {
        public IReadOnlyDictionary<string, GameTextCatalog> Catalogs { get; } = catalogs;
    }
}
