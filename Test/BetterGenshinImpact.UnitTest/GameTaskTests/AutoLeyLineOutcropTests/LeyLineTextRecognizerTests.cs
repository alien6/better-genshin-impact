using System.Globalization;
using BetterGenshinImpact.GameTask.AutoLeyLineOutcrop;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoLeyLineOutcropTests;

public class LeyLineTextRecognizerTests
{
    [Theory]
    [MemberData(nameof(LocalizedDecisionCases))]
    public void LeyLineDecision_MatchesConfiguredGameCulture(
        string culture,
        string decision,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(Matches(sut, decision, recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "resin-original", "Original Resin")]
    [InlineData("en", "blossom-wealth", "Flor da Riqueza")]
    [InlineData("zh-Hans", "touch", "Touch the Ley Line Outcrop")]
    [InlineData("pt-BR", "stop", "停止")]
    public void LeyLineDecision_DoesNotFallbackAcrossLanguages(
        string culture,
        string decision,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.False(Matches(sut, decision, recognizedText));
    }

    [Fact]
    public void RewardBlossomPrompt_MatchesSemanticFragmentsSplitAcrossOcrRegions()
    {
        var sut = Create("pt-BR");

        Assert.True(sut.IsRewardBlossomPrompt(["Selecione a forma", "de ativação", "da Flor da Linha Ley"]));
    }

    [Fact]
    public void AllowedResinOption_UsesExistingResinPriorityVocabulary()
    {
        var sut = Create("en");

        Assert.True(sut.IsAllowedResinOption("Original Resin"));
        Assert.True(sut.IsAllowedResinOption("Condensed Resin"));
        Assert.True(sut.IsAllowedResinOption("Transient Resin"));
        Assert.True(sut.IsAllowedResinOption("Fragile Resin"));
        Assert.False(sut.IsAllowedResinOption("Primogems"));
    }

    [Fact]
    public void AliasSnapshots_AreResolvedOnlyAtConstruction()
    {
        var matcher = new CountingMatcher();
        var sut = new LeyLineTextRecognizer(matcher);
        var callsAfterConstruction = matcher.GetAliasesCalls;

        _ = sut.IsTouch("Touch");
        _ = sut.IsRewardBlossomPrompt(["Activate", "Ley Line"]);
        _ = sut.IsAllowedResinOption("Original Resin");

        Assert.Equal(callsAfterConstruction, matcher.GetAliasesCalls);
    }

    public static IEnumerable<object[]> LocalizedDecisionCases()
    {
        yield return ["zh-Hans", "resin-original", "原粹树脂"];
        yield return ["zh-Hant", "resin-condensed", "濃縮樹脂"];
        yield return ["en", "resin-transient", "Transient Resin"];
        yield return ["ja", "resin-fragile", "脆弱樹脂"];
        yield return ["fr", "replenish", "Recharger la Résine originelle"];
        yield return ["pt-BR", "double-reward", "Recompensas em Dobro"];
        yield return ["en", "double-reward-2x", "2x Rewards"];
        yield return ["zh-Hans", "touch", "接触地脉之花"];
        yield return ["zh-Hant", "activate", "啟動地脈之花"];
        yield return ["en", "select", "Select Activation Method"];
        yield return ["ja", "ley-line", "地脈"];
        yield return ["fr", "outcrop", "émergence"];
        yield return ["pt-BR", "blossom-wealth", "Afloramento da Linha Ley: Flor da Riqueza"];
        yield return ["en", "blossom-revelation", "Ley Line Outcrop: Blossom of Revelation"];
        yield return ["zh-Hans", "revive", "复苏"];
        yield return ["zh-Hant", "use", "使用"];
        yield return ["ja", "stop", "停止する"];
        yield return ["fr", "original-40", "40 Résine originelle"];
    }

    private static bool Matches(LeyLineTextRecognizer sut, string decision, string recognizedText) => decision switch
    {
        "resin-original" => sut.IsOriginalResin(recognizedText),
        "resin-condensed" => sut.IsCondensedResin(recognizedText),
        "resin-transient" => sut.IsTransientResin(recognizedText),
        "resin-fragile" => sut.IsFragileResin(recognizedText),
        "replenish" => sut.IsReplenish(recognizedText),
        "double-reward" => sut.IsDoubleReward(recognizedText),
        "double-reward-2x" => sut.IsDoubleReward2x(recognizedText),
        "touch" => sut.IsTouch(recognizedText),
        "activate" => sut.IsActivate(recognizedText),
        "select" => sut.IsSelect(recognizedText),
        "ley-line" => sut.IsLeyLine(recognizedText),
        "outcrop" => sut.IsOutcrop(recognizedText),
        "blossom-wealth" => sut.IsBlossomOfWealth(recognizedText),
        "blossom-revelation" => sut.IsBlossomOfRevelation(recognizedText),
        "revive" => sut.IsRevive(recognizedText),
        "use" => sut.IsUse(recognizedText),
        "stop" => sut.IsStop(recognizedText),
        "original-40" => sut.IsOriginalResin40Prompt(recognizedText),
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null)
    };

    private static LeyLineTextRecognizer Create(string culture) => new(CreateMatcher(culture));

    private static GameTextMatcher CreateMatcher(string culture) =>
        new(new FixedGameCultureProvider(CultureInfo.GetCultureInfo(culture)), new EmbeddedGameTextCatalogProvider());

    private sealed class CountingMatcher : IGameTextMatcher
    {
        public int GetAliasesCalls { get; private set; }

        public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null)
        {
            GetAliasesCalls++;
            return [key];
        }

        public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null) => throw new NotSupportedException();
        public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) => throw new NotSupportedException();
        public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) => throw new NotSupportedException();
    }

    private sealed class FixedGameCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
