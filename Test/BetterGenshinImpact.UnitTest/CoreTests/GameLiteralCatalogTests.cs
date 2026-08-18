using System.Globalization;
using BetterGenshinImpact.Core.Localization;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class GameLiteralCatalogTests
{
    [Theory]
    [InlineData("原石", "Gema Essencial")]
    [InlineData("尘歌壶", "Bule de Relachá")]
    [InlineData("浓缩树脂", "Resina Condensada")]
    [InlineData("按键", "Comandos")]
    [InlineData("按鍵", "Comandos")]
    [InlineData("分钟", "m")]
    public void Get_ReturnsAuditedPortuguese(string canonicalZh, string expected)
    {
        Assert.Equal(expected, GameLiteralCatalog.Get(canonicalZh, new CultureInfo("pt-BR")));
    }

    [Fact]
    public void GetAll_PreservesAuditedVariants()
    {
        var values = GameLiteralCatalog.GetAll("设置", new CultureInfo("pt-BR"));
        Assert.Contains("Ajustes", values);
        Assert.Contains("Configurações", values);
    }

    [Fact]
    public void MissingPortugueseLiteral_ThrowsInsteadOfGuessing()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() =>
            GameLiteralCatalog.Get("不存在的文本", new CultureInfo("pt-BR")));
        Assert.Contains("no audited PT-BR mapping", exception.Message);
    }

    [Fact]
    public void NonPortugueseCulture_PreservesHistoricalChineseLiteral()
    {
        Assert.Equal("原石", GameLiteralCatalog.Get("原石", new CultureInfo("ja")));
    }
}
