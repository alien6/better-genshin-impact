using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public class ConfiguredGameCultureProviderTests
{
    [Fact]
    public void CurrentCulture_ReadsTheLatestConfiguredCulture()
    {
        var configured = "pt-BR";
        var sut = new ConfiguredGameCultureProvider(() => configured);

        Assert.Equal("pt-BR", sut.CurrentCulture.Name);

        configured = "en-US";

        Assert.Equal("en-US", sut.CurrentCulture.Name);
    }

    [Fact]
    public void CurrentCulture_InvalidConfiguredCulture_ThrowsDescriptiveInvalidOperationException()
    {
        const string configured = "not a game culture";
        var sut = new ConfiguredGameCultureProvider(() => configured);

        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.CurrentCulture;
        });

        Assert.Contains(configured, error.Message);
    }
}
