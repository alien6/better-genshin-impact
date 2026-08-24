using System.Globalization;
using BetterGenshinImpact.GameTask.AutoTrackPath;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoTrackPathTests;

public class TrackPathTextRecognizerTests
{
    [Theory]
    [InlineData("pt-BR", "“Bule de Relachá”", "尘歌壶")]
    [InlineData("pt-BR", "NOD — KRAI", "挪德卡莱")]
    [InlineData("en", "Mondstadt", "蒙德")]
    [InlineData("zh-Hant", "「楓丹」", "枫丹")]
    public void IsSwitchAreaCandidateMatch_MatchesLocalizedOcrArea(
        string culture,
        string recognizedText,
        string routeAreaName)
    {
        var sut = new TrackPathTextRecognizer(CreateMatcher(culture));

        Assert.True(sut.IsSwitchAreaCandidateMatch(recognizedText, routeAreaName));
    }

    [Fact]
    public void IsSwitchAreaCandidateMatch_PreservesUnknownRouteAreaLiteralFallback()
    {
        var sut = new TrackPathTextRecognizer(CreateMatcher("pt-BR"));

        Assert.True(sut.IsSwitchAreaCandidateMatch("“层岩巨渊”", "层岩巨渊"));
        Assert.False(sut.IsSwitchAreaCandidateMatch("Liyue", "层岩巨渊"));
    }

    [Fact]
    public void IsSwitchAreaCandidateMatch_KnownAreaDoesNotAcceptInternalRouteLabel()
    {
        var sut = new TrackPathTextRecognizer(CreateMatcher("pt-BR"));

        Assert.False(sut.IsSwitchAreaCandidateMatch("蒙德", "蒙德"));
    }

    [Theory]
    [MemberData(nameof(LegacyAreaResourceValues))]
    public void Catalog_PreservesEveryLegacyAreaResourceValue(
        string culture,
        string key,
        string legacyValue)
    {
        var matcher = CreateMatcher(culture);

        Assert.Contains(legacyValue, matcher.GetAliases(key));
    }

    public static IEnumerable<object[]> LegacyAreaResourceValues()
    {
        var values = new Dictionary<string, string[]>
        {
            ["world_area.serenitea_pot"] = ["尘歌壶", "塵歌壺", "Serenitea Pot", "Sérénithéière"],
            ["world_area.nod_krai"] = ["挪德卡莱", "挪德卡萊", "Nod-Krai", "Nod-Krai"],
            ["world_area.snezhnaya"] = ["至冬", "至冬", "Snezhnaya", "Snezhnaya"],
            ["world_area.fontaine"] = ["枫丹", "楓丹", "Fontaine", "Fontaine"],
            ["world_area.liyue"] = ["璃月", "璃月", "Liyue", "Liyue"],
            ["world_area.inazuma"] = ["稻妻", "稻妻", "Inazuma", "Inazuma"],
            ["world_area.natlan"] = ["纳塔", "納塔", "Natlan", "Natlan"],
            ["world_area.mondstadt"] = ["蒙德", "蒙德", "Mondstadt", "Mondstadt"],
            ["world_area.sumeru"] = ["须弥", "須彌", "Sumeru", "Sumeru"]
        };
        var cultures = new[] { "zh-Hans", "zh-Hant", "en", "fr" };

        foreach (var (key, aliases) in values)
        {
            for (var index = 0; index < cultures.Length; index++)
            {
                yield return [cultures[index], key, aliases[index]];
            }
        }
    }

    private static GameTextMatcher CreateMatcher(string culture) =>
        new(new FixedCultureProvider(CultureInfo.GetCultureInfo(culture)), new EmbeddedGameTextCatalogProvider());

    private sealed class FixedCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
