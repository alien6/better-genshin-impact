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

        Assert.Equal(["Desafio Solo"], sut.SoloChallengeOcrMatchAliases);
        Assert.Equal(["Iniciar Desafio"], sut.StartChallengeOcrMatchAliases);
        Assert.True(((ICollection<string>)sut.SoloChallengeOcrMatchAliases).IsReadOnly);
        Assert.True(((ICollection<string>)sut.StartChallengeOcrMatchAliases).IsReadOnly);
    }

    [Fact]
    public void ChallengeAliasSnapshots_PreserveEveryAliasWithoutDroppingAny()
    {
        var sut = new DomainTextRecognizer(new ChallengeAliasMatcher());

        Assert.Equal(["Solo Challenge", "Legacy Solo Challenge"], sut.SoloChallengeOcrMatchAliases);
        Assert.Equal(["Start Challenge", "Legacy Start Challenge"], sut.StartChallengeOcrMatchAliases);
        Assert.True(((ICollection<string>)sut.SoloChallengeOcrMatchAliases).IsReadOnly);
        Assert.True(((ICollection<string>)sut.StartChallengeOcrMatchAliases).IsReadOnly);
    }

    [Fact]
    public void NewRecognizerSnapshot_UsesCultureAfterAConfiguredLanguageSwitch()
    {
        var matcher = new SwitchingChallengeAliasMatcher();
        var simplifiedChinese = new DomainTextRecognizer(matcher);

        matcher.UsePortuguese = true;
        var portuguese = new DomainTextRecognizer(matcher);

        Assert.Equal(["单人挑战"], simplifiedChinese.SoloChallengeOcrMatchAliases);
        Assert.Equal(["Desafio Solo"], portuguese.SoloChallengeOcrMatchAliases);
    }

    [Theory]
    [InlineData(GameTextKeys.Domain.SoloChallenge, "DESAFIO, SOLO!")]
    [InlineData(GameTextKeys.Domain.StartChallenge, "INICIAR - DESAFIO")]
    public void ConfirmTextAtProductionBoundary_UsesSemanticMatchingForRawPortugueseOcr(
        string key,
        string recognizedText)
    {
        var sut = Create("pt-BR");

        Assert.True(sut.IsConfirmText(recognizedText, key));
    }

    [Fact]
    public void ResinTextSnapshots_AreReusedWithoutFreezingTheFirstCulture()
    {
        var matcher = new SwitchingResinMatcher();
        var simplifiedChinese = AutoDomainTask.CreateResinTextRecognizer(matcher);
        var callsAfterSimplifiedChineseSnapshot = matcher.GetAliasesCalls;

        Assert.True(AutoDomainTask.IsConfiguredResin(simplifiedChinese, "原粹树脂", "原粹树脂"));
        Assert.Equal(callsAfterSimplifiedChineseSnapshot, matcher.GetAliasesCalls);

        matcher.UsePortuguese = true;
        var portuguese = AutoDomainTask.CreateResinTextRecognizer(matcher);

        Assert.True(AutoDomainTask.IsConfiguredResin(portuguese, "RESINA, FRAGIL!", "脆弱树脂"));
        Assert.True(matcher.GetAliasesCalls > callsAfterSimplifiedChineseSnapshot);
    }

    [Theory]
    [InlineData("pt-BR", "solo", "Desafio Solo")]
    [InlineData("pt-BR", "start", "Iniciar Desafio")]
    [InlineData("en", "solo", "Solo Challenge")]
    [InlineData("en", "start", "Start Challenge")]
    [InlineData("fr", "solo", "Défi solo")]
    [InlineData("fr", "start", "Défi lancé")]
    public void ChallengeText_MatchesAtTheSemanticProductionBoundary(
        string culture,
        string decision,
        string recognizedText)
    {
        var sut = Create(culture);
        var key = decision == "solo"
            ? GameTextKeys.Domain.SoloChallenge
            : GameTextKeys.Domain.StartChallenge;

        Assert.True(sut.IsConfirmText(recognizedText, key));
    }

    [Theory]
    [InlineData("zh-Hans", "激活石化古树以收取秘宝。激活将消耗20个原粹树脂，当前拥有的原粹树脂数量不足，是否使用原石补充？")]
    [InlineData("zh-Hant", "活化石化古樹以收取秘寶。活化將消耗20個原粹樹脂，目前擁有的原粹樹脂數量不足，是否使用原石補充？")]
    [InlineData("en", "Revitalize the Petrified Tree to claim a reward. 20 Original Resin required. You don't have enough Original Resin. Purchase with Primogems?")]
    [InlineData("ja", "石化古樹を活性化させると報酬を獲得できます。活性化は天然樹脂を20個消費します。現在天然樹脂が足りませんので、原石を消費して補充しますか？")]
    [InlineData("fr", "Vous devez revitaliser l'Arbre pétrifié pour récupérer les récompenses. Cette action requiert Résine originelle ×20. Vous n'avez pas assez de Résine originelle Souhaitez-vous utiliser des primo-gemmes pour compléter ?")]
    [InlineData("pt-BR", "Revitalize a Árvore Petrificada para resgatar uma recompensa. São necessários .\n\n20 Resina Original. Você não possui Resina Original suficientes. Comprar com Gemas Essenciais?")]
    public void ResinInsufficient_MatchesOfficialFormattedPrompt(
        string culture,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(sut.IsResinInsufficient(recognizedText));
    }

    [Theory]
    [InlineData("zh-Hans", "是否仍要继续挑战该秘境")]
    [InlineData("pt-BR", "Ainda deseja realmente desafiar este difícil Domínio?")]
    public void ResinUsePrompt_AllowsOcrNoiseBetweenSemanticFragments(
        string culture,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(sut.IsResinUsePrompt(recognizedText));
    }

    [Theory]
    [InlineData("zh-Hans", "是否仍要挑战该秘境")]
    [InlineData("zh-Hant", "是否仍要挑戰該秘境")]
    [InlineData("en", "Do you still wish to challenge this Domain?")]
    [InlineData("ja", "この秘境に引き続き挑戦しますか？")]
    [InlineData("fr", "Souhaitez-vous toujours défier ce donjon ?")]
    [InlineData("pt-BR", "Ainda deseja desafiar este Domínio?")]
    public void ResinUsePrompt_MatchesAllThreeFragmentsInEveryCatalogCulture(
        string culture,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.True(sut.IsResinUsePrompt(recognizedText));
    }

    [Theory]
    [MemberData(nameof(MissingResinUsePromptFragmentCases))]
    public void ResinUsePrompt_RejectsTextMissingAnySemanticFragment(
        string culture,
        string missingFragment,
        string recognizedText)
    {
        var sut = Create(culture);

        Assert.False(
            sut.IsResinUsePrompt(recognizedText),
            $"Expected the prompt without its {missingFragment} fragment to be rejected.");
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

        yield return ["pt-BR", "resin-replenish", "Repor Resina Original"];
        yield return ["zh-Hans", "resin-replenish", "补充原粹树脂"];
        yield return ["en", "resin-replenish", "Replenish Original Resin"];

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

    public static IEnumerable<object[]> MissingResinUsePromptFragmentCases()
    {
        yield return ["zh-Hans", "lead", "是否要继续挑战这个秘境？"];
        yield return ["zh-Hans", "challenge", "请确认是否仍要继续进入这个秘境？"];
        yield return ["zh-Hans", "domain", "请确认是否仍要继续挑战？"];

        yield return ["zh-Hant", "lead", "是否要繼續挑戰這個秘境？"];
        yield return ["zh-Hant", "challenge", "請確認是否仍要繼續進入這個秘境？"];
        yield return ["zh-Hant", "domain", "請確認是否仍要繼續挑戰？"];

        yield return ["en", "lead", "Would you like to continue the challenge in this Domain?"];
        yield return ["en", "challenge", "Do you still wish to enter this Domain?"];
        yield return ["en", "domain", "Do you still wish to challenge this trial?"];

        yield return ["ja", "lead", "この秘境にもう一度挑戦しますか？"];
        yield return ["ja", "challenge", "この秘境に引き続き入りますか？"];
        yield return ["ja", "domain", "この試練に引き続き挑戦しますか？"];

        yield return ["fr", "lead", "Voulez-vous encore défier ce donjon ?"];
        yield return ["fr", "challenge", "Souhaitez-vous toujours entrer dans ce donjon ?"];
        yield return ["fr", "domain", "Souhaitez-vous toujours défier cette épreuve ?"];

        yield return ["pt-BR", "lead", "Deseja novamente desafiar este Domínio?"];
        yield return ["pt-BR", "challenge", "Ainda deseja entrar neste Domínio?"];
        yield return ["pt-BR", "domain", "Ainda deseja desafiar esta prova?"];
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

    private sealed class ChallengeAliasMatcher : IGameTextMatcher
    {
        public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null) => key switch
        {
            GameTextKeys.Domain.SoloChallenge => ["Solo Challenge", "Legacy Solo Challenge"],
            GameTextKeys.Domain.StartChallenge => ["Start Challenge", "Legacy Start Challenge"],
            _ => [key]
        };

        public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null) =>
            throw new NotSupportedException();

        public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) =>
            throw new NotSupportedException();

        public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) =>
            throw new NotSupportedException();
    }

    private sealed class SwitchingChallengeAliasMatcher : IGameTextMatcher
    {
        public bool UsePortuguese { get; set; }

        public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null) => key switch
        {
            GameTextKeys.Domain.SoloChallenge => [UsePortuguese ? "Desafio Solo" : "单人挑战"],
            GameTextKeys.Domain.StartChallenge => [UsePortuguese ? "Iniciar Desafio" : "开始挑战"],
            _ => [key]
        };

        public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null) =>
            GetAliases(key, culture)
                .Select(GameTextNormalizer.Normalize)
                .Any(alias => GameTextNormalizer.Normalize(recognizedText).Contains(alias, StringComparison.Ordinal));
        public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) => throw new NotSupportedException();
        public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) => throw new NotSupportedException();
    }

    private sealed class FixedGameCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }

    private sealed class SwitchingResinMatcher : IGameTextMatcher
    {
        public bool UsePortuguese { get; set; }

        public int GetAliasesCalls { get; private set; }

        public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null)
        {
            GetAliasesCalls++;
            return key switch
            {
                GameTextKeys.Resin.Original => [UsePortuguese ? "Resina Original" : "原粹树脂"],
                GameTextKeys.Resin.Condensed => [UsePortuguese ? "Resina Condensada" : "浓缩树脂"],
                GameTextKeys.Resin.Fragile => [UsePortuguese ? "Resina Frágil" : "脆弱树脂"],
                GameTextKeys.Resin.Transient => [UsePortuguese ? "Resina Transiente" : "须臾树脂"],
                GameTextKeys.Common.Use => [UsePortuguese ? "Usar" : "使用"],
                _ => [key]
            };
        }

        public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null) =>
            GetAliases(key, culture)
                .Select(GameTextNormalizer.Normalize)
                .Any(alias => GameTextNormalizer.Normalize(recognizedText).Contains(alias, StringComparison.Ordinal));

        public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) => throw new NotSupportedException();

        public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) => throw new NotSupportedException();
    }
}
