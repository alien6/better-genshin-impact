using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoLeyLineOutcrop;

public sealed class LeyLineTextRecognizer
{
    private static readonly string[] Keys =
    [
        GameTextKeys.Resin.Original,
        GameTextKeys.Resin.Condensed,
        GameTextKeys.Resin.Transient,
        GameTextKeys.Resin.Fragile,
        GameTextKeys.Resin.Replenish,
        GameTextKeys.Common.Revive,
        GameTextKeys.Common.Use,
        GameTextKeys.LeyLine.DoubleReward,
        GameTextKeys.LeyLine.DoubleReward2x,
        GameTextKeys.LeyLine.Touch,
        GameTextKeys.LeyLine.Activate,
        GameTextKeys.LeyLine.Select,
        GameTextKeys.LeyLine.Line,
        GameTextKeys.LeyLine.Outcrop,
        GameTextKeys.LeyLine.BlossomOfWealth,
        GameTextKeys.LeyLine.BlossomOfRevelation,
        GameTextKeys.LeyLine.Stop,
        GameTextKeys.LeyLine.OriginalResin40Prompt,
        GameTextKeys.LeyLine.FightSuccess,
        GameTextKeys.LeyLine.FightFailure,
        GameTextKeys.LeyLine.FightObjective,
    ];

    private readonly FrozenDictionary<string, IReadOnlyList<string>> _aliases;

    public LeyLineTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
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

    public bool IsOriginalResin(string recognizedText) => IsOriginalResinNormalized(NormalizeOcrText(recognizedText));
    public bool IsCondensedResin(string recognizedText) => IsCondensedResinNormalized(NormalizeOcrText(recognizedText));
    public bool IsTransientResin(string recognizedText) => IsTransientResinNormalized(NormalizeOcrText(recognizedText));
    public bool IsFragileResin(string recognizedText) => IsFragileResinNormalized(NormalizeOcrText(recognizedText));
    public bool IsReplenish(string recognizedText) => IsReplenishNormalized(NormalizeOcrText(recognizedText));
    public bool IsDoubleReward(string recognizedText) => IsDoubleRewardNormalized(NormalizeOcrText(recognizedText));
    public bool IsDoubleReward2x(string recognizedText) => IsDoubleReward2xNormalized(NormalizeOcrText(recognizedText));
    public bool IsTouch(string recognizedText) => IsTouchNormalized(NormalizeOcrText(recognizedText));
    public bool IsActivate(string recognizedText) => IsActivateNormalized(NormalizeOcrText(recognizedText));
    public bool IsSelect(string recognizedText) => IsSelectNormalized(NormalizeOcrText(recognizedText));
    public bool IsLeyLine(string recognizedText) => IsLeyLineNormalized(NormalizeOcrText(recognizedText));
    public bool IsOutcrop(string recognizedText) => IsOutcropNormalized(NormalizeOcrText(recognizedText));
    public bool IsBlossomOfWealth(string recognizedText) => IsBlossomOfWealthNormalized(NormalizeOcrText(recognizedText));
    public bool IsBlossomOfRevelation(string recognizedText) => IsBlossomOfRevelationNormalized(NormalizeOcrText(recognizedText));
    public bool IsRevive(string recognizedText) => IsReviveNormalized(NormalizeOcrText(recognizedText));
    public bool IsUse(string recognizedText) => IsUseNormalized(NormalizeOcrText(recognizedText));
    public bool IsStop(string recognizedText) => IsStopNormalized(NormalizeOcrText(recognizedText));
    public bool IsOriginalResin40Prompt(string recognizedText) => IsOriginalResin40PromptNormalized(NormalizeOcrText(recognizedText));
    public bool IsFightSuccess(string recognizedText) => IsFightSuccessNormalized(NormalizeOcrText(recognizedText));
    public bool IsFightFailure(string recognizedText) => IsFightFailureNormalized(NormalizeOcrText(recognizedText));
    public bool IsFightObjective(string recognizedText) => IsFightObjectiveNormalized(NormalizeOcrText(recognizedText));

