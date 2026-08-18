using System.Globalization;
using BetterGenshinImpact.Core.Localization;
using BetterGenshinImpact.GameTask;

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
