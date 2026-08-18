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
    /// <summary>Current Genshin client culture configured in BetterGI.</summary>
    public string GameCulture => TaskContext.Instance().Config.OtherConfig.GameCultureInfoName;

    /// <summary>Resolve a stable semantic game-text key.</summary>
    public string GetText(string key) => GameTextCatalog.Get(key, GetConfiguredGameCulture());

    /// <summary>Resolve all accepted OCR variants for a semantic game-text key.</summary>
    public string[] GetTexts(string key) => [.. GameTextCatalog.GetAll(key, GetConfiguredGameCulture())];

    /// <summary>
    /// Resolve script-specific canonical Simplified-Chinese game text through an
    /// exact TextMap mapping. Prefer GetText/GetTexts when a semantic key exists.
    /// </summary>
    public string GetTextLiteral(string canonicalZhHans) =>
        GameLiteralCatalog.Get(canonicalZhHans, GetConfiguredGameCulture());

    /// <summary>Resolve every accepted TextMap variant for a canonical game literal.</summary>
    public string[] GetTextLiterals(string canonicalZhHans) =>
        [.. GameLiteralCatalog.GetAll(canonicalZhHans, GetConfiguredGameCulture())];

    /// <summary>Check whether OCR text contains any localized TextMap variant.</summary>
    public bool TextContainsLiteral(string actualText, string canonicalZhHans)
    {
        if (string.IsNullOrWhiteSpace(actualText))
        {
            return false;
        }

        var actual = NormalizeOcrText(actualText);
        return GetTextLiterals(canonicalZhHans)
            .Select(NormalizeOcrText)
            .Any(expected => actual.Contains(expected, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Check whether OCR text equals any localized TextMap variant.</summary>
    public bool TextEqualsLiteral(string actualText, string canonicalZhHans)
    {
        if (string.IsNullOrWhiteSpace(actualText))
        {
            return false;
        }

        var actual = NormalizeOcrText(actualText);
        return GetTextLiterals(canonicalZhHans)
            .Select(NormalizeOcrText)
            .Any(expected => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Find the exact OCR result region matching a semantic key.</summary>
    public Region FindTextKey(string key, ImageRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        return FindMatchingOcrRegion(GetTexts(key), region.FindMulti(new RecognitionObject { RecognitionType = RecognitionTypes.Ocr }));
    }

    /// <summary>Find a semantic key in a capture rectangle.</summary>
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
