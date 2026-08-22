using System.Globalization;

namespace BetterGenshinImpact.GameTask.Localization;

public interface IGameCultureProvider
{
    CultureInfo CurrentCulture { get; }
}
