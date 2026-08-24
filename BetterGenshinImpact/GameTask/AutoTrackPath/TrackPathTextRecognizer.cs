using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoTrackPath;

public sealed class TrackPathTextRecognizer
{
    private static readonly FrozenDictionary<string, string> AreaKeys =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["尘歌壶"] = GameTextKeys.WorldArea.SereniteaPot,
            ["挪德卡莱"] = GameTextKeys.WorldArea.NodKrai,
            ["至冬"] = GameTextKeys.WorldArea.Snezhnaya,
            ["枫丹"] = GameTextKeys.WorldArea.Fontaine,
            ["璃月"] = GameTextKeys.WorldArea.Liyue,
            ["稻妻"] = GameTextKeys.WorldArea.Inazuma,
            ["纳塔"] = GameTextKeys.WorldArea.Natlan,
            ["蒙德"] = GameTextKeys.WorldArea.Mondstadt,
            ["须弥"] = GameTextKeys.WorldArea.Sumeru
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private readonly IGameTextMatcher _matcher;
    private readonly CultureInfo? _culture;

    public TrackPathTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        _culture = culture;
    }

    public bool IsSwitchAreaCandidateMatch(string recognizedText, string routeAreaName)
    {
        ArgumentNullException.ThrowIfNull(recognizedText);
        ArgumentException.ThrowIfNullOrWhiteSpace(routeAreaName);

        if (AreaKeys.TryGetValue(routeAreaName, out var key))
        {
            return _matcher.IsMatch(recognizedText, key, _culture);
        }

        var normalizedRouteArea = GameTextNormalizer.Normalize(routeAreaName);
        return normalizedRouteArea.Length > 0
               && GameTextNormalizer.Normalize(recognizedText)
                   .Contains(normalizedRouteArea, StringComparison.Ordinal);
    }
}
