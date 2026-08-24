using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.GameTask.Model;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BetterGenshinImpact.GameTask.AutoArtifactSalvage
{
    public class AutoArtifactSalvageTaskParam : BaseTaskParam<AutoArtifactSalvageTask>
    {
        public AutoArtifactSalvageTaskParam(int star, string? javaScript, string? artifactSetFilter, int? maxNumToCheck, RecognitionFailurePolicy? recognitionFailurePolicy, CultureInfo? cultureInfo = null, IStringLocalizer<AutoArtifactSalvageTask>? stringLocalizer = null) : base(cultureInfo, stringLocalizer)
        {
            Star = star;
            JavaScript = javaScript;
            ArtifactSetFilter = artifactSetFilter;
            MaxNumToCheck = maxNumToCheck;
            RecognitionFailurePolicy = recognitionFailurePolicy;
        }

        public int Star { get; set; }
        public string? JavaScript { get; set; }
        public string? ArtifactSetFilter { get; set; }
        public int? MaxNumToCheck { get; set; }
        public RecognitionFailurePolicy? RecognitionFailurePolicy { get; set; }
        public IGameTextMatcher? GameTextMatcher { get; init; }
    }
}
