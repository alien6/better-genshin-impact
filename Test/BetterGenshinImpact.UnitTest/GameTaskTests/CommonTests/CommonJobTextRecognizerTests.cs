using System.Globalization;
using BetterGenshinImpact.GameTask.Common.GameText;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.CommonTests;

public class CommonJobTextRecognizerTests
{
    [Theory]
    [InlineData("pt-BR", "REVIVER!")]
    [InlineData("zh-Hans", "复苏角色")]
    public void IsRevive_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.Common.Revive, culture == "pt-BR" ? "Reviver" : "复苏");

        Assert.True(recognizer.IsRevive(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Sintetizar")]
    [InlineData("en", "Craft")]
    public void IsCrafting_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.Common.Crafting, recognizedText);

        Assert.True(recognizer.IsCrafting(recognizedText));
    }

    [Theory]
    [InlineData("Synthèse")]
    [InlineData("Synthése")]
    [InlineData("Synthètiser")]
    [InlineData("Synthétiser")]
    public void CraftingSearchText_MatchesEveryLegacyFrenchRegexExpansion(string recognizedText)
    {
        var recognizer = CreateFromEmbeddedCatalog("fr");
        var searchPattern = recognizer.CraftingSearchText;

        Assert.Matches(searchPattern, recognizedText);
        Assert.True(recognizer.IsCrafting(recognizedText));
    }

    [Fact]
    public void CraftingSearchText_EscapesRegexSyntax()
    {
        var matcher = GameTextTestFactory.Create(
            "fr",
            (GameTextKeys.Common.Crafting, "Synth.*"));
        var recognizer = new CommonJobTextRecognizer(matcher);
        var searchPattern = recognizer.CraftingSearchText;

        Assert.Matches(searchPattern, "Synth.*");
        Assert.DoesNotMatch(searchPattern, "SynthABC");
    }

    [Theory]
    [InlineData("pt-BR", "Katheryne")]
    [InlineData("zh-Hans", "凯瑟琳")]
    public void IsKatheryne_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.AdventurersGuild.Katheryne, recognizedText);

        Assert.True(recognizer.IsKatheryne(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Missões Diárias")]
    [InlineData("en", "Daily Commissions")]
    public void IsDailyCommissions_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.AdventurersGuild.DailyCommissions, recognizedText);

        Assert.True(recognizer.IsDailyCommissions(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Expedição")]
    [InlineData("zh-Hans", "探索派遣")]
    public void IsExpedition_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.Expedition.Entry, recognizedText);

        Assert.True(recognizer.IsExpedition(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Resgatar", "Tudo")]
    [InlineData("zh-Hans", "领取", "一键")]
    public void IsClaimAll_MatchesCombinedPortugueseAndLegacyWords(
        string culture,
        string claimText,
        string allText)
    {
        var recognizer = CreateFromEmbeddedCatalog(culture);

        Assert.True(recognizer.IsClaim(claimText));
        Assert.True(recognizer.IsAll(allText));
        Assert.True(recognizer.IsClaimAll([claimText, allText]));
        Assert.False(recognizer.IsClaimAll([claimText]));
        Assert.False(recognizer.IsClaimAll([allText]));
    }

    [Theory]
    [InlineData("A recompensa de hoje já foi coletada")]
    [InlineData("Você já coletou a recompensa de hoje. Volte amanhã para mais missões!")]
    [InlineData("A recompensa de hoje já foi resgatada")]
    public void IsDailyRewardClaimed_MatchesRealPortugueseForms(string recognizedText)
    {
        var recognizer = CreateFromEmbeddedCatalog("pt-BR");

        Assert.True(recognizer.IsDailyRewardClaimed(recognizedText));
    }

    [Fact]
    public void IsDailyRewardClaimed_MatchesLegacyEnglishGameText()
    {
        var recognizer = Create("en", GameTextKeys.AdventureHandbook.DailyRewardClaimed, "claimed");

        Assert.True(recognizer.IsDailyRewardClaimed("Today's reward claimed"));
    }

    [Theory]
    [InlineData("pt-BR", "Tubby — Espírito do Bule")]
    [InlineData("zh-Hans", "阿圆 <壶灵>")]
    public void IsTeapotSpirit_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var alias = culture == "pt-BR" ? "Espírito do Bule" : "壶灵";
        var recognizer = Create(culture, GameTextKeys.SereniteaPot.Spirit, alias);

        Assert.True(recognizer.IsTeapotSpirit(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Nível de confiança")]
    [InlineData("zh-Hans", "信任等阶")]
    public void IsTrustRank_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.SereniteaPot.TrustRank, recognizedText);

        Assert.True(recognizer.IsTrustRank(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Tesouro do Paraíso Mágico")]
    [InlineData("en", "Realm Depot")]
    public void IsRealmDepot_MatchesPortugueseAndLegacyGameText(string culture, string recognizedText)
    {
        var recognizer = Create(culture, GameTextKeys.SereniteaPot.RealmDepot, recognizedText);

        Assert.True(recognizer.IsRealmDepot(recognizedText));
    }

    [Fact]
    public void IsRevive_DoesNotMatchOriginalResin()
    {
        var recognizer = Create("pt-BR", GameTextKeys.Common.Revive, "Reviver");

        Assert.False(recognizer.IsRevive("Resina Original"));
    }

    private static CommonJobTextRecognizer Create(string culture, string key, string alias) =>
        new(GameTextTestFactory.Create(culture, (key, alias)));

    private static CommonJobTextRecognizer CreateFromEmbeddedCatalog(string culture) =>
        new(new GameTextMatcher(
            new FixedGameCultureProvider(CultureInfo.GetCultureInfo(culture)),
            new EmbeddedGameTextCatalogProvider()));

    private sealed class FixedGameCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
