using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoDomain;

public sealed class DomainTextRecognizer
{
    private static readonly string[] Keys =
    [
        GameTextKeys.Domain.ChallengeCompleted,
        GameTextKeys.Domain.AutoLeaving,
        GameTextKeys.Domain.Skip,
        GameTextKeys.Domain.LeyLineDisorder,
        GameTextKeys.Common.ClickAnywhereToClose,
        GameTextKeys.Artifact.QuickSelect,
        GameTextKeys.Artifact.Star2,
        GameTextKeys.Domain.LimitedTimeFullyOpen,
        GameTextKeys.Domain.SoloChallenge,
        GameTextKeys.Domain.StartChallenge,
        GameTextKeys.Domain.PetrifiedTree,
        GameTextKeys.Resin.Insufficient,
        GameTextKeys.Resin.Replenish,
        GameTextKeys.Domain.ResinUsePromptLead,
        GameTextKeys.Domain.ResinUsePromptChallenge,
        GameTextKeys.Domain.ResinUsePromptDomain,
        GameTextKeys.Resin.Original,
        GameTextKeys.Resin.Condensed,
        GameTextKeys.Resin.Fragile,
        GameTextKeys.Resin.Transient,
        GameTextKeys.Common.Use,
        GameTextKeys.Common.Cancel,
    ];

    private readonly FrozenDictionary<string, IReadOnlyList<string>> _rawAliases;
    private readonly FrozenDictionary<string, IReadOnlyList<string>> _aliases;

    public DomainTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(matcher);
        _rawAliases = Keys.ToFrozenDictionary(
            key => key,
            key => (IReadOnlyList<string>)matcher.GetAliases(key, culture)
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Distinct(StringComparer.Ordinal)
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
        SoloChallengeOcrMatchAliases = _rawAliases[GameTextKeys.Domain.SoloChallenge];
        StartChallengeOcrMatchAliases = _rawAliases[GameTextKeys.Domain.StartChallenge];
    }

    public IReadOnlyList<string> SoloChallengeOcrMatchAliases { get; }

    public IReadOnlyList<string> StartChallengeOcrMatchAliases { get; }

    public bool IsChallengeCompleted(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.ChallengeCompleted);

    public bool IsAutoLeaving(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.AutoLeaving);

    public bool IsSkip(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.Skip);

    public bool IsLeyLineDisorder(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.LeyLineDisorder);

    public bool IsClickAnywhereToClose(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Common.ClickAnywhereToClose);

    public bool IsQuickSelect(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Artifact.QuickSelect);

    public bool IsTwoStarArtifact(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Artifact.Star2);

    public bool IsLimitedTimeFullyOpen(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.LimitedTimeFullyOpen);

    public bool IsSoloChallenge(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.SoloChallenge);

    public bool IsStartChallenge(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.StartChallenge);

    public bool IsPetrifiedTree(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.PetrifiedTree);

    public bool IsResinInsufficient(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Resin.Insufficient);

    public bool IsResinReplenishPrompt(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Resin.Replenish);

    public bool IsResinUsePrompt(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Domain.ResinUsePromptLead) &&
        IsMatch(recognizedText, GameTextKeys.Domain.ResinUsePromptChallenge) &&
        IsMatch(recognizedText, GameTextKeys.Domain.ResinUsePromptDomain);

    public bool IsOriginalResin(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Resin.Original);

    public bool IsCondensedResin(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Resin.Condensed);

    public bool IsFragileResin(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Resin.Fragile);

    public bool IsTransientResin(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Resin.Transient);

    public bool IsUse(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Common.Use);

    public bool IsCancel(string recognizedText) =>
        IsMatch(recognizedText, GameTextKeys.Common.Cancel);

    internal bool IsConfirmText(string recognizedText, string key) =>
        key switch
        {
            GameTextKeys.Domain.SoloChallenge => IsSoloChallenge(recognizedText),
            GameTextKeys.Domain.StartChallenge => IsStartChallenge(recognizedText),
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unsupported domain confirmation text key.")
        };

    private bool IsMatch(string recognizedText, string key)
    {
        var normalizedText = GameTextNormalizer.Normalize(recognizedText);
        return normalizedText.Length > 0
               && _aliases[key].Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
    }

}
