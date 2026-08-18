using System;
using System.Globalization;
using System.Linq;
using BetterGenshinImpact.Core.Localization;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Model.Area;
using static BetterGenshinImpact.GameTask.Common.TaskControl;

namespace BetterGenshinImpact.Core.Script.Dependence;

public partial class Genshin
{
    public string GameCulture => TaskContext.Instance().Config.OtherConfig.GameCultureInfoName;

    public string GetText(string key) => GameTextCatalog.Get(key, GetConfiguredGameCulture());

    public string[] GetTexts(string key) => [.. GameTextCatalog.GetAll(key, GetConfiguredGameCulture())];

    public string GetTextLiteral(string canonicalZhHans) =>
        GameLiteralCatalog.Get(canonicalZhHans, GetConfiguredGameCulture());

    public string[] GetTextLiterals(string canonicalZhHans) =>
        [.. GameLiteralCatalog.GetAll(canonicalZhHans, GetConfiguredGameCulture())];

    /// <summary>
    /// Resolve a historical Chinese string embedded in an older JavaScript script.
    /// Semantic game-text entries are preferred, followed by audited literal mappings.
    /// Unknown strings are preserved verbatim for backward compatibility.
    /// </summary>
    public string GetLegacyText(string canonicalText) =>
        LegacyScriptTextResolver.Get(canonicalText, GetConfiguredGameCulture());

    public string[] GetLegacyTexts(string canonicalText) =>
        [.. LegacyScriptTextResolver.GetAll(canonicalText, GetConfiguredGameCulture())];

    public bool TextContainsLiteral(string actualText, string canonicalZhHans) =>
        MatchLiteral(actualText, canonicalZhHans, static (actual, expected) =>
            actual.Contains(expected, StringComparison.OrdinalIgnoreCase));

    public bool TextEqualsLiteral(string actualText, string canonicalZhHans) =>
        MatchLiteral(actualText, canonicalZhHans, static (actual, expected) =>
            string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase));

    public bool TextStartsWithLiteral(string actualText, string canonicalZhHans) =>
        MatchLiteral(actualText, canonicalZhHans, static (actual, expected) =>
            actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase));

    public bool TextEndsWithLiteral(string actualText, string canonicalZhHans) =>
        MatchLiteral(actualText, canonicalZhHans, static (actual, expected) =>
            actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase));

    public Region FindTextKey(string key, ImageRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        return FindMatchingOcrRegion(GetTexts(key), region.FindMulti(new RecognitionObject { RecognitionType = RecognitionTypes.Ocr }));
    }

    public Region FindTextKey(string key, double x, double y, double width, double height)
    {
        using var capture = CaptureToRectArea();
        return FindMatchingOcrRegion(GetTexts(key), capture.FindMulti(RecognitionObject.Ocr(x, y, width, height)));
    }

    public bool HasTextKey(string key, ImageRegion region) => !FindTextKey(key, region).IsEmpty();

    public bool HasTextKey(string key, double x, double y, double width, double height) =>
        !FindTextKey(key, x, y, width, height).IsEmpty();

    public string FindTextKeyText(string key, ImageRegion region)
    {
        var result = FindTextKey(key, region);
        return result.IsEmpty() ? string.Empty : result.Text ?? string.Empty;
    }

    public string FindTextKeyText(string key, double x, double y, double width, double height)
    {
        var result = FindTextKey(key, x, y, width, height);
        return result.IsEmpty() ? string.Empty : result.Text ?? string.Empty;
    }

    public bool FindTextKeyAndClick(string key, ImageRegion region) => ClickIfPresent(FindTextKey(key, region));

    public bool FindTextKeyAndClick(string key, double x, double y, double width, double height) =>
        ClickIfPresent(FindTextKey(key, x, y, width, height));

    private bool MatchLiteral(string actualText, string canonicalZhHans, Func<string, string, bool> match)
    {
        if (string.IsNullOrWhiteSpace(actualText))
        {
            return false;
        }

        var actual = NormalizeOcrText(actualText);
        return GetTextLiterals(canonicalZhHans)
            .Select(NormalizeOcrText)
            .Any(expected => match(actual, expected));
    }

    private static Region FindMatchingOcrRegion(string[] acceptedTexts, System.Collections.Generic.IEnumerable<Region> results)
    {
        var accepted = acceptedTexts.Select(NormalizeOcrText).ToArray();
        return results.FirstOrDefault(result =>
        {
            if (result == null || !result.IsExist() || string.IsNullOrWhiteSpace(result.Text))
            {
                return false;
            }

            var actual = NormalizeOcrText(result.Text);
            return accepted.Any(expected => actual.Contains(expected, StringComparison.OrdinalIgnoreCase));
        }) ?? new Region();
    }

    private static bool ClickIfPresent(Region result)
    {
        if (result.IsEmpty())
        {
            return false;
        }

        result.Click();
        return true;
    }

    private static string NormalizeOcrText(string text) =>
        string.Concat(text.Where(character => !char.IsWhiteSpace(character)));

    private CultureInfo GetConfiguredGameCulture()
    {
        if (string.IsNullOrWhiteSpace(GameCulture))
        {
            throw new InvalidOperationException("GameCultureInfoName is empty. Configure the Genshin game language before using localized script text.");
        }

        try
        {
            return new CultureInfo(GameCulture);
        }
        catch (CultureNotFoundException exception)
        {
            throw new InvalidOperationException($"Unsupported Genshin game culture: '{GameCulture}'.", exception);
        }
    }
}
