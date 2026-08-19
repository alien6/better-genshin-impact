using BetterGenshinImpact.GameTask.Model.Area;
using ScriptGenshin = BetterGenshinImpact.Core.Script.Dependence.Genshin;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class GenshinLocalizationApiTests
{
    [Theory]
    [InlineData("GameCulture")]
    [InlineData("GetText")]
    [InlineData("GetTexts")]
    [InlineData("GetTextLiteral")]
    [InlineData("GetTextLiterals")]
    [InlineData("GetLegacyText")]
    [InlineData("GetLegacyTexts")]
    [InlineData("TextContainsLiteral")]
    [InlineData("TextEqualsLiteral")]
    [InlineData("TextStartsWithLiteral")]
    [InlineData("TextEndsWithLiteral")]
    [InlineData("FindTextKey")]
    [InlineData("HasTextKey")]
    [InlineData("FindTextKeyText")]
    [InlineData("FindTextKeyAndClick")]
    public void LocalizationApi_IsExposedByGenshin(string memberName)
    {
        var type = typeof(ScriptGenshin);
        var exists = type.GetProperty(memberName) != null || type.GetMethods().Any(method => method.Name == memberName);

        Assert.True(exists, $"Genshin must expose {memberName} to the JavaScript host.");
    }

    [Fact]
    public void FindTextKey_AcceptsImageRegion()
    {
        var method = typeof(ScriptGenshin).GetMethod("FindTextKey", [typeof(string), typeof(ImageRegion)]);
        Assert.NotNull(method);
    }
}
