using System.Globalization;
using BetterGenshinImpact.GameTask.AutoStygianOnslaught;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoStygianOnslaughtTests;

public class StygianTextRecognizerTests
{
    [Theory]
    [MemberData(nameof(LocalizedDecisionCases))]
    public void Decision_MatchesConfiguredGameCulture(string culture, string decision, string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(Matches(sut, decision, recognizedText));
    }

    [Fact]
    public void PortugueseChallengeFailure_DoesNotDependOnFormerChineseLiteral()
    {
        var sut = Create("pt-BR");

        Assert.True(sut.IsChallengeFailed(sut.NormalizeOcrText("Desafio Fracassado")));
        Assert.False(sut.IsChallengeFailed(sut.NormalizeOcrText("挑战失败")));
    }

    [Fact]
    public void AliasSnapshots_AreResolvedOnlyAtConstruction()
    {
        var matcher = new CountingMatcher();
        var sut = new StygianTextRecognizer(matcher);
        var callsAfterConstruction = matcher.GetAliasesCalls;

        _ = sut.IsReturn(sut.NormalizeOcrText("Return"));
        _ = sut.IsRewardPhaseEnded(sut.NormalizeOcrTexts(["Reward phase", "ended"]));
        _ = sut.IsResinSelect(sut.NormalizeOcrTexts(["Original Resin", "Ley Line Blossom"]));

        Assert.Equal(callsAfterConstruction, matcher.GetAliasesCalls);
    }

    public static IEnumerable<object[]> LocalizedDecisionCases()
    {
        yield return ["pt-BR", "return", "Voltar"];
        yield return ["pt-BR", "challenge-failed", "Desafio Fracassado"];
        yield return ["en", "retry", "Retry Challenge"];
        yield return ["zh-Hans", "blossom", "地脉之花"];
        yield return ["zh-Hant", "resin", "地脈之花濃縮樹脂"];
        yield return ["fr", "preview", "Aperçu de personnage"];
        yield return ["ja", "start", "挑戦開始"];
        yield return ["pt-BR", "solo", "Desafio Solo"];
        yield return ["en", "event-name", "Stygian Onslaught"];
        yield return ["pt-BR", "event-overview", "Visão geral do evento"];
        yield return ["en", "reward-phase-ended", "Turbulent Outbreak Phase Ended"];
        yield return ["pt-BR", "resin-insufficient", "Você não possui Resina Original suficientes"];
        yield return ["pt-BR", "supplement-resin", "Repor Resina Original"];
        yield return ["pt-BR", "activate", "Ativar"];
    }

    private static bool Matches(StygianTextRecognizer sut, string decision, string recognizedText) => decision switch
    {
        "return" => sut.IsReturn(sut.NormalizeOcrText(recognizedText)),
        "challenge-failed" => sut.IsChallengeFailed(sut.NormalizeOcrText(recognizedText)),
        "retry" => sut.IsRetryChallenge(sut.NormalizeOcrText(recognizedText)),
        "blossom" => sut.IsLeyLineBlossom(sut.NormalizeOcrText(recognizedText)),
        "resin" => sut.IsResinSelect(sut.NormalizeOcrText(recognizedText)),
        "preview" => sut.IsCharacterPreview(sut.NormalizeOcrText(recognizedText)),
        "start" => sut.IsStartChallenge(sut.NormalizeOcrText(recognizedText)),
        "solo" => sut.IsSoloChallenge(sut.NormalizeOcrText(recognizedText)),
        "event-name" => sut.IsEventName(sut.NormalizeOcrText(recognizedText)),
        "event-overview" => sut.IsEventOverview(sut.NormalizeOcrText(recognizedText)),
        "reward-phase-ended" => sut.IsRewardPhaseEnded(sut.NormalizeOcrText(recognizedText)),
        "resin-insufficient" => sut.IsResinInsufficient(sut.NormalizeOcrText(recognizedText)),
        "supplement-resin" => sut.IsSupplementResin(sut.NormalizeOcrText(recognizedText)),
        "activate" => sut.IsActivate(sut.NormalizeOcrText(recognizedText)),
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null)
    };

    private static StygianTextRecognizer Create(string culture) => new(CreateMatcher(culture));

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
