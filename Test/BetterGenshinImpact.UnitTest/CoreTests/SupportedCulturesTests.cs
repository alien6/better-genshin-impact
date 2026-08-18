using BetterGenshinImpact.Core.Localization;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class SupportedCulturesTests
{
    [Fact]
    public void Names_ContainsBrazilianPortuguese()
    {
        Assert.Contains("pt-BR", SupportedCultures.Names);
    }

    [Fact]
    public void Names_PreservesExistingCultures()
    {
        Assert.Contains("zh-Hans", SupportedCultures.Names);
        Assert.Contains("zh-Hant", SupportedCultures.Names);
        Assert.Contains("en", SupportedCultures.Names);
        Assert.Contains("ja", SupportedCultures.Names);
    }
}
