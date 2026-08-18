using System.Globalization;
using BetterGenshinImpact.Core.Localization;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class LegacyScriptTextResolverTests
{
    [Theory]
    [InlineData("确认", "Confirmar")]
    [InlineData("委托", "Comissão")]
    [InlineData("委託", "Comissão")]
    [InlineData("原粹树脂", "Resina Original")]
    [InlineData("箱", "Baú")]
    [InlineData("挂起来吧", "Pendure-o.")]
    public void Get_ResolvesSemanticAndAuditedLegacyTextToPortuguese(string canonical, string expected)
    {
        Assert.Equal(expected, LegacyScriptTextResolver.Get(canonical, new CultureInfo("pt-BR")));
    }

    [Fact]
    public void GetAll_PreservesPortugueseVariants()
    {
        var values = LegacyScriptTextResolver.GetAll("设置", new CultureInfo("pt-BR"));

        Assert.Contains("Ajustes", values);
        Assert.Contains("Configurações", values);
    }

    [Fact]
    public void UnknownText_IsPreservedVerbatim()
    {
        const string canonical = "尚未审计的旧脚本文本";

        Assert.Equal(canonical, LegacyScriptTextResolver.Get(canonical, new CultureInfo("pt-BR")));
    }

    [Fact]
    public void ExistingChineseCulture_RemainsChinese()
    {
        Assert.Equal("确认", LegacyScriptTextResolver.Get("确认", new CultureInfo("zh-Hans")));
        Assert.Equal("委託", LegacyScriptTextResolver.Get("委託", new CultureInfo("zh-TW")));
    }
}
