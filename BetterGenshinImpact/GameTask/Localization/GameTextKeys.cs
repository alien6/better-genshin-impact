using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace BetterGenshinImpact.GameTask.Localization;

public static class GameTextKeys
{
    public static class Common
    {
        public const string Revive = "common.revive";
        public const string Confirm = "common.confirm";
        public const string Cancel = "common.cancel";
        public const string Use = "common.use";
        public const string ClickAnywhereToClose = "common.click_anywhere_to_close";
        public const string Crafting = "common.crafting";
        public const string Claim = "common.claim";
        public const string All = "common.all";
    }

    public static class Resin
    {
        public const string Original = "resin.original";
        public const string Condensed = "resin.condensed";
        public const string Fragile = "resin.fragile";
        public const string Transient = "resin.transient";
        public const string Replenish = "resin.replenish";
        public const string Insufficient = "resin.insufficient";
    }

    public static class Domain
    {
        public const string ChallengeCompleted = "domain.challenge_completed";
    }

    public static class Expedition
    {
        public const string Entry = "expedition.entry";
    }

    public static class AdventurersGuild
    {
        public const string Katheryne = "adventurers_guild.katheryne";
        public const string DailyCommissions = "adventurers_guild.daily_commissions";
    }

    public static class AdventureHandbook
    {
        public const string DailyRewardClaimed = "adventure_handbook.daily_reward_claimed";
    }

    public static class SereniteaPot
    {
        public const string Spirit = "serenitea_pot.spirit";
        public const string TrustRank = "serenitea_pot.trust_rank";
        public const string RealmDepot = "serenitea_pot.realm_depot";
    }

    public static IReadOnlySet<string> All { get; } = new[]
    {
        Common.Revive,
        Common.Confirm,
        Common.Cancel,
        Common.Use,
        Common.ClickAnywhereToClose,
        Common.Crafting,
        Common.Claim,
        Common.All,
        Resin.Original,
        Resin.Condensed,
        Resin.Fragile,
        Resin.Transient,
        Resin.Replenish,
        Resin.Insufficient,
        Domain.ChallengeCompleted,
        Expedition.Entry,
        AdventurersGuild.Katheryne,
        AdventurersGuild.DailyCommissions,
        AdventureHandbook.DailyRewardClaimed,
        SereniteaPot.Spirit,
        SereniteaPot.TrustRank,
        SereniteaPot.RealmDepot
    }.ToFrozenSet(StringComparer.Ordinal);
}
