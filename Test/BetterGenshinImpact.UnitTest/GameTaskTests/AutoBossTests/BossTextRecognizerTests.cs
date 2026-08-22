using System.Globalization;
using BetterGenshinImpact.GameTask.AutoBoss;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.Helpers;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoBossTests;

public class BossTextRecognizerTests
{
    [Theory]
    [MemberData(nameof(LocalizedDecisionCases))]
    public void BossDecision_MatchesConfiguredGameCulture(string culture, string decision, string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(Matches(sut, decision, recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Resina Original totalmente recuperada", 0, 0, 0)]
    [InlineData("pt-BR", "Recupera\u00e7\u00e3o total\n12:34:56", 12, 34, 56)]
    [InlineData("en", "Full Recovery\n12:34:56", 12, 34, 56)]
    [InlineData("zh-Hans", "全部恢\n复 12:34:56", 12, 34, 56)]
    public void RecoveryState_GatesTimeParsingWithLocalizedWording(string culture, string recognizedText, int hours, int minutes, int seconds)
    {
        var sut = Create(culture);

        Assert.Equal(new TimeSpan(hours, minutes, seconds), sut.TryGetFullRecoveryTime(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "12:34:56")]
    [InlineData("en", "A meeting begins at 12:34:56")]
    [InlineData("zh-Hans", "12:34:56")]
    public void RecoveryState_RejectsUnrelatedTimeWithoutRecoveryWording(string culture, string recognizedText)
    {
        var sut = Create(culture);

        Assert.Null(sut.TryGetFullRecoveryTime(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "use", "Use")]
    [InlineData("en", "use", "Usar")]
    [InlineData("pt-BR", "original", "Original Resin")]
    [InlineData("en", "original", "Resina Original")]
    public void BossDecision_DoesNotFallbackAcrossLanguages(string culture, string decision, string recognizedText)
    {
        var sut = Create(culture);

        Assert.False(decision == "use" ? sut.IsUse(recognizedText) : sut.IsOriginalResin(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Repor Resina")]
    [InlineData("pt-BR", "Repor Original")]
    [InlineData("pt-BR", "Resina Original")]
    [InlineData("en", "Replenish Original")]
    [InlineData("en", "Replenish Resin")]
    [InlineData("en", "Original Resin")]
    public void SupplementPrompt_RejectsTextMissingAnySemanticFragment(string culture, string recognizedText)
    {
        var sut = Create(culture);

        Assert.False(sut.IsSupplementPrompt(recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "Usar", "Resina Original")]
    [InlineData("en", "Use", "Original Resin")]
    [InlineData("zh-Hans", "使用", "原粹树脂")]
    [InlineData("zh-Hant", "使用", "原粹樹脂")]
    [InlineData("ja", "使用", "天然樹脂")]
    [InlineData("fr", "Utiliser", "Résine originelle")]
    public void RewardUsePrompt_MatchesSemanticFragmentsSplitAcrossOcrRegions(
        string culture,
        string firstRegion,
        string secondRegion)
    {
        var sut = Create(culture);

        Assert.True(sut.IsRewardUseOriginalResinPrompt([firstRegion, secondRegion]));
    }

    [Theory]
    [InlineData("pt-BR", "Repor", "Resina", "Original")]
    [InlineData("en", "Replenish", "Original", "Resin")]
    [InlineData("zh-Hans", "补充", "原粹", "树脂")]
    [InlineData("zh-Hant", "補充", "原粹", "樹脂")]
    [InlineData("ja", "補充", "天然", "樹脂")]
    [InlineData("fr", "Recharger", "originelle", "résine")]
    public void SupplementPrompt_MatchesAllSemanticFragmentsAcrossOcrRegions(
        string culture,
        string firstRegion,
        string secondRegion,
        string thirdRegion)
    {
        var sut = Create(culture);

        Assert.True(sut.IsSupplementPrompt([firstRegion, secondRegion, thirdRegion]));
    }

    [Fact]
    public void SupplementPrompt_RejectsCrossLanguageRegionFragments()
    {
        var sut = Create("pt-BR");

        Assert.False(sut.IsSupplementPrompt(["Repor", "Original", "Resin"]));
    }

    [Fact]
    public void AliasSnapshots_AreResolvedOnlyAtConstruction()
    {
        var matcher = new CountingMatcher();
        var sut = new BossTextRecognizer(matcher);
        var callsAfterConstruction = matcher.GetAliasesCalls;

        _ = sut.IsUse("Use");
        _ = sut.IsQuickUse("Quick Use");
        _ = sut.IsSupplementPrompt("Replenish Original Resin");

        Assert.Equal(callsAfterConstruction, matcher.GetAliasesCalls);
    }

    [Fact]
    public void UseQuantity_ParsesNumbersOnlyAfterLocalizedLabelMatches()
    {
        var sut = Create("pt-BR");

        Assert.Equal(7, sut.TryGetUseQuantity("Quantidade a usar: 7"));
        Assert.Null(sut.TryGetUseQuantity("7"));
    }

    [Fact]
    public void UseQuantity_PrefersNumberFollowingLocalizedLabel()
    {
        var sut = Create("pt-BR");

        Assert.Equal(7, sut.TryGetUseQuantity("Disponível: 99 Quantidade a usar: 7"));
    }

    [Fact]
    public void RecoveryTime_PrefersTimeFollowingLocalizedRecoveryWording()
    {
        var sut = Create("pt-BR");

        Assert.Equal(
            new TimeSpan(12, 34, 56),
            sut.TryGetFullRecoveryTime("Atualização: 01:02:03 Recuperação total: 12:34:56"));
    }

    [Theory]
    [InlineData(500, 300, 2)]
    [InlineData(1500, 1000, 2)]
    [InlineData(3000, 1000, 3)]
    public void OcrPollingAttemptCount_CoversWholeTimeoutWindow(int timeout, int retryInterval, int expected)
    {
        Assert.Equal(expected, AutoBossTask.CalculateOcrPollingAttempts(timeout, retryInterval));
    }

    public static IEnumerable<object[]> LocalizedDecisionCases()
    {
        yield return ["pt-BR", "full", "Resina Original totalmente recuperada"];
        yield return ["zh-Hans", "full", "原粹树脂已完全恢复"];
        yield return ["en", "full", "Original Resin fully recovered"];

        yield return ["pt-BR", "replenish", "Repor Resina Original"];
        yield return ["zh-Hans", "replenish", "补充原粹树脂"];
        yield return ["en", "replenish", "Replenish Original Resin"];

        yield return ["pt-BR", "transient", "Resina Transiente"];
        yield return ["en", "transient", "Transient Resin"];
        yield return ["pt-BR", "fragile", "Resina Frágil"];
        yield return ["zh-Hans", "fragile", "脆弱树脂"];
        yield return ["pt-BR", "original", "Resina Original"];
        yield return ["en", "use", "Use"];
        yield return ["pt-BR", "use", "Usar"];
        yield return ["pt-BR", "quick-use", "Uso Rápido"];
        yield return ["pt-BR", "use-quantity", "Quantidade a usar: 3"];
        yield return ["pt-BR", "obtain", "Obtido"];
        yield return ["en", "obtain", "Obtained"];
        yield return ["pt-BR", "touch", "Toque na Flor da Linha Ley"];
        yield return ["zh-Hans", "touch", "接触征讨之花"];
        yield return ["pt-BR", "reward-use", "Usar Resina Original"];
        yield return ["en", "reward-use", "Use Original Resin"];
        yield return ["pt-BR", "continue", "Clique em uma área em branco para continuar"];
        yield return ["en", "continue", "Click blank area to continue"];
        yield return ["pt-BR", "supplement", "Repor Resina Original"];
        yield return ["en", "insufficient", "You don't have enough Original Resin"];
        yield return ["pt-BR", "insufficient", "Você não possui Resina Original suficientes"];
    }

    private static bool Matches(BossTextRecognizer sut, string decision, string recognizedText) => decision switch
    {
        "full" => sut.IsFullResinRecovered(recognizedText),
        "replenish" => sut.IsReplenishOriginalResin(recognizedText),
        "transient" => sut.IsTransientResin(recognizedText),
        "fragile" => sut.IsFragileResin(recognizedText),
        "original" => sut.IsOriginalResin(recognizedText),
        "use" => sut.IsUse(recognizedText),
        "quick-use" => sut.IsQuickUse(recognizedText),
        "use-quantity" => sut.TryGetUseQuantity(recognizedText) != null,
        "obtain" => sut.IsObtain(recognizedText),
        "touch" => sut.IsTouchTrounceBlossom(recognizedText),
        "reward-use" => sut.IsRewardUseOriginalResinPrompt(recognizedText),
        "continue" => sut.IsClickBlankAreaToContinue(recognizedText),
        "supplement" => sut.IsSupplementPrompt(recognizedText),
        "insufficient" => sut.IsResinInsufficient(recognizedText),
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null)
    };

    private static BossTextRecognizer Create(string culture) => new(CreateMatcher(culture));

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
