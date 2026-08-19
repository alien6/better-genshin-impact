using System.Globalization;
using BetterGenshinImpact.Core.Recognition.OCR.Paddle;

namespace BetterGenshinImpact.UnitTest.CoreTests;

public class PaddleOcrCultureTests
{
    [Fact]
    public void FromCultureInfo_PtBr_UsesV5Latin()
    {
        var culture = new CultureInfo("pt-BR");

        var model = PaddleOcrService.PaddleOcrModelType.FromCultureInfo(culture);

        Assert.Same(PaddleOcrService.PaddleOcrModelType.V5Latin, model);
    }
}
