using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BetterGenshinImpact.GameTask.AutoTrackPath
{
    public class TpTaskParam
    {
        public TpTaskParam(CultureInfo? gameCultureInfo = null, IStringLocalizer<TpTask>? stringLocalizer = null)
        {
            GameCultureInfo = gameCultureInfo ?? new CultureInfo(TaskContext.Instance().Config.OtherConfig.GameCultureInfoName);
        }

        public CultureInfo GameCultureInfo { get; }
    }
}
