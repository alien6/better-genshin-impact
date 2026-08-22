using BetterGenshinImpact.GameTask.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public class GameTextServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGameTextLocalization_RegistersMatcherAndCatalogProviderAsSingletons()
    {
        var configured = "pt-BR";
        var services = new ServiceCollection();
        services.AddGameTextLocalization(() => configured);
        using var serviceProvider = services.BuildServiceProvider();

        var firstMatcher = serviceProvider.GetRequiredService<IGameTextMatcher>();
        var secondMatcher = serviceProvider.GetRequiredService<IGameTextMatcher>();
        var firstCatalogProvider = serviceProvider.GetRequiredService<IGameTextCatalogProvider>();
        var secondCatalogProvider = serviceProvider.GetRequiredService<IGameTextCatalogProvider>();

        Assert.Same(firstMatcher, secondMatcher);
        Assert.Same(firstCatalogProvider, secondCatalogProvider);
    }
}
