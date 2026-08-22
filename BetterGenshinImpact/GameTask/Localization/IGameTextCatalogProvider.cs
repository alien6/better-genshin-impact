using System.Collections.Generic;

namespace BetterGenshinImpact.GameTask.Localization;

public interface IGameTextCatalogProvider
{
    IReadOnlyDictionary<string, GameTextCatalog> Catalogs { get; }
}
