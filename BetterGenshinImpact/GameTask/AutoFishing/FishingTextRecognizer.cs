using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoFishing;

public sealed class FishingTextRecognizer
{
    private readonly IGameTextMatcher _matcher;
    private readonly CultureInfo? _culture;

    public FishingTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        _culture = culture;
        FishingSearchPattern = string.Join(
            '|',
            _matcher.GetAliases(GameTextKeys.Fishing.Action, _culture).Select(Regex.Escape));
    }

    public string FishingSearchPattern { get; }

    public bool IsBite(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Fishing.Bite, _culture);

    public bool IsFishingPrompt(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Fishing.Action, _culture);
}
