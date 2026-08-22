using System.Collections;
using System.Reflection;
using BetterGenshinImpact.GameTask.Localization;
using Newtonsoft.Json.Linq;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public class GameTextCatalogTests
{
    private static readonly string[] RequiredCultures = ["zh-Hans", "zh-Hant", "en", "ja", "fr", "pt-BR"];

    private static readonly IReadOnlyDictionary<string, string> RequiredKeyConstants = new Dictionary<string, string>
    {
        ["Common.Revive"] = "common.revive",
        ["Common.Confirm"] = "common.confirm",
        ["Common.Cancel"] = "common.cancel",
        ["Common.Use"] = "common.use",
        ["Common.ClickAnywhereToClose"] = "common.click_anywhere_to_close",
        ["Common.Crafting"] = "common.crafting",
        ["Common.Claim"] = "common.claim",
        ["Common.All"] = "common.all",
        ["Resin.Original"] = "resin.original",
        ["Resin.Condensed"] = "resin.condensed",
        ["Resin.Fragile"] = "resin.fragile",
        ["Resin.Transient"] = "resin.transient",
        ["Resin.Replenish"] = "resin.replenish",
        ["Resin.Insufficient"] = "resin.insufficient",
        ["Domain.ChallengeCompleted"] = "domain.challenge_completed",
        ["Domain.AutoLeaving"] = "domain.auto_leaving",
        ["Domain.Skip"] = "domain.skip",
        ["Domain.LeyLineDisorder"] = "domain.ley_line_disorder",
        ["Domain.LimitedTimeFullyOpen"] = "domain.limited_time_fully_open",
        ["Domain.SoloChallenge"] = "domain.solo_challenge",
        ["Domain.StartChallenge"] = "domain.start_challenge",
        ["Domain.PetrifiedTree"] = "domain.petrified_tree",
        ["Domain.ResinUsePromptLead"] = "domain.resin_use_prompt_lead",
        ["Domain.ResinUsePromptChallenge"] = "domain.resin_use_prompt_challenge",
        ["Domain.ResinUsePromptDomain"] = "domain.resin_use_prompt_domain",
        ["Boss.FullResinRecovered"] = "boss.full_resin_recovered",
        ["Boss.FullRecovery"] = "boss.full_recovery",
        ["Boss.QuickUse"] = "boss.quick_use",
        ["Boss.UseQuantity"] = "boss.use_quantity",
        ["Boss.Obtain"] = "boss.obtain",
        ["Boss.TouchTrounceBlossom"] = "boss.touch_trounce_blossom",
        ["Boss.ClickBlankAreaToContinue"] = "boss.click_blank_area_to_continue",
        ["Boss.UsePromptLead"] = "boss.use_prompt_lead",
        ["Boss.ReplenishPromptLead"] = "boss.replenish_prompt_lead",
        ["Boss.ReplenishPromptOriginal"] = "boss.replenish_prompt_original",
        ["Boss.ReplenishPromptResin"] = "boss.replenish_prompt_resin",
        ["Expedition.Entry"] = "expedition.entry",
        ["AdventurersGuild.Katheryne"] = "adventurers_guild.katheryne",
        ["AdventurersGuild.DailyCommissions"] = "adventurers_guild.daily_commissions",
        ["AdventureHandbook.DailyRewardClaimed"] = "adventure_handbook.daily_reward_claimed",
        ["SereniteaPot.Spirit"] = "serenitea_pot.spirit",
        ["SereniteaPot.TrustRank"] = "serenitea_pot.trust_rank",
        ["SereniteaPot.RealmDepot"] = "serenitea_pot.realm_depot",
        ["Fishing.Bite"] = "fishing.bite",
        ["Fishing.Action"] = "fishing.action",
        ["WorldArea.SereniteaPot"] = "world_area.serenitea_pot",
        ["WorldArea.NodKrai"] = "world_area.nod_krai",
        ["WorldArea.Snezhnaya"] = "world_area.snezhnaya",
        ["WorldArea.Fontaine"] = "world_area.fontaine",
        ["WorldArea.Liyue"] = "world_area.liyue",
        ["WorldArea.Inazuma"] = "world_area.inazuma",
        ["WorldArea.Natlan"] = "world_area.natlan",
        ["WorldArea.Mondstadt"] = "world_area.mondstadt",
        ["WorldArea.Sumeru"] = "world_area.sumeru",
        ["Artifact.QuickSelect"] = "artifact.quick_select",
        ["Artifact.Star1"] = "artifact.star_1",
        ["Artifact.Star2"] = "artifact.star_2",
        ["Artifact.Star3"] = "artifact.star_3",
        ["Artifact.Star4"] = "artifact.star_4",
        ["Artifact.Atk"] = "artifact.affix.atk",
        ["Artifact.Def"] = "artifact.affix.def",
        ["Artifact.Hp"] = "artifact.affix.hp",
        ["Artifact.CritRate"] = "artifact.affix.crit_rate",
        ["Artifact.CritDmg"] = "artifact.affix.crit_dmg",
        ["Artifact.ElementalMastery"] = "artifact.affix.elemental_mastery",
        ["Artifact.EnergyRecharge"] = "artifact.affix.energy_recharge",
        ["Artifact.HealingBonus"] = "artifact.affix.healing_bonus",
        ["Artifact.PhysicalDmgBonus"] = "artifact.affix.physical_dmg_bonus",
        ["Artifact.PyroDmgBonus"] = "artifact.affix.pyro_dmg_bonus",
        ["Artifact.HydroDmgBonus"] = "artifact.affix.hydro_dmg_bonus",
        ["Artifact.DendroDmgBonus"] = "artifact.affix.dendro_dmg_bonus",
        ["Artifact.ElectroDmgBonus"] = "artifact.affix.electro_dmg_bonus",
        ["Artifact.AnemoDmgBonus"] = "artifact.affix.anemo_dmg_bonus",
        ["Artifact.CryoDmgBonus"] = "artifact.affix.cryo_dmg_bonus",
        ["Artifact.GeoDmgBonus"] = "artifact.affix.geo_dmg_bonus"
    };

    [Fact]
    public void StableKeys_ExposeTheCompleteBaselineContract()
    {
        var keyType = GetStableKeyType();

        foreach (var (path, expectedValue) in RequiredKeyConstants)
        {
            var parts = path.Split('.');
            var groupType = keyType.GetNestedType(parts[0], BindingFlags.Public);
            Assert.NotNull(groupType);
            var field = groupType.GetField(parts[1], BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(field);
            Assert.Equal(expectedValue, field.GetRawConstantValue());
        }

        Assert.Equal(
            RequiredKeyConstants.Values.Order(StringComparer.Ordinal),
            GetStableKeys().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EmbeddedCatalogs_ContainExactlyTheRequiredCultures()
    {
        var provider = new EmbeddedGameTextCatalogProvider();

        Assert.Equal(RequiredCultures.Order(StringComparer.Ordinal), provider.Catalogs.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EmbeddedCatalogs_ContainEveryStableKey()
    {
        var provider = new EmbeddedGameTextCatalogProvider();

        foreach (var culture in RequiredCultures)
        {
            var catalog = provider.Catalogs[culture];
            foreach (var key in GetStableKeys())
            {
                Assert.True(catalog.TryGetAliases(key, out _), $"Catalog '{culture}' is missing key '{key}'.");
            }
        }
    }

    [Fact]
    public void EmbeddedCatalogs_HaveUsableExactlyUniqueRawAliasesForEveryEntry()
    {
        var provider = new EmbeddedGameTextCatalogProvider();

        foreach (var culture in RequiredCultures)
        {
            var catalog = provider.Catalogs[culture];
            foreach (var key in GetStableKeys())
            {
                Assert.True(catalog.TryGetAliases(key, out var aliases));
                Assert.NotEmpty(aliases);

                var normalizedAliases = aliases.Select(GameTextNormalizer.Normalize).ToArray();
                Assert.All(normalizedAliases, alias => Assert.NotEmpty(alias));
                Assert.Equal(aliases.Count, aliases.Distinct(StringComparer.Ordinal).Count());
            }
        }
    }

    [Fact]
    public void Catalog_PreservesDistinctRawAliasesWithEquivalentNormalization()
    {
        var catalog = CreateFrenchCraftingCatalog("Synthèse", "Synthése", "Synthétiser", "Synthètiser");

        Assert.True(catalog.TryGetAliases(GameTextKeys.Common.Crafting, out var aliases));
        Assert.Equal(["Synthèse", "Synthése", "Synthétiser", "Synthètiser"], aliases);
    }

    [Fact]
    public void Catalog_DeduplicatesEquivalentNormalizedAliasesForMatching()
    {
        var catalog = CreateFrenchCraftingCatalog("Synthèse", "Synthése", "Synthétiser", "Synthètiser");

        Assert.True(catalog.TryGetNormalizedAliases(GameTextKeys.Common.Crafting, out var aliases));
        Assert.Equal(["synthese", "synthetiser"], aliases);
    }

    [Fact]
    public void Catalog_RejectsExactRawAliasDuplicates()
    {
        Assert.Throws<ArgumentException>(() => CreateFrenchCraftingCatalog("Synthèse", "Synthèse"));
    }

    [Fact]
    public void SourceManifest_UsesPinnedAnimeGameDataBaselineAndCoversEveryAlias()
    {
        var provider = new EmbeddedGameTextCatalogProvider();
        var manifest = LoadSourceManifest();
        var baseline = Assert.IsType<JObject>(manifest["baseline"]);
        Assert.Equal("https://github.com/DimbreathBot/AnimeGameData.git", baseline.Value<string>("repository"));
        Assert.Equal("26df1dfbdf05a82bbb1d97506859f3e1c40718d8", baseline.Value<string>("commit"));

        var manifestEntries = Assert.IsType<JObject>(manifest["entries"]);
        foreach (var key in GetStableKeys())
        {
            var cultures = Assert.IsType<JObject>(manifestEntries[key]);
            foreach (var culture in RequiredCultures)
            {
                Assert.True(provider.Catalogs[culture].TryGetAliases(key, out var aliases));
                var records = Assert.IsType<JArray>(cultures[culture]);
                Assert.Equal(aliases.Count, records.Count);

                foreach (var alias in aliases)
                {
                    var record = Assert.Single(records.OfType<JObject>(), item => item.Value<string>("value") == alias);
                    var textMapHash = record.Value<string>("textMapHash");
                    var source = record.Value<string>("source");
                    var reason = record.Value<string>("reason");
                    Assert.True(
                        !string.IsNullOrWhiteSpace(textMapHash) ||
                        (source == "curated" && !string.IsNullOrWhiteSpace(reason)),
                        $"Manifest record '{key}'/'{culture}'/'{alias}' needs a TextMap hash or curated reason.");
                }
            }
        }
    }

    [Theory]
    [InlineData("resin.original", "Resina Original")]
    [InlineData("domain.challenge_completed", "Desafio concluído")]
    [InlineData("expedition.entry", "Expedição")]
    [InlineData("common.crafting", "Sintetizar")]
    [InlineData("common.revive", "Reviver")]
    [InlineData("common.claim", "Resgatar")]
    [InlineData("common.all", "Tudo")]
    [InlineData("adventurers_guild.katheryne", "Katheryne")]
    [InlineData("adventurers_guild.daily_commissions", "Missões Diárias")]
    [InlineData("adventure_handbook.daily_reward_claimed", "A recompensa de hoje já foi coletada")]
    [InlineData("adventure_handbook.daily_reward_claimed", "Você já coletou a recompensa de hoje. Volte amanhã para mais missões!")]
    [InlineData("adventure_handbook.daily_reward_claimed", "A recompensa de hoje já foi resgatada")]
    [InlineData("serenitea_pot.spirit", "Espírito do Bule")]
    [InlineData("serenitea_pot.trust_rank", "Nível de confiança")]
    [InlineData("serenitea_pot.realm_depot", "Tesouro do Paraíso Mágico")]
    public void PortugueseCatalog_ContainsRequiredSemanticAlias(string key, string alias)
    {
        var provider = new EmbeddedGameTextCatalogProvider();

        Assert.True(provider.Catalogs["pt-BR"].TryGetAliases(key, out var aliases));
        Assert.Contains(alias, aliases);
    }

    private static IReadOnlySet<string> GetStableKeys()
    {
        var keyType = GetStableKeyType();
        var allProperty = keyType.GetProperty("All", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(allProperty);
        var values = Assert.IsAssignableFrom<IEnumerable>(allProperty.GetValue(null));
        return values.Cast<string>().ToHashSet(StringComparer.Ordinal);
    }

    private static Type GetStableKeyType()
    {
        var keyType = typeof(GameTextCatalog).Assembly.GetType(
            "BetterGenshinImpact.GameTask.Localization.GameTextKeys");
        Assert.NotNull(keyType);
        return keyType;
    }

    private static JObject LoadSourceManifest()
    {
        var manifestPath = Path.Combine(AppContext.BaseDirectory, "Assets", "GameText", "source-manifest.json");
        Assert.True(File.Exists(manifestPath), $"Source manifest was not copied to '{manifestPath}'.");
        return JObject.Parse(File.ReadAllText(manifestPath));
    }

    private static GameTextCatalog CreateFrenchCraftingCatalog(params string[] aliases) =>
        new(
            "fr",
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            {
                [GameTextKeys.Common.Crafting] = aliases
            });
}
