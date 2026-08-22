using System;
using System.Globalization;

namespace BetterGenshinImpact.GameTask.Localization;

public sealed class ConfiguredGameCultureProvider : IGameCultureProvider
{
    private readonly Func<string> _configuredCultureName;

    public ConfiguredGameCultureProvider(Func<string> configuredCultureName)
    {
        _configuredCultureName = configuredCultureName ?? throw new ArgumentNullException(nameof(configuredCultureName));
    }

    public CultureInfo CurrentCulture
    {
        get
        {
            var configuredCultureName = _configuredCultureName();
            try
            {
                return CultureInfo.GetCultureInfo(configuredCultureName);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException(
                    $"Configured game culture '{configuredCultureName ?? "<null>"}' is invalid.",
                    exception);
            }
        }
    }
}
