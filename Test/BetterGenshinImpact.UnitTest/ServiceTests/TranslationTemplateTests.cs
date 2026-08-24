using System.Reflection;
using BetterGenshinImpact.Service;

namespace BetterGenshinImpact.UnitTest.ServiceTests;

public sealed class TranslationTemplateTests
{
    [Fact]
    public void TryTranslateTemplate_InsertsRuntimePlaceholderValue()
    {
        var map = new Dictionary<string, string>
        {
            ["刷新曲库失败：{MusicFolder}"] = "Falha ao atualizar a biblioteca de músicas: {MusicFolder}"
        };

        var ok = Invoke("刷新曲库失败：C:\\Music", map, out var translated);

        Assert.True(ok);
        Assert.Equal("Falha ao atualizar a biblioteca de músicas: C:\\Music", translated);
    }

    [Fact]
    public void TryTranslateTemplate_SupportsReorderedPlaceholders()
    {
        var map = new Dictionary<string, string>
        {
            ["从{Source}复制到{Target}"] = "Copiar para {Target} de {Source}"
        };

        var ok = Invoke("从origem复制到destino", map, out var translated);

        Assert.True(ok);
        Assert.Equal("Copiar para destino de origem", translated);
    }

    [Fact]
    public void TryTranslateTemplate_DoesNotMatchDifferentLiteralShape()
    {
        var map = new Dictionary<string, string>
        {
            ["刷新曲库失败：{MusicFolder}"] = "Falha ao atualizar a biblioteca de músicas: {MusicFolder}"
        };

        var ok = Invoke("刷新配置失败：C:\\Music", map, out var translated);

        Assert.False(ok);
        Assert.Equal("刷新配置失败：C:\\Music", translated);
    }

    private static bool Invoke(string text, IReadOnlyDictionary<string, string> map, out string translated)
    {
        var method = typeof(JsonTranslationService).GetMethod(
            "TryTranslateTemplate",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        object?[] args = [text, map, null];
        var result = method!.Invoke(null, args);
        translated = Assert.IsType<string>(args[2]);
        return Assert.IsType<bool>(result);
    }
}
