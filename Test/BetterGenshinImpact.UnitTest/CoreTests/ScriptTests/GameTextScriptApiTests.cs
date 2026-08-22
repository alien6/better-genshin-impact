using System.Collections;
using System.Globalization;
using BetterGenshinImpact.Core.Script;
using BetterGenshinImpact.Core.Script.Dependence;
using BetterGenshinImpact.GameTask.Localization;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace BetterGenshinImpact.UnitTest.CoreTests.ScriptTests;

public class GameTextScriptApiTests
{
    [Fact]
    public void Api_ExposesAliasesSemanticMatchesAndCulture()
    {
        var (matcher, cultureProvider) = CreateMatcher();
        var sut = new GameTextScriptApi(matcher, cultureProvider);

        Assert.Equal(["Resina Original"], sut.aliases("resin.original"));
        Assert.True(sut.matches("RESINA ORIGINAL", "resin.original"));
        Assert.True(sut.matchesAny(new[] { "Vida", "Resina Original" }, "resin.original"));
        Assert.Equal("pt-BR", sut.culture);
    }

    [Fact]
    public void MatchesAny_RejectsInvalidCollections()
    {
        var (matcher, cultureProvider) = CreateMatcher();
        var sut = new GameTextScriptApi(matcher, cultureProvider);

        Assert.Throws<ArgumentException>((Action)(() => sut.matchesAny("Resina Original", "resin.original")));
        Assert.Throws<ArgumentException>((Action)(() => sut.matchesAny(Array.Empty<string>(), "resin.original")));
        Assert.Throws<ArgumentException>((Action)(() => sut.matchesAny(new ArrayList { "Resina Original", 1 }, "resin.original")));
    }

    [Fact]
    public void V8Bridge_ExposesOnlyTheGameTextSurface()
    {
        var (matcher, cultureProvider) = CreateMatcher();
        using var engine = new V8ScriptEngine();
        EngineExtend.AddGameTextHostObject(engine, matcher, cultureProvider);

        Assert.Equal("Resina Original", engine.Evaluate("gameText.aliases('resin.original')[0]"));
        Assert.Equal(true, engine.Evaluate("gameText.matches('RESINA ORIGINAL', 'resin.original')"));
        Assert.Equal(true, engine.Evaluate("gameText.matchesAny(['Vida', 'Resina Original'], 'resin.original')"));
        Assert.Equal("pt-BR", engine.Evaluate("gameText.culture"));
        Assert.Throws<ScriptEngineException>((Action)(() => engine.Evaluate("gameText.matchesAny([], 'resin.original')")));
        Assert.Throws<ScriptEngineException>((Action)(() => engine.Evaluate("'use strict'; gameText.culture = 'en-US'")));
        Assert.Equal("undefined", engine.Evaluate("typeof keyMouseScript"));
    }

    private static (GameTextMatcher Matcher, IGameCultureProvider CultureProvider) CreateMatcher()
    {
        var cultureProvider = new TestGameCultureProvider(CultureInfo.GetCultureInfo("pt-BR"));
        var catalog = new GameTextCatalog(
            "pt-BR",
            new Dictionary<string, IReadOnlyList<string>>
            {
                ["resin.original"] = ["Resina Original"]
            });
        var matcher = new GameTextMatcher(
            cultureProvider,
            new TestGameTextCatalogProvider(new Dictionary<string, GameTextCatalog>
            {
                [catalog.CultureName] = catalog
            }));
        return (matcher, cultureProvider);
    }

    private sealed class TestGameCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }

    private sealed class TestGameTextCatalogProvider(IReadOnlyDictionary<string, GameTextCatalog> catalogs)
        : IGameTextCatalogProvider
    {
        public IReadOnlyDictionary<string, GameTextCatalog> Catalogs { get; } = catalogs;
    }
}
