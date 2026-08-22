using System.Globalization;
using BetterGenshinImpact.GameTask.AutoDomain;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoDomainTests;

public class DomainTextRecognizerTests
{
    [Theory]
    [MemberData(nameof(LocalizedDecisionCases))]
    public void DomainDecision_MatchesConfiguredGameCulture(
        string culture,
        string decision,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(Matches(sut, decision, recognizedText));
    }

    [Theory]
    [InlineData("pt-BR", "challenge-completed", "Challenge Completed")]
    [InlineData("en", "petrified-tree", "Árvore Petrificada")]
    [InlineData("zh-Hans", "resin-use-prompt", "Ainda deseja desafiar este Domínio?")]
    [InlineData("pt-BR", "cancel", "取消")]
    [InlineData("pt-BR", "solo-challenge", "Solo Challenge")]
    [InlineData("en", "start-challenge", "Iniciar Desafio")]
    public void DomainDecision_DoesNotFallbackAcrossLanguages(
        string culture,
        string decision,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.False(Matches(sut, decision, recognizedText));
    }

    [Fact]
    public void ChallengeAliasSnapshots_AreResolvedOnceForConfiguredCulture()
    {
        var sut = Create("pt-BR");

        Assert.Equal(["Desafio Solo"], sut.SoloChallengeAliases);
        Assert.Equal(["Iniciar Desafio"], sut.StartChallengeAliases);
        Assert.True(((ICollection<string>)sut.SoloChallengeAliases).IsReadOnly);
        Assert.True(((ICollection<string>)sut.StartChallengeAliases).IsReadOnly);
    }

    [Theory]
    [MemberData(nameof(LegacyDomainResourceAliases))]
    public void Catalog_PreservesEveryLegacyDomainResourceValueOrRegexExpansion(
        string culture,
        string key,
        string legacyAlias)
    {
        var matcher = CreateMatcher(culture);

        Assert.Contains(legacyAlias, matcher.GetAliases(key));
    }

    public static IEnumerable<object[]> LocalizedDecisionCases()
    {
        yield return ["pt-BR", "challenge-completed", "DESAFIO CONCLUÍDO!"];
        yield return ["zh-Hans", "challenge-completed", "挑战完成"];
        yield return ["en", "challenge-completed", "Challenge Completed"];

        yield return ["pt-BR", "auto-leaving", "Saindo em 3s"];
        yield return ["zh-Hans", "auto-leaving", "3秒后自动退出"];
        yield return ["en", "auto-leaving", "Exiting in 3s"];

        yield return ["pt-BR", "skip", "Pular"];
        yield return ["zh-Hans", "skip", "跳过"];
        yield return ["en", "skip", "Skip"];

        yield return ["pt-BR", "ley-line-disorder", "Anomalia da Linha Ley"];
        yield return ["zh-Hans", "ley-line-disorder", "地脉异常"];
        yield return ["en", "ley-line-disorder", "Ley Line Disorder"];

        yield return ["pt-BR", "click-anywhere", "Clique em qualquer lugar para fechar"];
        yield return ["zh-Hans", "click-anywhere", "点击任意位置关闭"];
        yield return ["en", "click-anywhere", "Click anywhere to close"];

        yield return ["pt-BR", "quick-select", "Escolha rápida"];
        yield return ["zh-Hans", "quick-select", "快速选择"];
        yield return ["en", "quick-select", "Quick Select"];

        yield return ["pt-BR", "two-star-artifact", "Artefatos de 2 estrelas"];
        yield return ["zh-Hans", "two-star-artifact", "2星圣遗物"];
        yield return ["en", "two-star-artifact", "2-Star Artifacts"];

        yield return ["pt-BR", "limited-time-fully-open", "Todos disponíveis por tempo limitado"];
        yield return ["zh-Hans", "limited-time-fully-open", "限时全部开放"];
        yield return ["en", "limited-time-fully-open", "All available for a limited time"];

        yield return ["pt-BR", "solo-challenge", "Desafio Solo"];
        yield return ["zh-Hans", "solo-challenge", "单人挑战"];
        yield return ["en", "solo-challenge", "Solo Challenge"];

        yield return ["pt-BR", "start-challenge", "Iniciar Desafio"];
        yield return ["zh-Hans", "start-challenge", "开始挑战"];
        yield return ["en", "start-challenge", "Start Challenge"];

        yield return ["pt-BR", "petrified-tree", "Árvore Petrificada"];
        yield return ["zh-Hans", "petrified-tree", "石化古树"];
        yield return ["en", "petrified-tree", "Petrified Tree"];

        yield return ["pt-BR", "resin-insufficient", "Resina Original insuficiente"];
        yield return ["zh-Hans", "resin-insufficient", "数量不足"];
        yield return ["en", "resin-insufficient", "Insufficient Original Resin"];

        yield return ["pt-BR", "resin-replenish", "Repor Resina Original"];
        yield return ["zh-Hans", "resin-replenish", "补充原粹树脂"];
        yield return ["en", "resin-replenish", "Replenish Original Resin"];

        yield return ["pt-BR", "resin-use-prompt", "Ainda deseja desafiar este Domínio?"];
        yield return ["zh-Hans", "resin-use-prompt", "是否仍要挑战该秘境"];
        yield return ["en", "resin-use-prompt", "Do you still wish to challenge this Domain?"];

        yield return ["pt-BR", "original-resin", "Resina Original"];
        yield return ["zh-Hans", "original-resin", "原粹树脂"];
        yield return ["en", "original-resin", "Original Resin"];

        yield return ["pt-BR", "condensed-resin", "Resina Condensada"];
        yield return ["zh-Hans", "condensed-resin", "浓缩树脂"];
        yield return ["en", "condensed-resin", "Condensed Resin"];

        yield return ["pt-BR", "fragile-resin", "Resina Frágil"];
        yield return ["zh-Hans", "fragile-resin", "脆弱树脂"];
        yield return ["en", "fragile-resin", "Fragile Resin"];

        yield return ["pt-BR", "transient-resin", "Resina Transiente"];
        yield return ["zh-Hans", "transient-resin", "须臾树脂"];
        yield return ["en", "transient-resin", "Transient Resin"];

        yield return ["pt-BR", "use", "Usar"];
        yield return ["zh-Hans", "use", "使用"];
        yield return ["en", "use", "Use"];

        yield return ["pt-BR", "cancel", "Cancelar"];
        yield return ["zh-Hans", "cancel", "取消"];
        yield return ["en", "cancel", "Cancel"];
    }

