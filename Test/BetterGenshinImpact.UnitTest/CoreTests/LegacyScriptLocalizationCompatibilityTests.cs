using BetterGenshinImpact.Core.Script;
using Microsoft.ClearScript.V8;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class LegacyScriptLocalizationCompatibilityTests
{
    [Fact]
    public void BootstrapScript_ProvidesAdditiveLocalizedStringSearches()
    {
        using var engine = new V8ScriptEngine();
        engine.AddHostObject("genshin", new FakeGenshin());
        LegacyScriptLocalizationCompatibility.Install(engine);

        Assert.True((bool)engine.Evaluate("'Confirmar agora'.includes('确认')"));
        Assert.True((bool)engine.Evaluate("'Confirmar agora'.startsWith('确认')"));
        Assert.True((bool)engine.Evaluate("'Agora Confirmar'.endsWith('确认')"));
        Assert.Equal(6, Convert.ToInt32(engine.Evaluate("'Agora Confirmar'.indexOf('确认')")));
    }

    [Fact]
    public void BootstrapScript_MatchesLocalizedVariantsAfterOcrHelperLowercasesText()
    {
        using var engine = new V8ScriptEngine();
        engine.AddHostObject("genshin", new FakeGenshin());
        LegacyScriptLocalizationCompatibility.Install(engine);

        Assert.True((bool)engine.Evaluate("'continuar'.includes('继续')"));
    }

    [Fact]
    public void BootstrapScript_PreservesNativeAndUnknownSearchBehavior()
    {
        using var engine = new V8ScriptEngine();
        engine.AddHostObject("genshin", new FakeGenshin());
        LegacyScriptLocalizationCompatibility.Install(engine);

        Assert.True((bool)engine.Evaluate("'abcdef'.includes('bcd')"));
        Assert.False((bool)engine.Evaluate("'abcdef'.includes('尚未审计')"));
        Assert.Equal(-1, Convert.ToInt32(engine.Evaluate("'abcdef'.indexOf('尚未审计')")));
    }

    [Fact]
    public void Install_IsIdempotent()
    {
        using var engine = new V8ScriptEngine();
        engine.AddHostObject("genshin", new FakeGenshin());

        LegacyScriptLocalizationCompatibility.Install(engine);
        LegacyScriptLocalizationCompatibility.Install(engine);

        Assert.True((bool)engine.Evaluate("'Confirmar'.includes('确认')"));
    }

    public sealed class FakeGenshin
    {
        public string[] GetLegacyTexts(string canonicalText) => canonicalText switch
        {
            "确认" => ["Confirmar"],
            "继续" => ["Continuar"],
            _ => [canonicalText]
        };
    }
}
