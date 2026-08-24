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

        Assert.True(sut.IsRewardBlossomPrompt(Normalize(sut, ["Selecione a forma", "de ativação", "da Flor da Linha Ley"])));
    }

    [Fact]
    public void RewardBlossomPrompt_MatchesStandaloneActivationSelectionTitle()
    {
        var sut = Create("en");

        Assert.True(sut.IsRewardBlossomTitle(Normalize(sut, ["Select Activation Method"])));
    }

    [Fact]
    public void RewardBlossomPrompt_RejectsContentWithOnlySelectionText()
    {
        var sut = Create("en");

        Assert.False(sut.IsRewardBlossomPrompt(Normalize(sut, ["Select"])));
    }

    [Fact]
    public void AllowedResinOption_UsesExistingResinPriorityVocabulary()
    {
        var sut = Create("en");

        Assert.True(sut.IsAllowedResinOption(sut.NormalizeOcrText("Original Resin")));
        Assert.True(sut.IsAllowedResinOption(sut.NormalizeOcrText("Condensed Resin")));
        Assert.True(sut.IsAllowedResinOption(sut.NormalizeOcrText("Transient Resin")));
        Assert.True(sut.IsAllowedResinOption(sut.NormalizeOcrText("Fragile Resin")));
        Assert.False(sut.IsAllowedResinOption(sut.NormalizeOcrText("Primogems")));
    }

    [Fact]
    public void AliasSnapshots_AreResolvedOnlyAtConstruction()
    {
        var matcher = new CountingMatcher();
        var sut = new LeyLineTextRecognizer(matcher);
        var callsAfterConstruction = matcher.GetAliasesCalls;

        _ = sut.IsTouch(sut.NormalizeOcrText("Touch"));
        _ = sut.IsRewardBlossomPrompt(Normalize(sut, ["Activate", "Ley Line"]));
        _ = sut.IsAllowedResinOption(sut.NormalizeOcrText("Original Resin"));

        Assert.Equal(callsAfterConstruction, matcher.GetAliasesCalls);
    }

    [Fact]
    public void FightOutcomeAndObjective_RecognizePortugueseOcrAtTheBoundary()
    {
        var sut = Create("pt-BR");

        Assert.True(sut.IsFightSuccess("  DESAFIO CONCLUÍDO!  "));
        Assert.True(sut.IsFightFailure("Desafio Fracassado"));
        Assert.True(sut.IsFightObjective("Derrote todos os inimigos"));
        Assert.False(sut.IsFightSuccess("Desafio Fracassado"));
    }

    [Fact]
    public void PublicPredicates_NormalizeRawOcrWithoutRequiringCallersToPreNormalize()
    {
        var sut = Create("pt-BR");

        Assert.True(sut.IsOriginalResin("  RESINA-ORIGINAL! "));
        Assert.True(sut.IsBlossomOfWealth("Afloramento da Linha Ley: Flor da Riqueza"));
        Assert.True(sut.IsReplenish("REPOR resina original"));
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
        "resin-original" => sut.IsOriginalResin(sut.NormalizeOcrText(recognizedText)),
        "resin-condensed" => sut.IsCondensedResin(sut.NormalizeOcrText(recognizedText)),
        "resin-transient" => sut.IsTransientResin(sut.NormalizeOcrText(recognizedText)),
        "resin-fragile" => sut.IsFragileResin(sut.NormalizeOcrText(recognizedText)),
        "replenish" => sut.IsReplenish(sut.NormalizeOcrText(recognizedText)),
        "double-reward" => sut.IsDoubleReward(sut.NormalizeOcrText(recognizedText)),
        "double-reward-2x" => sut.IsDoubleReward2x(sut.NormalizeOcrText(recognizedText)),
        "touch" => sut.IsTouch(sut.NormalizeOcrText(recognizedText)),
        "activate" => sut.IsActivate(sut.NormalizeOcrText(recognizedText)),
        "select" => sut.IsSelect(sut.NormalizeOcrText(recognizedText)),
        "ley-line" => sut.IsLeyLine(sut.NormalizeOcrText(recognizedText)),
        "outcrop" => sut.IsOutcrop(sut.NormalizeOcrText(recognizedText)),
        "blossom-wealth" => sut.IsBlossomOfWealth(sut.NormalizeOcrText(recognizedText)),
        "blossom-revelation" => sut.IsBlossomOfRevelation(sut.NormalizeOcrText(recognizedText)),
        "revive" => sut.IsRevive(sut.NormalizeOcrText(recognizedText)),
        "use" => sut.IsUse(sut.NormalizeOcrText(recognizedText)),
        "stop" => sut.IsStop(sut.NormalizeOcrText(recognizedText)),
        "original-40" => sut.IsOriginalResin40Prompt(sut.NormalizeOcrText(recognizedText)),
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null)
    };

    private static string[] Normalize(LeyLineTextRecognizer sut, IEnumerable<string> texts) =>
        texts.Select(sut.NormalizeOcrText).ToArray();

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