    public static IEnumerable<object[]> LegacyDomainResourceAliases()
    {
        // Simplified Chinese resource values and finite regex expansions.
        yield return ["zh-Hans", GameTextKeys.Domain.LeyLineDisorder, "地脉异常"];
        yield return ["zh-Hans", GameTextKeys.Domain.ChallengeCompleted, "(挑战|达成)"];
        yield return ["zh-Hans", GameTextKeys.Domain.ChallengeCompleted, "挑战"];
        yield return ["zh-Hans", GameTextKeys.Domain.ChallengeCompleted, "达成"];
        yield return ["zh-Hans", GameTextKeys.Common.ClickAnywhereToClose, "点击任意位置关闭"];
        yield return ["zh-Hans", GameTextKeys.Domain.AutoLeaving, "(自动|退出)"];
        yield return ["zh-Hans", GameTextKeys.Domain.AutoLeaving, "自动"];
        yield return ["zh-Hans", GameTextKeys.Domain.AutoLeaving, "退出"];
        yield return ["zh-Hans", GameTextKeys.Domain.Skip, "跳过"];

        // Traditional Chinese resource values, including every finite character-class expansion.
        yield return ["zh-Hant", GameTextKeys.Domain.LeyLineDisorder, "地[脈服][異翼昊]常"];
        foreach (var alias in new[] { "地脈異常", "地脈翼常", "地脈昊常", "地服異常", "地服翼常", "地服昊常" })
        {
            yield return ["zh-Hant", GameTextKeys.Domain.LeyLineDisorder, alias];
        }
        yield return ["zh-Hant", GameTextKeys.Domain.ChallengeCompleted, "(挑戰|達成)"];
        yield return ["zh-Hant", GameTextKeys.Domain.ChallengeCompleted, "挑戰"];
        yield return ["zh-Hant", GameTextKeys.Domain.ChallengeCompleted, "達成"];
        yield return ["zh-Hant", GameTextKeys.Common.ClickAnywhereToClose, "點擊任意位置關閉"];
        yield return ["zh-Hant", GameTextKeys.Domain.AutoLeaving, "(自動|退出)"];
        yield return ["zh-Hant", GameTextKeys.Domain.AutoLeaving, "自動"];
        yield return ["zh-Hant", GameTextKeys.Domain.AutoLeaving, "退出"];
        yield return ["zh-Hant", GameTextKeys.Domain.Skip, "跳過"];

        // English resource values plus literal compatibility expansions for dot-star regexes.
        yield return ["en", GameTextKeys.Domain.LeyLineDisorder, "Ley.*Line.*Disorder"];
        yield return ["en", GameTextKeys.Domain.LeyLineDisorder, "Ley Line Disorder"];
        yield return ["en", GameTextKeys.Domain.ChallengeCompleted, "(Challenge|Completed)"];
        yield return ["en", GameTextKeys.Domain.ChallengeCompleted, "Challenge"];
        yield return ["en", GameTextKeys.Domain.ChallengeCompleted, "Completed"];
        yield return ["en", GameTextKeys.Common.ClickAnywhereToClose, "Click.*any.*where.*to.*close"];
        yield return ["en", GameTextKeys.Common.ClickAnywhereToClose, "Click anywhere to close"];
        yield return ["en", GameTextKeys.Domain.AutoLeaving, "Leaving"];
        yield return ["en", GameTextKeys.Domain.Skip, "Skip"];

        // French resource values plus literal compatibility expansions for dot-star regexes.
        yield return ["fr", GameTextKeys.Domain.LeyLineDisorder, "Anomalie.*énergétique"];
        yield return ["fr", GameTextKeys.Domain.LeyLineDisorder, "Anomalie énergétique"];
        yield return ["fr", GameTextKeys.Domain.ChallengeCompleted, "(Défi|terminé)"];
        yield return ["fr", GameTextKeys.Domain.ChallengeCompleted, "Défi"];
        yield return ["fr", GameTextKeys.Domain.ChallengeCompleted, "terminé"];
        yield return ["fr", GameTextKeys.Common.ClickAnywhereToClose, "Cliquez.*pour.*fermer"];
        yield return ["fr", GameTextKeys.Common.ClickAnywhereToClose, "Cliquez pour fermer"];
        yield return ["fr", GameTextKeys.Domain.AutoLeaving, "Sortie"];
        yield return ["fr", GameTextKeys.Domain.Skip, "Passer"];
    }

