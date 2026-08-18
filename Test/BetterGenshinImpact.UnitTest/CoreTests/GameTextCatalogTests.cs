using System.Globalization;
using BetterGenshinImpact.Core.Localization;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class GameTextCatalogTests
{
    [Theory]
    [InlineData("zh-Hans", GameTextKey.Confirm, "确认")]
    [InlineData("en-US", GameTextKey.Confirm, "Confirm")]
    [InlineData("pt-BR", GameTextKey.Confirm, "Confirmar")]
    [InlineData("pt-BR", GameTextKey.Ok, "OK")]
    [InlineData("pt-BR", GameTextKey.OriginalResin, "Resina Original")]
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
        Assert.DoesNotContain(values, value => ContainsCjk(value));
    }

    [Fact]
    public void AllSemanticKeys_HavePortugueseValuesWithoutCjk()
    {
        var culture = new CultureInfo("pt-BR");

        foreach (var key in GameTextCatalog.Keys)
        {
            var values = GameTextCatalog.GetAll(key, culture);
            Assert.NotEmpty(values);
            Assert.All(values, value =>
            {
                Assert.False(string.IsNullOrWhiteSpace(value), $"Empty pt-BR value for '{key}'.");
                Assert.False(ContainsCjk(value), $"CJK leaked into pt-BR value for '{key}': {value}");
            });
        }
    }

    [Fact]
    public void Get_UnknownKey_ThrowsDiagnosticException()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() =>
            GameTextCatalog.Get("does_not_exist", new CultureInfo("pt-BR")));

        Assert.Contains("does_not_exist", exception.Message);
        Assert.Contains("pt-BR", exception.Message);
    }

    private static bool ContainsCjk(string value)
    {
        return value.Any(character => character is >= '\u3400' and <= '\u4DBF' or >= '\u4E00' and <= '\u9FFF');
    }
}
