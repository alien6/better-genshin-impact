using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public class GameTextNormalizerTests
{
    [Theory]
    [InlineData("Resina Original", "resinaoriginal")]
    [InlineData("  EXPEDIÇÃO! ", "expedicao")]
    [InlineData("Síntese", "sintese")]
    [InlineData("２× Recompensa", "2recompensa")]
    [InlineData("地脉 异常", "地脉异常")]
    [InlineData("", "")]
    public void Normalize_ReturnsStableComparableText(string input, string expected)
    {
        Assert.Equal(expected, GameTextNormalizer.Normalize(input));
    }
}
