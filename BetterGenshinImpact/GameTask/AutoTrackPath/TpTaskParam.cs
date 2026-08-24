using BetterGenshinImpact.GameTask.Model;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BetterGenshinImpact.GameTask.AutoTrackPath
{
    public class TpTaskParam : BaseTaskParam<TpTask>
    {
        public TpTaskParam(CultureInfo? gameCultureInfo = null, IStringLocalizer<TpTask>? stringLocalizer = null) : base(gameCultureInfo, stringLocalizer)
        {
        }
    }
}
