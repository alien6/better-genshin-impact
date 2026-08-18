using System.Globalization;
using BetterGenshinImpact.Core.Localization;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Model.Area;

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
    /// Find any localized OCR variant represented by a semantic key inside an existing image region.
    /// </summary>
    public Region FindTextKey(string key, ImageRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        var recognitionObject = new RecognitionObject
        {
            RecognitionType = RecognitionTypes.OcrMatch,
            OneContainMatchText = [.. GetTexts(key)]
        };
        return region.Find(recognitionObject);
    }

    /// <summary>
    /// Find a semantic text key in a rectangle of the current game capture.
    /// Coordinates use the same capture coordinate system as RecognitionObject.OcrMatch.
    /// </summary>
    public Region FindTextKey(string key, double x, double y, double width, double height)
    {
        using var capture = CaptureToRectArea();
        return capture.Find(RecognitionObject.OcrMatch(x, y, width, height, GetTexts(key)));
    }

    /// <summary>
    /// Find a semantic text key inside an existing image region and click the matched region.
    /// Returns false when OCR did not find any accepted localized variant.
    /// </summary>
    public bool FindTextKeyAndClick(string key, ImageRegion region)
    {
        var result = FindTextKey(key, region);
        if (result.IsEmpty())
        {
            return false;
        }

        result.Click();
        return true;
    }

    /// <summary>
    /// Find a semantic text key in a game-capture rectangle and click the matched region.
    /// </summary>
    public bool FindTextKeyAndClick(string key, double x, double y, double width, double height)
    {
        using var capture = CaptureToRectArea();
        var result = capture.Find(RecognitionObject.OcrMatch(x, y, width, height, GetTexts(key)));
        if (result.IsEmpty())
        {
            return false;
        }

        result.Click();
        return true;
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
