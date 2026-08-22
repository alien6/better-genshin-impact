using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoStygianOnslaught;

public sealed class StygianTextRecognizer
{
    private static readonly string[] Keys =
    [
        GameTextKeys.Stygian.Return,
        GameTextKeys.Stygian.ChallengeFailed,
        GameTextKeys.Stygian.RetryChallenge,
        GameTextKeys.Stygian.LeyLineBlossom,
        GameTextKeys.Stygian.CharacterPreview,
        GameTextKeys.Stygian.EventName,
        GameTextKeys.Stygian.EventOverview,
        GameTextKeys.Stygian.RewardPhase,
        GameTextKeys.Stygian.PhaseEnded,
        GameTextKeys.Stygian.GoToChallenge,
        GameTextKeys.Stygian.HardDifficulty,
        GameTextKeys.Stygian.UltimateChallenge,
        GameTextKeys.Stygian.NormalChallenge,
        GameTextKeys.Stygian.PresetTeams,
        GameTextKeys.Domain.SoloChallenge,
        GameTextKeys.Domain.StartChallenge,
        GameTextKeys.Resin.Original,
        GameTextKeys.Resin.Condensed,
        GameTextKeys.Resin.Insufficient,
        GameTextKeys.Resin.Replenish,
        GameTextKeys.LeyLine.Activate,
    ];

    private readonly FrozenDictionary<string, IReadOnlyList<string>> _aliases;

    public StygianTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
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

    public string NormalizeOcrText(string recognizedText) => GameTextNormalizer.Normalize(recognizedText);

    public IReadOnlyList<string> NormalizeOcrTexts(IEnumerable<string> recognizedTexts)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);
        return recognizedTexts.Select(NormalizeOcrText).ToArray().AsReadOnly();
    }

    public bool IsReturn(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.Return);
    public bool IsChallengeFailed(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.ChallengeFailed);
    public bool IsRetryChallenge(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.RetryChallenge);
    public bool IsLeyLineBlossom(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.LeyLineBlossom);
    public bool IsCharacterPreview(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.CharacterPreview);
    public bool IsStartChallenge(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Domain.StartChallenge);
    public bool IsSoloChallenge(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Domain.SoloChallenge);
    public bool IsEventName(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.EventName);
    public bool IsEventOverview(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.EventOverview);
    public bool IsResinInsufficient(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Insufficient);
    public bool IsSupplementResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Replenish);
    public bool IsActivate(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Activate);
    public bool IsGoToChallenge(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.GoToChallenge);
    public bool IsHardDifficulty(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.HardDifficulty);
    public bool IsUltimateChallenge(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.UltimateChallenge);
    public bool IsNormalChallenge(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.NormalChallenge);
    public bool IsPresetTeams(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Stygian.PresetTeams);

    public bool IsResinSelect(string recognizedText) =>
        IsLeyLineBlossom(recognizedText) &&
        (IsMatch(recognizedText, GameTextKeys.Resin.Original) || IsMatch(recognizedText, GameTextKeys.Resin.Condensed));

    public bool IsRewardPhaseEnded(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Stygian.RewardPhase) &&
        IsMatch(recognizedText, GameTextKeys.Stygian.PhaseEnded);

    public bool IsRewardPhaseEnded(IReadOnlyList<string> recognizedTexts) =>
        IsRewardPhaseEnded(string.Concat(recognizedTexts));

    public bool IsResinSelect(IReadOnlyList<string> recognizedTexts) =>
        IsResinSelect(string.Concat(recognizedTexts));

    private bool IsMatch(string recognizedText, string key) =>
        recognizedText.Length > 0 && _aliases[key].Any(alias => recognizedText.Contains(alias, StringComparison.Ordinal));
}
