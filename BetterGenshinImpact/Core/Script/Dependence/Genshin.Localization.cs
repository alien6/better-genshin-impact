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
    /// <summary>
    /// Current Genshin client culture configured in BetterGI (for example zh-Hans, en or pt-BR).
    /// </summary>
    public string GameCulture => TaskContext.Instance().Config.OtherConfig.GameCultureInfoName;

    /// <summary>
    /// Resolve a semantic game-text key for the configured Genshin client culture.
    /// </summary>
    public string GetText(string key)
    {
        return GameTextCatalog.Get(key, GetConfiguredGameCulture());
    }

    /// <summary>
    /// Resolve all accepted OCR variants for a semantic game-text key.
    /// </summary>
    public string[] GetTexts(string key)
    {
        return [.. GameTextCatalog.GetAll(key, GetConfiguredGameCulture())];
    }

    /// <summary>
    /// Find the exact OCR result region that contains any localized variant represented by a semantic key.
    /// </summary>
    public Region FindTextKey(string key, ImageRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        var recognitionObject = new RecognitionObject { RecognitionType = RecognitionTypes.Ocr };
        return FindMatchingOcrRegion(key, region.FindMulti(recognitionObject));
    }

    /// <summary>
    /// Find a semantic text key in a rectangle of the current game capture and return the exact OCR result region.
    /// </summary>
    public Region FindTextKey(string key, double x, double y, double width, double height)
    {
        using var capture = CaptureToRectArea();
        return FindMatchingOcrRegion(key, capture.FindMulti(RecognitionObject.Ocr(x, y, width, height)));
    }

    /// <summary>
    /// Find a semantic text key inside an existing image region and click the exact matching OCR result.
    /// Returns false when OCR did not find any accepted localized variant.
    /// </summary>
    public bool FindTextKeyAndClick(string key, ImageRegion region)
    {
        return ClickIfPresent(FindTextKey(key, region));
    }

    /// <summary>
    /// Find a semantic text key in a game-capture rectangle and click the exact matching OCR result.
    /// </summary>
    public bool FindTextKeyAndClick(string key, double x, double y, double width, double height)
    {
        return ClickIfPresent(FindTextKey(key, x, y, width, height));
    }

    private Region FindMatchingOcrRegion(string key, System.Collections.Generic.IEnumerable<Region> results)
    {
        var accepted = GetTexts(key).Select(NormalizeOcrText).ToArray();
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

    private static string NormalizeOcrText(string text)
    {
        return string.Concat(text.Where(character => !char.IsWhiteSpace(character)));
    }

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
