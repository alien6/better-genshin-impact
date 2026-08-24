using System.Collections;
using System.Globalization;
using BetterGenshinImpact.Core.BgiVision;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

namespace BetterGenshinImpact.UnitTest.CoreTests.BgiVisionTests;

public class BvGameTextTests
{
    [Fact]
    public void GetByTextKey_SnapshotsAliasesAndMatchesNormalizedOcr()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Resin.Original, "Resina Original"));
        var page = new BvPage(gameTextMatcher: matcher);

        var locator = page.GetByTextKey(GameTextKeys.Resin.Original);

        Assert.Equal(GameTextKeys.Resin.Original, locator.GameTextKey);
        Assert.Equal([GameTextKeys.Resin.Original], locator.GameTextKeys);
        Assert.Contains("Resina Original", locator.AnyTexts);
        Assert.True(locator.MatchesOcrText("RESINA-ORIGINAL"));
        Assert.Contains("resin.original", locator.DescribeTarget());
        Assert.Contains("Resina Original", locator.DescribeTarget());
    }

    [Fact]
    public void GetByTextKey_MatchesNormalizedNoiseAgainstASecondaryAlias()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Redemption.Success, "Resgate realizado com sucesso"),
            (GameTextKeys.Redemption.Success, "Código resgatado"));

        var locator = new BvPage(gameTextMatcher: matcher)
            .GetByTextKey(GameTextKeys.Redemption.Success);

        Assert.True(locator.MatchesOcrText("CÓDIGO-resgatado!"));
        Assert.False(locator.MatchesOcrText("resgate indisponível"));
    }

    [Fact]
    public void Clone_PreservesSemanticMetadataAndMatching()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Resin.Original, "Resina Original"));
        var original = new BvPage(gameTextMatcher: matcher)
            .GetByTextKey(GameTextKeys.Resin.Original);

        var clone = original.Clone();

        Assert.Equal(original.GameTextKeys, clone.GameTextKeys);
        Assert.Equal(original.AnyTexts, clone.AnyTexts);
        Assert.Equal(original.NormalizedAnyTexts, clone.NormalizedAnyTexts);
        Assert.True(clone.MatchesOcrText("resina_original"));
    }

    [Fact]
    public void GetByAnyTextKey_DeduplicatesKeysAndAliasesInFirstSeenOrder()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Resin.Original, "Resina"),
            (GameTextKeys.Resin.Original, "Original"),
            (GameTextKeys.Resin.Condensed, "Resina"),
            (GameTextKeys.Resin.Condensed, "Condensada"));
        var page = new BvPage(gameTextMatcher: matcher);

        var locator = page.GetByAnyTextKey(new ArrayList
        {
            GameTextKeys.Resin.Original,
            GameTextKeys.Resin.Original,
            GameTextKeys.Resin.Condensed
        });

        Assert.Equal(
            [GameTextKeys.Resin.Original, GameTextKeys.Resin.Condensed],
            locator.GameTextKeys);
        Assert.Equal(["Resina", "Original", "Condensada"], locator.AnyTexts);
        Assert.True(locator.MatchesOcrText("RESINA-CONDENSADA"));
    }

    [Fact]
    public void SemanticLocator_CopiesAliasesInsteadOfReadingMatcherAgain()
    {
        var matcher = new MutableAliasMatcher("Resina Original");
        var locator = new BvPage(gameTextMatcher: matcher).GetByTextKey(GameTextKeys.Resin.Original);

        matcher.Aliases[0] = "Alterada";

        Assert.Equal(1, matcher.GetAliasesCallCount);
        Assert.Equal(["Resina Original"], locator.AnyTexts);
        Assert.True(locator.MatchesOcrText("RESINA-ORIGINAL"));
        Assert.False(locator.MatchesOcrText("Alterada"));
    }

    [Fact]
    public void SemanticLocator_WithNoAliasesDoesNotFallBackToMatchAllOcr()
    {
        var locator = new BvPage(gameTextMatcher: new MutableAliasMatcher())
            .GetByTextKey(GameTextKeys.Resin.Original);

        Assert.False(locator.MatchesOcrText("unrelated text"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void SemanticLocator_NormalizedEmptyAliasDoesNotMatchArbitraryOcr(string alias)
    {
        var locator = new BvPage(gameTextMatcher: new MutableAliasMatcher(alias))
            .GetByTextKey(GameTextKeys.Resin.Original);

        Assert.Empty(locator.NormalizedAnyTexts);
        Assert.False(locator.MatchesOcrText("unrelated text"));
    }

    [Fact]
    public void SemanticLocatorCreation_RejectsUnknownKeysImmediately()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Resin.Original, "Resina Original"));
        var page = new BvPage(gameTextMatcher: matcher);

        Assert.Throws<KeyNotFoundException>(() => page.GetByTextKey("missing.key"));
        Assert.Throws<KeyNotFoundException>(() => page.GetByAnyTextKey(new[]
        {
            GameTextKeys.Resin.Original,
            "missing.key"
        }));
    }

    [Fact]
    public void GetByAnyTextKey_UsesExistingCollectionValidation()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Resin.Original, "Resina Original"));
        var page = new BvPage(gameTextMatcher: matcher);

        Assert.Throws<ArgumentException>(() => page.GetByAnyTextKey(GameTextKeys.Resin.Original));
        Assert.Throws<ArgumentException>(() => page.GetByAnyTextKey(Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => page.GetByAnyTextKey(new object[] { GameTextKeys.Resin.Original, 1 }));
    }

    [Fact]
    public void SemanticWaitMethods_ConstructFlowStepsFromFlowAndAction()
    {
        var matcher = GameTextTestFactory.Create(
            "pt-BR",
            (GameTextKeys.Resin.Original, "Resina Original"),
            (GameTextKeys.Resin.Condensed, "Resina Condensada"));
        var page = new BvPage(gameTextMatcher: matcher);
        var directFlow = page.Flow();

        Assert.Same(directFlow, directFlow.WaitUntilTextKey(GameTextKeys.Resin.Original));
        Assert.Same(directFlow, directFlow.WaitUntilAnyTextKey(new[]
        {
            GameTextKeys.Resin.Original,
            GameTextKeys.Resin.Condensed
        }));

        var actionTextFlow = page.Flow();
        Assert.Same(
            actionTextFlow,
            actionTextFlow.Do((Action)(() => { })).WaitUntilTextKey(GameTextKeys.Resin.Original));

        var actionAnyFlow = page.Flow();
        Assert.Same(
            actionAnyFlow,
            actionAnyFlow.Do((Action)(() => { })).WaitUntilAnyTextKey(new[]
            {
                GameTextKeys.Resin.Original,
                GameTextKeys.Resin.Condensed
            }));
    }

    [Fact]
    public void LiteralApis_RemainOrdinalAndDoNotResolveSemanticMatcher()
    {
        var page = new BvPage(gameTextMatcher: new ThrowingMatcher());
        var single = page.GetByText("Resina Original");
        var any = page.GetByAnyText(new[] { "Resina Original", "Resina Condensada" });

        Assert.True(single.MatchesOcrText("xResina Originalx"));
        Assert.False(single.MatchesOcrText("RESINA-ORIGINAL"));
        Assert.True(any.MatchesOcrText("xResina Condensaday"));
        Assert.False(any.MatchesOcrText("RESINA-CONDENSADA"));
        Assert.Null(single.GameTextKey);
        Assert.Empty(any.GameTextKeys);
    }

    private sealed class MutableAliasMatcher(params string[] aliases) : IGameTextMatcher
    {
        public List<string> Aliases { get; } = [.. aliases];

        public int GetAliasesCallCount { get; private set; }

        public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null)
        {
            GetAliasesCallCount++;
            return Aliases;
        }

        public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null) =>
            throw new NotSupportedException();

        public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) =>
            throw new NotSupportedException();

        public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) =>
            throw new NotSupportedException();
    }

    private sealed class ThrowingMatcher : IGameTextMatcher
    {
        public IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null) =>
            throw new InvalidOperationException("Literal APIs must not resolve semantic aliases.");

        public bool IsMatch(string recognizedText, string key, CultureInfo? culture = null) =>
            throw new InvalidOperationException("Literal APIs must not use semantic matching.");

        public bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) =>
            throw new InvalidOperationException("Literal APIs must not use semantic matching.");

        public bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null) =>
            throw new InvalidOperationException("Literal APIs must not use semantic matching.");
    }
}
