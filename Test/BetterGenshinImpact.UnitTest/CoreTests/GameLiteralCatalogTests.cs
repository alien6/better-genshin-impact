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
    [InlineData("点击", "Pressione")]
    [InlineData("箱", "Baú")]
    [InlineData("珍贵", "Precioso")]
    [InlineData("珍貴", "Precioso")]
    [InlineData("委託", "Comissão")]
    [InlineData("初始手牌", "Mão Inicial")]
    [InlineData("重投骰子", "Rolar Novamente")]
    [InlineData("出战角色", "Personagem em Combate")]
    [InlineData("对局胜利", "Vitória")]
    [InlineData("对局失败", "Derrota")]
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
