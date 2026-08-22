using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterGenshinImpact.GameTask.Localization;
using OpenCvSharp;

namespace BetterGenshinImpact.GameTask.AutoSkip;

public sealed class ExpeditionTextRecognizer
{
    private static readonly string[] Keys =
    [
        GameTextKeys.Expedition.Entry,
        GameTextKeys.Expedition.TimeShortened,
        GameTextKeys.Expedition.RewardsIncreased,
        GameTextKeys.Expedition.NoBonus,
        GameTextKeys.Expedition.Complete,
        GameTextKeys.Expedition.InProgress,
        GameTextKeys.Expedition.Rewards,
        GameTextKeys.Expedition.SelectCharacter,
        GameTextKeys.Common.Claim,
        GameTextKeys.AdventurersGuild.DailyCommissions,
    ];

    private readonly FrozenDictionary<string, IReadOnlyList<string>> _aliases;
    private readonly FrozenDictionary<string, IReadOnlyList<string>> _rawAliases;

    public ExpeditionTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(matcher);
        _rawAliases = Keys.ToFrozenDictionary(
            key => key,
            key => (IReadOnlyList<string>)matcher.GetAliases(key, culture)
                .ToArray()
                .AsReadOnly(),
            StringComparer.Ordinal);
        _aliases = _rawAliases.ToFrozenDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<string>)pair.Value
                .Select(GameTextNormalizer.Normalize)
                .Where(alias => alias.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
                .AsReadOnly(),
            StringComparer.Ordinal);
    }

    public string NormalizeOcrText(string recognizedText) => GameTextNormalizer.Normalize(recognizedText);

    public string GetPrimaryAlias(string key) => _rawAliases[key][0];

    public IReadOnlyList<string> NormalizeOcrTexts(IEnumerable<string> recognizedTexts)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);
        return recognizedTexts.Select(NormalizeOcrText).ToArray().AsReadOnly();
    }

    public bool IsTimeShortened(string recognizedText) => IsTimeShortenedNormalized(NormalizeOcrText(recognizedText));
    public bool IsRewardsIncreased(string recognizedText) => IsRewardsIncreasedNormalized(NormalizeOcrText(recognizedText));
    public bool IsNoBonus(string recognizedText) => IsNoBonusNormalized(NormalizeOcrText(recognizedText));
    public bool IsExpeditionComplete(string recognizedText) => IsExpeditionCompleteNormalized(NormalizeOcrText(recognizedText));
    public bool IsExpeditionInProgress(string recognizedText) => IsExpeditionInProgressNormalized(NormalizeOcrText(recognizedText));
    public bool IsExplorationDispatchRewards(string recognizedText) => IsExplorationDispatchRewardsNormalized(NormalizeOcrText(recognizedText));

    public bool IsTimeShortened(IEnumerable<string> recognizedTexts) => IsTimeShortenedNormalized(CombineNormalizedOcrTexts(recognizedTexts));
    public bool IsRewardsIncreased(IEnumerable<string> recognizedTexts) => IsRewardsIncreasedNormalized(CombineNormalizedOcrTexts(recognizedTexts));
    public bool IsNoBonus(IEnumerable<string> recognizedTexts) => IsNoBonusNormalized(CombineNormalizedOcrTexts(recognizedTexts));
    public bool IsExpeditionComplete(IEnumerable<string> recognizedTexts) => IsExpeditionCompleteNormalized(CombineNormalizedOcrTexts(recognizedTexts));
    public bool IsExpeditionInProgress(IEnumerable<string> recognizedTexts) => IsExpeditionInProgressNormalized(CombineNormalizedOcrTexts(recognizedTexts));
    public bool IsExplorationDispatchRewards(IEnumerable<string> recognizedTexts) => IsExplorationDispatchRewardsNormalized(CombineNormalizedOcrTexts(recognizedTexts));

    public bool IsExpeditionBonus(IEnumerable<string> recognizedTexts)
    {
        var normalizedText = CombineNormalizedOcrTexts(recognizedTexts);
        return IsExpeditionBonusNormalized(normalizedText);
    }

    public bool IsExpeditionState(IEnumerable<string> recognizedTexts)
    {
        var normalizedText = CombineNormalizedOcrTexts(recognizedTexts);
        return IsExpeditionStateNormalized(normalizedText);
    }

    public bool IsDailyCommission(IEnumerable<string> recognizedTexts) =>
        IsDailyCommissionNormalized(CombineNormalizedOcrTexts(recognizedTexts));

    public bool IsExplorationDispatch(IEnumerable<string> recognizedTexts) =>
        IsExplorationDispatchNormalized(CombineNormalizedOcrTexts(recognizedTexts));

    public bool IsExcludedDialogueOption(IEnumerable<string> recognizedTexts)
    {
        var normalizedText = CombineNormalizedOcrTexts(recognizedTexts);
        return IsExcludedDialogueOptionNormalized(normalizedText);
    }

    public bool IsTimeShortenedNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.TimeShortened);
    public bool IsRewardsIncreasedNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.RewardsIncreased);
    public bool IsNoBonusNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.NoBonus);
    public bool IsExpeditionCompleteNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.Complete);
    public bool IsExpeditionInProgressNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.InProgress);
    public bool IsExplorationDispatchRewardsNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.Rewards);
    public bool IsDailyCommissionNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.AdventurersGuild.DailyCommissions);
    public bool IsExplorationDispatchNormalized(string normalizedText) => IsMatch(normalizedText, GameTextKeys.Expedition.Entry);

    public bool IsExpeditionBonusNormalized(string normalizedText) =>
        IsTimeShortenedNormalized(normalizedText)
        || IsRewardsIncreasedNormalized(normalizedText)
        || IsNoBonusNormalized(normalizedText);

    public bool IsExpeditionStateNormalized(string normalizedText) =>
        IsExpeditionCompleteNormalized(normalizedText)
        || IsExpeditionInProgressNormalized(normalizedText);

    public bool IsExcludedDialogueOptionNormalized(string normalizedText) =>
        IsDailyCommissionNormalized(normalizedText) || IsExplorationDispatchNormalized(normalizedText);

    public string RemoveExpeditionStateText(string recognizedText)
    {
        ArgumentNullException.ThrowIfNull(recognizedText);
        foreach (var alias in _rawAliases[GameTextKeys.Expedition.Complete]
                     .Concat(_rawAliases[GameTextKeys.Expedition.InProgress]))
        {
            recognizedText = recognizedText.Replace(alias, string.Empty, StringComparison.Ordinal);
        }

        return recognizedText.Replace("/", string.Empty, StringComparison.Ordinal).Trim();
    }

    private string CombineNormalizedOcrTexts(IEnumerable<string> recognizedTexts) =>
        string.Concat(NormalizeOcrTexts(recognizedTexts));

    private bool IsMatch(string normalizedText, string key) =>
        normalizedText.Length > 0 && _aliases[key].Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
}

internal static class ExpeditionOcrFragmentGrouping
{
    public static IReadOnlyList<string> GetSameLineTexts(
        Rect anchorRect,
        IEnumerable<(Rect Rect, string Text)> fragments)
    {
        ArgumentNullException.ThrowIfNull(fragments);
        return fragments
            .Where(fragment => AreOnSameLine(anchorRect, fragment.Rect))
            .OrderBy(fragment => fragment.Rect.Y)
            .ThenBy(fragment => fragment.Rect.X)
            .Select(fragment => fragment.Text)
            .ToArray();
    }

    internal static bool AreOnSameLine(Rect first, Rect second) =>
        first.Y < second.Y + second.Height && second.Y < first.Y + first.Height;
}
