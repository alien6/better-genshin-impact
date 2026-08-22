using BetterGenshinImpact.GameTask.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BetterGenshinImpact.GameTask.AutoArtifactSalvage
{
    public class AutoArtifactSalvageTaskParam
    {
        public AutoArtifactSalvageTaskParam(int star, string? javaScript, string? artifactSetFilter, int? maxNumToCheck, RecognitionFailurePolicy? recognitionFailurePolicy, CultureInfo? cultureInfo = null, IStringLocalizer<AutoArtifactSalvageTask>? stringLocalizer = null)
        {
            Star = star;
            JavaScript = javaScript;
            ArtifactSetFilter = artifactSetFilter;
            MaxNumToCheck = maxNumToCheck;
            RecognitionFailurePolicy = recognitionFailurePolicy;
            GameCultureInfo = cultureInfo ?? new CultureInfo(TaskContext.Instance().Config.OtherConfig.GameCultureInfoName);
        }

        public int Star { get; set; }
        public string? JavaScript { get; set; }
        public string? ArtifactSetFilter { get; set; }
        public int? MaxNumToCheck { get; set; }
        public RecognitionFailurePolicy? RecognitionFailurePolicy { get; set; }
        public CultureInfo GameCultureInfo { get; }
        public IGameTextMatcher? GameTextMatcher { get; init; }
    }
}
