using System.Globalization;
using BetterGenshinImpact.GameTask.AutoArtifactSalvage;
using BetterGenshinImpact.GameTask.AutoFishing;
using BetterGenshinImpact.GameTask.AutoTrackPath;
using BetterGenshinImpact.GameTask.Model;
using Microsoft.Extensions.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public class ResourceBackedTaskParamCompatibilityTests
{
    [Fact]
    public void TaskParams_PreserveLegacyBaseTypeAndLocalizerContract()
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var fishingLocalizer = new StubStringLocalizer<AutoFishingTask>();
        var trackPathLocalizer = new StubStringLocalizer<TpTask>();
        var artifactLocalizer = new StubStringLocalizer<AutoArtifactSalvageTask>();

        var fishing = new AutoFishingTaskParam(60, 15, FishingTimePolicy.All, false, culture, fishingLocalizer);
        var trackPath = new TpTaskParam(culture, trackPathLocalizer);
        var artifact = new AutoArtifactSalvageTaskParam(4, null, null, null, null, culture, artifactLocalizer);

        AssertLegacyContract<AutoFishingTask>(fishing, culture, fishingLocalizer);
        AssertLegacyContract<TpTask>(trackPath, culture, trackPathLocalizer);
        AssertLegacyContract<AutoArtifactSalvageTask>(artifact, culture, artifactLocalizer);
    }

    private static void AssertLegacyContract<TTask>(
        object parameter,
        CultureInfo expectedCulture,
        IStringLocalizer<TTask> expectedLocalizer)
        where TTask : class
    {
        Assert.Equal(typeof(BaseTaskParam<TTask>), parameter.GetType().BaseType);
        Assert.True(typeof(BaseTaskParam<TTask>).IsAssignableFrom(parameter.GetType()));
        var legacyParameter = Assert.IsAssignableFrom<BaseTaskParam<TTask>>(parameter);
        Assert.Same(expectedCulture, legacyParameter.GameCultureInfo);
        Assert.Same(expectedLocalizer, legacyParameter.StringLocalizer);
    }

    private sealed class StubStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
