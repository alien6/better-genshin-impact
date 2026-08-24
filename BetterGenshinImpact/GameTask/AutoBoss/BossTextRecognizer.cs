using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.Helpers;

namespace BetterGenshinImpact.GameTask.AutoBoss;

public sealed class BossTextRecognizer
{
    private static readonly string[] Keys =
    [
        GameTextKeys.Boss.FullResinRecovered,
        GameTextKeys.Boss.FullRecovery,
        GameTextKeys.Resin.Replenish,
        GameTextKeys.Resin.Transient,
        GameTextKeys.Resin.Fragile,
        GameTextKeys.Resin.Original,
        GameTextKeys.Resin.Insufficient,
        GameTextKeys.Common.Use,
        GameTextKeys.Boss.QuickUse,
        GameTextKeys.Boss.UseQuantity,
        GameTextKeys.Boss.Obtain,
        GameTextKeys.Boss.TouchTrounceBlossom,
        GameTextKeys.Boss.ClickBlankAreaToContinue,
        GameTextKeys.Boss.UsePromptLead,
        GameTextKeys.Boss.ReplenishPromptLead,
        GameTextKeys.Boss.ReplenishPromptOriginal,
        GameTextKeys.Boss.ReplenishPromptResin,
    ];

    private readonly FrozenDictionary<string, IReadOnlyList<string>> _aliases;

    public BossTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(matcher);
        _aliases = Keys.ToFrozenDictionary(
            key => key,
            key => (IReadOnlyList<string>)matcher.GetAliases(key, culture)
                .Select(GameTextNormalizer.Normalize)
                .Where(alias => alias.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
                .AsReadOnly(),
            StringComparer.Ordinal);
    }

    public bool IsFullResinRecovered(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.FullResinRecovered);
    public bool IsFullRecovery(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.FullRecovery);

    public TimeSpan? TryGetFullRecoveryTime(string recognizedText)
    {
        if (IsFullResinRecovered(recognizedText))
        {
            return TimeSpan.Zero;
        }

        var recoveryText = StringUtils.ConvertFullWidthNumToHalfWidth(recognizedText).Replace('：', ':');
        var match = Regex.Matches(
                recoveryText,
                @"(?<hours>\d{1,3}):(?<minutes>\d{2}):(?<seconds>\d{2})")
            .Cast<Match>()
            .FirstOrDefault(candidate => IsFullRecovery(recoveryText[..candidate.Index]));
        return match is { Success: true }
            && int.TryParse(match.Groups["hours"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var hours)
            && int.TryParse(match.Groups["minutes"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var minutes)
            && int.TryParse(match.Groups["seconds"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            && minutes is >= 0 and <= 59
            && seconds is >= 0 and <= 59
            ? new TimeSpan(hours, minutes, seconds)
            : null;
    }

    public bool IsReplenishOriginalResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Replenish);
    public bool IsTransientResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Transient);
    public bool IsFragileResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Fragile);
    public bool IsOriginalResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Original);
    public bool IsResinInsufficient(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Insufficient);
    public bool IsUse(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Common.Use);
    public bool IsQuickUse(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.QuickUse);
    public bool IsObtain(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.Obtain);
    public bool IsTouchTrounceBlossom(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.TouchTrounceBlossom);
    public bool IsClickBlankAreaToContinue(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.ClickBlankAreaToContinue);
    public bool IsRewardUsePromptLead(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Boss.UsePromptLead);

    public bool IsRewardUseOriginalResinPrompt(string recognizedText) =>
        IsRewardUsePromptLead(recognizedText) && IsOriginalResin(recognizedText);

    public bool IsRewardUseOriginalResinPrompt(IEnumerable<string> recognizedTexts)
    {
        var texts = Materialize(recognizedTexts);
        return IsMatch(texts, GameTextKeys.Boss.UsePromptLead)
            && IsMatch(texts, GameTextKeys.Resin.Original);
    }

    public bool IsSupplementPrompt(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Boss.ReplenishPromptLead) &&
        IsMatch(recognizedText, GameTextKeys.Boss.ReplenishPromptOriginal) &&
        IsMatch(recognizedText, GameTextKeys.Boss.ReplenishPromptResin);

    public bool IsSupplementPrompt(IEnumerable<string> recognizedTexts)
    {
        var texts = Materialize(recognizedTexts);
        return IsMatch(texts, GameTextKeys.Boss.ReplenishPromptLead)
            && IsMatch(texts, GameTextKeys.Boss.ReplenishPromptOriginal)
            && IsMatch(texts, GameTextKeys.Boss.ReplenishPromptResin);
    }

    public int? TryGetUseQuantity(string recognizedText)
    {
        if (!IsMatch(recognizedText, GameTextKeys.Boss.UseQuantity))
        {
            return null;
        }

        var normalizedText = GameTextNormalizer.Normalize(
            StringUtils.ConvertFullWidthNumToHalfWidth(recognizedText));
        Match? match = null;
        foreach (var alias in _aliases[GameTextKeys.Boss.UseQuantity])
        {
            var aliasIndex = normalizedText.IndexOf(alias, StringComparison.Ordinal);
            if (aliasIndex < 0)
            {
                continue;
            }

            var candidate = Regex.Match(normalizedText[(aliasIndex + alias.Length)..], @"\d+");
            if (candidate.Success)
            {
                match = candidate;
                break;
            }
        }

        match ??= Regex.Match(normalizedText, @"\d+");
        return match.Success && int.TryParse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var quantity)
            ? quantity
            : null;
    }

    private bool IsMatch(string recognizedText, string key)
    {
        var normalizedText = GameTextNormalizer.Normalize(recognizedText);
        return normalizedText.Length > 0 && _aliases[key].Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
    }

    private bool IsMatch(IReadOnlyList<string> recognizedTexts, string key) =>
        IsMatch(string.Concat(recognizedTexts), key);

    private static IReadOnlyList<string> Materialize(IEnumerable<string> recognizedTexts)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);
        return recognizedTexts as IReadOnlyList<string> ?? recognizedTexts.ToArray();
    }
}
