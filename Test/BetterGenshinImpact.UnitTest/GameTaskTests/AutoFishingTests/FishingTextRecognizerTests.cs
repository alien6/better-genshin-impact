using System.Globalization;
using BetterGenshinImpact.GameTask.AutoFishing;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoFishingTests;

public class FishingTextRecognizerTests
{
    [Theory]
    [InlineData("pt-BR", "O PEIXE MORDEU A ISCA!", "Mordeu")]
    [InlineData("en", "A fish got a BITE!", "bite")]
    [InlineData("zh-Hans", "鱼儿上钩了", "上钩")]
    public void IsBite_MatchesLocalizedOcrText(string culture, string recognizedText, string alias)
    {
        var sut = new FishingTextRecognizer(
            GameTextTestFactory.Create(
                culture,
                ("fishing.bite", alias),
                ("fishing.action", "Fishing")));

        Assert.True(sut.IsBite(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "[F] Pescar")]
    [InlineData("fr", "Pêcher")]
    [InlineData("fr", "Pécher")]
    public void FishingSearchPattern_MatchesLocalizedInteractionPrompt(string culture, string recognizedText)
    {
        var sut = CreateFromEmbeddedCatalog(culture);

        Assert.Matches(sut.FishingSearchPattern, recognizedText);
        Assert.True(sut.IsFishingPrompt(recognizedText));
    }

    [Theory]
    [InlineData("zh-Hans", "fishing.bite", "上钩")]
    [InlineData("zh-Hant", "fishing.bite", "上鉤")]
    [InlineData("en", "fishing.bite", "bite")]
    [InlineData("fr", "fishing.bite", "mordu")]
    [InlineData("zh-Hans", "fishing.action", "钓鱼")]
    [InlineData("zh-Hant", "fishing.action", "釣魚")]
    [InlineData("en", "fishing.action", "Fishing")]
    [InlineData("fr", "fishing.action", "P[êé]cher")]
    public void Catalog_PreservesEveryLegacyFishingResourceValue(
        string culture,
        string key,
        string legacyValue)
    {
        var matcher = CreateMatcher(culture);

        Assert.Contains(legacyValue, matcher.GetAliases(key));
    }

    private static FishingTextRecognizer CreateFromEmbeddedCatalog(string culture) =>
        new(CreateMatcher(culture));

    private static GameTextMatcher CreateMatcher(string culture) =>
        new(new FixedCultureProvider(CultureInfo.GetCultureInfo(culture)), new EmbeddedGameTextCatalogProvider());

    private sealed class FixedCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
