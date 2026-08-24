using BetterGenshinImpact.ViewModel.Pages;

namespace BetterGenshinImpact.UnitTest.ViewModelTests;

public class CommonSettingsPageViewModelTests
{
    [Fact]
    public void ResolveMissingRemoteLanguage_WhenInstalledTranslationExists_KeepsInstalledTranslation()
    {
        var result = CommonSettingsPageViewModel.ResolveMissingRemoteLanguage(installedTranslationExists: true);

        Assert.Equal(UiLanguageMissingRemoteResolution.KeepInstalledTranslation, result);
    }

    [Fact]
    public void ResolveMissingRemoteLanguage_WhenInstalledTranslationIsAbsent_ReportsMissingTranslation()
    {
        var result = CommonSettingsPageViewModel.ResolveMissingRemoteLanguage(installedTranslationExists: false);

        Assert.Equal(UiLanguageMissingRemoteResolution.ReportMissingTranslation, result);
    }
}
