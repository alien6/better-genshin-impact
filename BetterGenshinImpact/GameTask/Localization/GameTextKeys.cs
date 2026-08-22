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
        public const string AutoLeaving = "domain.auto_leaving";
        public const string Skip = "domain.skip";
        public const string LeyLineDisorder = "domain.ley_line_disorder";
        public const string LimitedTimeFullyOpen = "domain.limited_time_fully_open";
        public const string SoloChallenge = "domain.solo_challenge";
        public const string StartChallenge = "domain.start_challenge";
        public const string PetrifiedTree = "domain.petrified_tree";
        public const string ResinUsePromptLead = "domain.resin_use_prompt_lead";
        public const string ResinUsePromptChallenge = "domain.resin_use_prompt_challenge";
        public const string ResinUsePromptDomain = "domain.resin_use_prompt_domain";
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

    public static class Fishing
    {
        public const string Bite = "fishing.bite";
        public const string Action = "fishing.action";
    }

    public static class WorldArea
    {
        public const string SereniteaPot = "world_area.serenitea_pot";
        public const string NodKrai = "world_area.nod_krai";
        public const string Snezhnaya = "world_area.snezhnaya";
        public const string Fontaine = "world_area.fontaine";
        public const string Liyue = "world_area.liyue";
        public const string Inazuma = "world_area.inazuma";
        public const string Natlan = "world_area.natlan";
        public const string Mondstadt = "world_area.mondstadt";
        public const string Sumeru = "world_area.sumeru";
    }

    public static class Artifact
    {
        public const string QuickSelect = "artifact.quick_select";
        public const string Star1 = "artifact.star_1";
        public const string Star2 = "artifact.star_2";
        public const string Star3 = "artifact.star_3";
        public const string Star4 = "artifact.star_4";
        public const string Atk = "artifact.affix.atk";
        public const string Def = "artifact.affix.def";
        public const string Hp = "artifact.affix.hp";
        public const string CritRate = "artifact.affix.crit_rate";
        public const string CritDmg = "artifact.affix.crit_dmg";
        public const string ElementalMastery = "artifact.affix.elemental_mastery";
        public const string EnergyRecharge = "artifact.affix.energy_recharge";
        public const string HealingBonus = "artifact.affix.healing_bonus";
        public const string PhysicalDmgBonus = "artifact.affix.physical_dmg_bonus";
        public const string PyroDmgBonus = "artifact.affix.pyro_dmg_bonus";
        public const string HydroDmgBonus = "artifact.affix.hydro_dmg_bonus";
        public const string DendroDmgBonus = "artifact.affix.dendro_dmg_bonus";
        public const string ElectroDmgBonus = "artifact.affix.electro_dmg_bonus";
        public const string AnemoDmgBonus = "artifact.affix.anemo_dmg_bonus";
        public const string CryoDmgBonus = "artifact.affix.cryo_dmg_bonus";
        public const string GeoDmgBonus = "artifact.affix.geo_dmg_bonus";
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
        Domain.AutoLeaving,
        Domain.Skip,
        Domain.LeyLineDisorder,
        Domain.LimitedTimeFullyOpen,
        Domain.SoloChallenge,
        Domain.StartChallenge,
        Domain.PetrifiedTree,
        Domain.ResinUsePromptLead,
        Domain.ResinUsePromptChallenge,
        Domain.ResinUsePromptDomain,
        Expedition.Entry,
        AdventurersGuild.Katheryne,
        AdventurersGuild.DailyCommissions,
        AdventureHandbook.DailyRewardClaimed,
        SereniteaPot.Spirit,
        SereniteaPot.TrustRank,
        SereniteaPot.RealmDepot,
        Fishing.Bite,
        Fishing.Action,
        WorldArea.SereniteaPot,
        WorldArea.NodKrai,
        WorldArea.Snezhnaya,
        WorldArea.Fontaine,
        WorldArea.Liyue,
        WorldArea.Inazuma,
        WorldArea.Natlan,
        WorldArea.Mondstadt,
        WorldArea.Sumeru,
        Artifact.QuickSelect,
        Artifact.Star1,
        Artifact.Star2,
        Artifact.Star3,
        Artifact.Star4,
        Artifact.Atk,
        Artifact.Def,
        Artifact.Hp,
        Artifact.CritRate,
        Artifact.CritDmg,
        Artifact.ElementalMastery,
        Artifact.EnergyRecharge,
        Artifact.HealingBonus,
        Artifact.PhysicalDmgBonus,
        Artifact.PyroDmgBonus,
        Artifact.HydroDmgBonus,
        Artifact.DendroDmgBonus,
        Artifact.ElectroDmgBonus,
        Artifact.AnemoDmgBonus,
        Artifact.CryoDmgBonus,
        Artifact.GeoDmgBonus
    }.ToFrozenSet(StringComparer.Ordinal);
}