    internal bool IsOriginalResinNormalized(string text) => IsMatch(text, GameTextKeys.Resin.Original);
    internal bool IsCondensedResinNormalized(string text) => IsMatch(text, GameTextKeys.Resin.Condensed);
    internal bool IsTransientResinNormalized(string text) => IsMatch(text, GameTextKeys.Resin.Transient);
    internal bool IsFragileResinNormalized(string text) => IsMatch(text, GameTextKeys.Resin.Fragile);
    internal bool IsReplenishNormalized(string text) => IsMatch(text, GameTextKeys.Resin.Replenish);
    internal bool IsDoubleRewardNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.DoubleReward);
    internal bool IsDoubleReward2xNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.DoubleReward2x);
    internal bool IsTouchNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.Touch);
    internal bool IsActivateNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.Activate);
    internal bool IsSelectNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.Select);
    internal bool IsLeyLineNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.Line);
    internal bool IsOutcropNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.Outcrop);
    internal bool IsBlossomOfWealthNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.BlossomOfWealth);
    internal bool IsBlossomOfRevelationNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.BlossomOfRevelation);
    internal bool IsReviveNormalized(string text) => IsMatch(text, GameTextKeys.Common.Revive);
    internal bool IsUseNormalized(string text) => IsMatch(text, GameTextKeys.Common.Use);
    internal bool IsStopNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.Stop);
    internal bool IsOriginalResin40PromptNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.OriginalResin40Prompt);
    internal bool IsFightSuccessNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.FightSuccess);
    internal bool IsFightFailureNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.FightFailure);
    internal bool IsFightObjectiveNormalized(string text) => IsMatch(text, GameTextKeys.LeyLine.FightObjective);

    public string NormalizeOcrText(string recognizedText) => GameTextNormalizer.Normalize(recognizedText);

    internal IReadOnlyList<string> NormalizeOcrTexts(IEnumerable<string> recognizedTexts)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);
        return recognizedTexts.Select(NormalizeOcrText).ToArray().AsReadOnly();
    }

    public bool IsAllowedResinOption(string recognizedText) =>
        IsOriginalResin(recognizedText)
        || IsCondensedResin(recognizedText)
        || IsTransientResin(recognizedText)
        || IsFragileResin(recognizedText);

    internal bool IsAllowedResinOptionNormalized(string text) =>
        IsOriginalResinNormalized(text)
        || IsCondensedResinNormalized(text)
        || IsTransientResinNormalized(text)
        || IsFragileResinNormalized(text);

    public bool IsRewardBlossomPrompt(IEnumerable<string> recognizedTexts)
    {
        return IsRewardBlossomPromptNormalized(NormalizeOcrTexts(recognizedTexts));
    }

    public bool IsRewardBlossomTitle(IEnumerable<string> recognizedTexts)
    {
        return IsRewardBlossomTitleNormalized(NormalizeOcrTexts(recognizedTexts));
    }

    internal bool IsRewardBlossomPromptNormalized(IReadOnlyList<string> normalizedTexts) =>
        IsCombinedMatch(normalizedTexts, GameTextKeys.LeyLine.Line)
        && (IsCombinedMatch(normalizedTexts, GameTextKeys.LeyLine.Activate)
            || IsCombinedMatch(normalizedTexts, GameTextKeys.LeyLine.Select));

    internal bool IsRewardBlossomTitleNormalized(IReadOnlyList<string> normalizedTexts) =>
        IsCombinedMatch(normalizedTexts, GameTextKeys.LeyLine.Select)
        || IsRewardBlossomPromptNormalized(normalizedTexts);

    public bool IsConfiguredResin(string recognizedText, string resinName) => resinName switch
    {
        "原粹树脂" => IsOriginalResin(recognizedText),
        "浓缩树脂" => IsCondensedResin(recognizedText),
        "须臾树脂" => IsTransientResin(recognizedText),
        "脆弱树脂" => IsFragileResin(recognizedText),
        _ => false
    };

    internal bool IsConfiguredResinNormalized(string text, string resinName) => resinName switch
    {
        "原粹树脂" => IsOriginalResinNormalized(text),
        "浓缩树脂" => IsCondensedResinNormalized(text),
        "须臾树脂" => IsTransientResinNormalized(text),
        "脆弱树脂" => IsFragileResinNormalized(text),
        _ => false
    };

    private bool IsMatch(string recognizedText, string key) =>
        IsCombinedMatch([recognizedText], key);

    private bool IsCombinedMatch(IReadOnlyList<string> recognizedTexts, string key)
    {
        var recognizedText = string.Concat(recognizedTexts);
        return recognizedText.Length > 0
               && _aliases[key].Any(alias => recognizedText.Contains(alias, StringComparison.Ordinal));
    }

}
