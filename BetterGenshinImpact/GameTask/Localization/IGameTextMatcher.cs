using System.Globalization;
using System.Collections.Generic;

namespace BetterGenshinImpact.GameTask.Localization;

public interface IGameTextMatcher
{
    IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null);

    bool IsMatch(string recognizedText, string key, CultureInfo? culture = null);

    bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null);

    bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null);
}
