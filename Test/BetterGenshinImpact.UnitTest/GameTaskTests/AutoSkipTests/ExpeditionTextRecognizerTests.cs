using System.Globalization;
using BetterGenshinImpact.GameTask.AutoSkip;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoSkipTests;

public class ExpeditionTextRecognizerTests
{
    [Theory]
    [InlineData("zh-Hans", "时间缩短", "探险完成", "探险中", "探索派遣奖励")]
    [InlineData("zh-Hant", "時間縮短", "探險完成", "探險中", "探索派遣獎勵")]
    [InlineData("en", "Time Shortened", "Expedition Complete", "Expedition in Progress", "Expedition Rewards")]
    [InlineData("ja", "時間短縮", "探索完了", "探索中", "探索派遣報酬")]
    [InlineData("fr", "Temps réduit", "Expédition terminée", "Expédition en cours", "Récompenses d'expédition")]
    [InlineData("pt-BR", "Tempo reduzido", "Expedição concluída", "Expedição em andamento", "Recompensas de Expedição")]
    public void ExpeditionState_MatchesLocalizedFullOcrText(
        string culture,
        string timeShortened,
        string complete,
        string inProgress,
        string rewards)
    {
        var sut = Create(culture);

        Assert.True(sut.IsTimeShortened(timeShortened));
        Assert.True(sut.IsRewardsIncreased(LocalizedRewardsIncreased(culture)));
        Assert.True(sut.IsNoBonus(LocalizedNoBonus(culture)));
        Assert.True(sut.IsExpeditionComplete(complete));
        Assert.True(sut.IsExpeditionInProgress(inProgress));
        Assert.True(sut.IsExplorationDispatchRewards(rewards));
    }

    [Theory]
    [InlineData("zh-Hans", "每日", "委托", "探索", "派遣")]
    [InlineData("zh-Hant", "每日", "委託", "探索", "派遣")]
    [InlineData("en", "Daily", "Commission", "Dispatch", "Expedition")]
    [InlineData("ja", "デイリー", "依頼", "探索", "派遣")]
    [InlineData("fr", "Missions", "quotidiennes", "Expé", "ditions")]
    [InlineData("pt-BR", "Missões", "Diárias", "Expedi", "ção")]
    public void DialogueClassification_MatchesFragmentedOcrRegions(
        string culture,
        string dailyPart1,
        string dailyPart2,
        string dispatchPart1,
        string dispatchPart2)
    {
        var sut = Create(culture);

        Assert.True(sut.IsDailyCommission([dailyPart1, dailyPart2]));
        Assert.True(sut.IsExplorationDispatch([dispatchPart1, dispatchPart2]));
        Assert.True(sut.IsExcludedDialogueOption([dailyPart1, dailyPart2]));
        Assert.True(sut.IsExcludedDialogueOption([dispatchPart1, dispatchPart2]));
        Assert.False(sut.IsExcludedDialogueOption(["A", "new", "dialogue", "option"]));
    }

    [Theory]
    [InlineData("zh-Hans", "时间", "缩短", "奖励", "增加", "暂", "无加成", "探险", "完成", "探险", "中", "探索派遣", "奖励")]
    [InlineData("zh-Hant", "時間", "縮短", "獎勵", "增加", "暫", "無加成", "探險", "完成", "探險", "中", "探索派遣", "獎勵")]
    [InlineData("en", "Time", "Shortened", "Rewards", "Increased", "No", "Bonus", "Expedition", "Complete", "Expedition", "in Progress", "Expedition", "Rewards")]
    [InlineData("ja", "時間", "短縮", "報酬", "増加", "ボーナス", "なし", "探索", "完了", "探索", "中", "探索派遣", "報酬")]
    [InlineData("fr", "Temps", "réduit", "Récompenses", "augmentées", "Aucun", "bonus", "Expédition", "terminée", "Expédition", "en cours", "Récompenses", "d'expédition")]
    [InlineData("pt-BR", "Tempo", "reduzido", "Recompensas", "aumentadas", "Sem", "bônus", "Expedição", "concluída", "Expedição", "em andamento", "Recompensas", "de Expedição")]
    public void ExpeditionState_MatchesFragmentedOcrRegions(
        string culture,
        string timePart1,
        string timePart2,
        string increasedPart1,
        string increasedPart2,
        string noBonusPart1,
        string noBonusPart2,
        string completePart1,
        string completePart2,
        string inProgressPart1,
        string inProgressPart2,
        string rewardsPart1,
        string rewardsPart2)
    {
        var sut = Create(culture);

        Assert.True(sut.IsTimeShortened([timePart1, timePart2]));
        Assert.True(sut.IsRewardsIncreased([increasedPart1, increasedPart2]));
        Assert.True(sut.IsNoBonus([noBonusPart1, noBonusPart2]));
        Assert.True(sut.IsExpeditionComplete([completePart1, completePart2]));
        Assert.True(sut.IsExpeditionInProgress([inProgressPart1, inProgressPart2]));
        Assert.True(sut.IsExplorationDispatchRewards([rewardsPart1, rewardsPart2]));
    }

    private static string LocalizedRewardsIncreased(string culture) => culture switch
    {
        "zh-Hans" => "奖励增加",
        "zh-Hant" => "獎勵增加",
        "en" => "Rewards Increased",
        "ja" => "報酬増加",
        "fr" => "Récompenses augmentées",
        "pt-BR" => "Recompensas aumentadas",
        _ => throw new ArgumentOutOfRangeException(nameof(culture))
    };

    private static string LocalizedNoBonus(string culture) => culture switch
    {
        "zh-Hans" => "暂无加成",
        "zh-Hant" => "暫無加成",
        "en" => "No Bonus",
        "ja" => "ボーナスなし",
        "fr" => "Aucun bonus",
        "pt-BR" => "Sem bônus",
        _ => throw new ArgumentOutOfRangeException(nameof(culture))
    };

    private static ExpeditionTextRecognizer Create(string culture) =>
        new(new GameTextMatcher(
            new FixedCultureProvider(CultureInfo.GetCultureInfo(culture)),
            new EmbeddedGameTextCatalogProvider()));

    private sealed class FixedCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
