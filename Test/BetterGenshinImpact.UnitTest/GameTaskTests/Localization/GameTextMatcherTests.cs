using System.Globalization;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public class GameTextMatcherTests
{
    [Fact]
    public void IsMatch_NormalizesPortugueseOcrNoise()
    {
        var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));

        Assert.True(sut.IsMatch("  RESINA\nORIGINAL! ", "resin.original"));
    }

    [Fact]
    public void IsAnyMatch_MatchesOneOcrRegion()
    {
        var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));

        Assert.True(sut.IsAnyMatch(["Vida", "Resina Original"], "resin.original"));
    }

    [Fact]
    public void IsCombinedMatch_MatchesWordsSplitAcrossRegions()
    {
        var sut = GameTextTestFactory.Create("pt-BR", ("domain.challenge_completed", "Desafio concluído"));

        Assert.True(sut.IsCombinedMatch(["Desafio", "concluído"], "domain.challenge_completed"));
    }

    [Fact]
    public void EmptyOcr_ReturnsFalse()
    {
        var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));

        Assert.False(sut.IsMatch("", "resin.original"));
        Assert.False(sut.IsAnyMatch(["", "  "], "resin.original"));
        Assert.False(sut.IsCombinedMatch(["", "  "], "resin.original"));
    }

    [Fact]
    public void UnknownKey_ThrowsKeyNotFoundException()
    {
        var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));

        var error = Assert.Throws<KeyNotFoundException>(() => sut.GetAliases("missing.key"));

        Assert.Contains("missing.key", error.Message);
        Assert.Contains("pt-BR", error.Message);
    }

    [Fact]
    public void MissingCulture_ThrowsInvalidOperationException()
    {
        var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));

        Assert.Throws<InvalidOperationException>(() => sut.GetAliases("resin.original", new CultureInfo("de-DE")));
    }

    [Fact]
    public void SpecificCulture_FallsBackOnlyToNeutralParent()
    {
        var sut = GameTextTestFactory.CreateForCultures(
            "pt-BR",
            ("pt", "resin.original", "Resina"),
            ("en", "resin.original", "Original Resin"));

        Assert.True(sut.IsMatch("Resina", "resin.original", new CultureInfo("pt-PT")));
        Assert.False(sut.IsMatch("Original Resin", "resin.original", new CultureInfo("pt-PT")));
    }

    [Fact]
    public void Matcher_DoesNotChangeThreadCultures()
    {
        var beforeCulture = CultureInfo.CurrentCulture;
        var beforeUiCulture = CultureInfo.CurrentUICulture;
        var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));

        _ = sut.IsMatch("Resina Original", "resin.original");

        Assert.Same(beforeCulture, CultureInfo.CurrentCulture);
        Assert.Same(beforeUiCulture, CultureInfo.CurrentUICulture);
    }
}
