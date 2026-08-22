using System.Linq;
using BetterGenshinImpact.Core.BgiVision;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.Core.Script.Dependence;

public sealed class GameTextScriptApi(
    IGameTextMatcher matcher,
    IGameCultureProvider cultureProvider)
{
    public string[] aliases(string key) => matcher.GetAliases(key).ToArray();

    public bool matches(string recognizedText, string key) =>
        matcher.IsMatch(recognizedText, key);

    public bool matchesAny(object recognizedTexts, string key) =>
        matcher.IsAnyMatch(BvPage.ParseCollection<string>(recognizedTexts, nameof(recognizedTexts)), key);

    public string culture => cultureProvider.CurrentCulture.Name;
}
