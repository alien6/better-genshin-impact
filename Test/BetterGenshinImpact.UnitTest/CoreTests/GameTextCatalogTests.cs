using System.Globalization;
using BetterGenshinImpact.Core.Localization;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class GameTextCatalogTests
{
    [Theory]
    [InlineData("zh-Hans", GameTextKey.Confirm, "确认")]
    [InlineData("en-US", GameTextKey.Confirm, "Confirm")]
    [InlineData("pt-BR", GameTextKey.Confirm, "Confirmar")]
    [InlineData("pt-PT", GameTextKey.LeyLineDisorder, "Desordem das Linhas Ley")]
    public void Get_ReturnsCultureSpecificText(string cultureName, string key, string expected)
    {
        var actual = GameTextCatalog.Get(key, new CultureInfo(cultureName));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetAll_ReturnsPortugueseVariantsWithoutChineseFallback()
    {
        var values = GameTextCatalog.GetAll(GameTextKey.ExitDomain, new CultureInfo("pt-BR"));

        Assert.Contains("Sair do Domínio", values);
        Assert.DoesNotContain(values, value => value.Contains('退'));
    }

    [Fact]
    public void Get_UnknownKey_ThrowsDiagnosticException()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() =>
            GameTextCatalog.Get("does_not_exist", new CultureInfo("pt-BR")));

        Assert.Contains("does_not_exist", exception.Message);
        Assert.Contains("pt-BR", exception.Message);
    }
}
