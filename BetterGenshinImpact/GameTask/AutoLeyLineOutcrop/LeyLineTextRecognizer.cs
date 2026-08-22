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

    public bool IsOriginalResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Original);
    public bool IsCondensedResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Condensed);
    public bool IsTransientResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Transient);
    public bool IsFragileResin(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Fragile);
    public bool IsReplenish(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Resin.Replenish);
    public bool IsDoubleReward(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.DoubleReward);
    public bool IsDoubleReward2x(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.DoubleReward2x);
    public bool IsTouch(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Touch);
    public bool IsActivate(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Activate);
    public bool IsSelect(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Select);
    public bool IsLeyLine(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Line);
    public bool IsOutcrop(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Outcrop);
    public bool IsBlossomOfWealth(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.BlossomOfWealth);
    public bool IsBlossomOfRevelation(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.BlossomOfRevelation);
    public bool IsRevive(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Common.Revive);
    public bool IsUse(string recognizedText) => IsMatch(recognizedText, GameTextKeys.Common.Use);
    public bool IsStop(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.Stop);
    public bool IsOriginalResin40Prompt(string recognizedText) => IsMatch(recognizedText, GameTextKeys.LeyLine.OriginalResin40Prompt);

    public bool IsAllowedResinOption(string recognizedText) =>
        IsOriginalResin(recognizedText)
        || IsCondensedResin(recognizedText)
        || IsTransientResin(recognizedText)
        || IsFragileResin(recognizedText);

    public bool IsRewardBlossomPrompt(IEnumerable<string> recognizedTexts)
    {
        var texts = Materialize(recognizedTexts);
        return IsCombinedMatch(texts, GameTextKeys.LeyLine.Line)
            && (IsCombinedMatch(texts, GameTextKeys.LeyLine.Activate)
                || IsCombinedMatch(texts, GameTextKeys.LeyLine.Select));
    }

    public bool IsConfiguredResin(string recognizedText, string resinName) => resinName switch
    {
        "原粹树脂" => IsOriginalResin(recognizedText),
        "浓缩树脂" => IsCondensedResin(recognizedText),
        "须臾树脂" => IsTransientResin(recognizedText),
        "脆弱树脂" => IsFragileResin(recognizedText),
        _ => false
    };

    private bool IsMatch(string recognizedText, string key) =>
        IsCombinedMatch([recognizedText], key);

    private bool IsCombinedMatch(IReadOnlyList<string> recognizedTexts, string key)
    {
        var normalizedText = GameTextNormalizer.Normalize(string.Concat(recognizedTexts));
        return normalizedText.Length > 0
               && _aliases[key].Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
    }

    private static IReadOnlyList<string> Materialize(IEnumerable<string> recognizedTexts)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);
        return recognizedTexts as IReadOnlyList<string> ?? recognizedTexts.ToArray();
    }
}
