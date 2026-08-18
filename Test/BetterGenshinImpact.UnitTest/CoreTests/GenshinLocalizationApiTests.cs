using BetterGenshinImpact.Core.Script.Dependence;
using BetterGenshinImpact.GameTask.Model.Area;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class GenshinLocalizationApiTests
{
    [Theory]
    [InlineData("GameCulture")]
    [InlineData("GetText")]
    [InlineData("GetTexts")]
    [InlineData("FindTextKey")]
    [InlineData("FindTextKeyAndClick")]
    public void LocalizationApi_IsExposedByGenshin(string memberName)
    {
        var type = typeof(Genshin);
        var exists = type.GetProperty(memberName) != null || type.GetMethods().Any(method => method.Name == memberName);

        Assert.True(exists, $"Genshin must expose {memberName} to the JavaScript host.");
    }

    [Fact]
    public void FindTextKey_AcceptsImageRegion()
    {
        var method = typeof(Genshin).GetMethod("FindTextKey", [typeof(string), typeof(ImageRegion)]);

        Assert.NotNull(method);
    }
}
