using System;
using Microsoft.Extensions.DependencyInjection;

namespace BetterGenshinImpact.GameTask.Localization;

public static class GameTextServiceCollectionExtensions
{
    public static IServiceCollection AddGameTextLocalization(
        this IServiceCollection services,
        Func<string> configuredCultureName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuredCultureName);

        services.AddSingleton<IGameCultureProvider>(new ConfiguredGameCultureProvider(configuredCultureName));
        services.AddSingleton<IGameTextCatalogProvider, EmbeddedGameTextCatalogProvider>();
        services.AddSingleton<IGameTextMatcher, GameTextMatcher>();

        return services;
    }
}