    private static bool Matches(DomainTextRecognizer sut, string decision, string recognizedText) => decision switch
    {
        "challenge-completed" => sut.IsChallengeCompleted(recognizedText),
        "auto-leaving" => sut.IsAutoLeaving(recognizedText),
        "skip" => sut.IsSkip(recognizedText),
        "ley-line-disorder" => sut.IsLeyLineDisorder(recognizedText),
        "click-anywhere" => sut.IsClickAnywhereToClose(recognizedText),
        "quick-select" => sut.IsQuickSelect(recognizedText),
        "two-star-artifact" => sut.IsTwoStarArtifact(recognizedText),
        "limited-time-fully-open" => sut.IsLimitedTimeFullyOpen(recognizedText),
        "solo-challenge" => sut.IsSoloChallenge(recognizedText),
        "start-challenge" => sut.IsStartChallenge(recognizedText),
        "petrified-tree" => sut.IsPetrifiedTree(recognizedText),
        "resin-insufficient" => sut.IsResinInsufficient(recognizedText),
        "resin-replenish" => sut.IsResinReplenishPrompt(recognizedText),
        "resin-use-prompt" => sut.IsResinUsePrompt(recognizedText),
        "original-resin" => sut.IsOriginalResin(recognizedText),
        "condensed-resin" => sut.IsCondensedResin(recognizedText),
        "fragile-resin" => sut.IsFragileResin(recognizedText),
        "transient-resin" => sut.IsTransientResin(recognizedText),
        "use" => sut.IsUse(recognizedText),
        "cancel" => sut.IsCancel(recognizedText),
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null)
    };

    private static DomainTextRecognizer Create(string culture) => new(CreateMatcher(culture));

    private static GameTextMatcher CreateMatcher(string culture) =>
        new(new FixedGameCultureProvider(CultureInfo.GetCultureInfo(culture)), new EmbeddedGameTextCatalogProvider());

    private sealed class FixedGameCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
