using System;
using System.Collections.Generic;
using System.Globalization;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoDomain;

public sealed class DomainTextRecognizer
{
    private readonly IGameTextMatcher _matcher;
    private readonly CultureInfo? _culture;

    public DomainTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        _culture = culture;
        SoloChallengeAliases = _matcher.GetAliases(GameTextKeys.Domain.SoloChallenge, _culture);
        StartChallengeAliases = _matcher.GetAliases(GameTextKeys.Domain.StartChallenge, _culture);
    }

    public IReadOnlyList<string> SoloChallengeAliases { get; }

    public IReadOnlyList<string> StartChallengeAliases { get; }

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
        IsMatch(recognizedText, GameTextKeys.Domain.ResinUsePrompt);

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

    private bool IsMatch(string recognizedText, string key) =>
        _matcher.IsMatch(recognizedText, key, _culture);
}
