using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
        public const string Obtained = "common.obtained";
        public const string Goodbye = "common.goodbye";
        public const string Clear = "common.clear";
        public const string Filter = "common.filter";
        public const string Enter = "common.enter";
        public const string Leave = "common.leave";
        public const string Paste = "common.paste";
    }

    public static class AutoPick
    {
        public const string SuppressedLabel = "auto_pick.suppressed_label";
        public const string TribeLead = "auto_pick.tribe_lead";
        public const string TribeMarker = "auto_pick.tribe_marker";
        public const string Frostmoon = "auto_pick.frostmoon";
        public const string Workshop = "auto_pick.workshop";
        public const string EggRoll = "auto_pick.egg_roll";
    }

    public static class Character
    {
        public const string TalentIntroduction = "character.talent_introduction";
        public const string NormalAttack = "character.normal_attack";
        public const string ElementalSkill = "character.elemental_skill";
        public const string ElementalBurst = "character.elemental_burst";
        public const string TalentLevel = "character.talent_level";
        public const string Attribute = "character.attribute";
        public const string Weapon = "character.weapon";
        public const string Talent = "character.talent";
    }

    public static class Party
    {
        public const string ConfirmFilter = "party.confirm_filter";
        public const string ConfigurationUnavailable = "party.configuration_unavailable";
        public const string ElementalResonance = "party.elemental_resonance";
        public const string Configuration = "party.configuration";
        public const string Remove = "party.remove";
        public const string Friendship = "party.friendship";
        public const string Replace = "party.replace";
        public const string Join = "party.join";
        public const string Order = "party.order";
    }

    public static class Redemption
    {
        public const string Account = "redemption.account";
        public const string GoToRedeem = "redemption.go_to_redeem";
        public const string RedeemReward = "redemption.redeem_reward";
        public const string Success = "redemption.success";
    }

    public static class GameLoading
    {
        public const string AgePrompt = "game_loading.age_prompt";
    }

    public static class Inventory
    {
        public const string EnhancementOre = "inventory.enhancement_ore";
    }

    public static class Wood
    {
        public const string Material = "wood.material";
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

    public static class Boss
    {
        public const string FullResinRecovered = "boss.full_resin_recovered";
        public const string FullRecovery = "boss.full_recovery";
        public const string QuickUse = "boss.quick_use";
        public const string UseQuantity = "boss.use_quantity";
        public const string Obtain = "boss.obtain";
        public const string TouchTrounceBlossom = "boss.touch_trounce_blossom";
        public const string ClickBlankAreaToContinue = "boss.click_blank_area_to_continue";
        public const string UsePromptLead = "boss.use_prompt_lead";
        public const string ReplenishPromptLead = "boss.replenish_prompt_lead";
        public const string ReplenishPromptOriginal = "boss.replenish_prompt_original";
        public const string ReplenishPromptResin = "boss.replenish_prompt_resin";
    }

    public static class LeyLine
    {
        public const string DoubleReward = "ley_line.double_reward";
        public const string DoubleReward2x = "ley_line.double_reward_2x";
        public const string Touch = "ley_line.touch";
        public const string Activate = "ley_line.activate";
        public const string Select = "ley_line.select";
        public const string Line = "ley_line.line";
        public const string Outcrop = "ley_line.outcrop";
        public const string BlossomOfWealth = "ley_line.blossom_of_wealth";
        public const string BlossomOfRevelation = "ley_line.blossom_of_revelation";
        public const string Stop = "ley_line.stop";
        public const string OriginalResin40Prompt = "ley_line.original_resin_40_prompt";
        public const string FightSuccess = "ley_line.fight_success";
        public const string FightFailure = "ley_line.fight_failure";
        public const string FightObjective = "ley_line.fight_objective";
    }

    public static class Stygian
    {
        public const string Return = "stygian.return";
        public const string ChallengeFailed = "stygian.challenge_failed";
        public const string RetryChallenge = "stygian.retry_challenge";
        public const string LeyLineBlossom = "stygian.ley_line_blossom";
        public const string CharacterPreview = "stygian.character_preview";
        public const string EventName = "stygian.event_name";
        public const string EventOverview = "stygian.event_overview";
        public const string RewardPhase = "stygian.reward_phase";
        public const string PhaseEnded = "stygian.phase_ended";
        public const string GoToChallenge = "stygian.go_to_challenge";
        public const string HardDifficulty = "stygian.hard_difficulty";
        public const string UltimateChallenge = "stygian.ultimate_challenge";
        public const string NormalChallenge = "stygian.normal_challenge";
        public const string PresetTeams = "stygian.preset_teams";
    }

    public static class Expedition
    {
        public const string Entry = "expedition.entry";
        public const string TimeShortened = "expedition.time_shortened";
        public const string RewardsIncreased = "expedition.rewards_increased";
        public const string NoBonus = "expedition.no_bonus";
        public const string Complete = "expedition.complete";
        public const string InProgress = "expedition.in_progress";
        public const string Rewards = "expedition.rewards";
        public const string SelectCharacter = "expedition.select_character";
        public const string CharacterSelection = "expedition.character_selection";
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
        public const string CompanionshipExpUnavailable = "serenitea_pot.companionship_exp_unavailable";
        public const string SoldOut = "serenitea_pot.sold_out";
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
        public const string SetContains = "artifact.set_contains";
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
        Common.Obtained,
        Common.Goodbye,
        Common.Clear,
        Common.Filter,
        Common.Enter,
        Common.Leave,
        Common.Paste,
        AutoPick.SuppressedLabel,
        AutoPick.TribeLead,
        AutoPick.TribeMarker,
        AutoPick.Frostmoon,
        AutoPick.Workshop,
        AutoPick.EggRoll,
        Character.TalentIntroduction,
        Character.NormalAttack,
        Character.ElementalSkill,
        Character.ElementalBurst,
        Character.TalentLevel,
        Character.Attribute,
        Character.Weapon,
        Character.Talent,
        Party.ConfirmFilter,
        Party.ConfigurationUnavailable,
        Party.ElementalResonance,
        Party.Configuration,
        Party.Remove,
        Party.Friendship,
        Party.Replace,
        Party.Join,
        Party.Order,
        Redemption.Account,
        Redemption.GoToRedeem,
        Redemption.RedeemReward,
        Redemption.Success,
        GameLoading.AgePrompt,
        Inventory.EnhancementOre,
        Wood.Material,
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
        Boss.FullResinRecovered,
        Boss.FullRecovery,
        Boss.QuickUse,
        Boss.UseQuantity,
        Boss.Obtain,
        Boss.TouchTrounceBlossom,
        Boss.ClickBlankAreaToContinue,
        Boss.UsePromptLead,
        Boss.ReplenishPromptLead,
        Boss.ReplenishPromptOriginal,
        Boss.ReplenishPromptResin,
        LeyLine.DoubleReward,
        LeyLine.DoubleReward2x,
        LeyLine.Touch,
        LeyLine.Activate,
        LeyLine.Select,
        LeyLine.Line,
        LeyLine.Outcrop,
        LeyLine.BlossomOfWealth,
        LeyLine.BlossomOfRevelation,
        LeyLine.Stop,
        LeyLine.OriginalResin40Prompt,
        LeyLine.FightSuccess,
        LeyLine.FightFailure,
        LeyLine.FightObjective,
        Stygian.Return,
        Stygian.ChallengeFailed,
        Stygian.RetryChallenge,
        Stygian.LeyLineBlossom,
        Stygian.CharacterPreview,
        Stygian.EventName,
        Stygian.EventOverview,
        Stygian.RewardPhase,
        Stygian.PhaseEnded,
        Stygian.GoToChallenge,
        Stygian.HardDifficulty,
        Stygian.UltimateChallenge,
        Stygian.NormalChallenge,
        Stygian.PresetTeams,
        Expedition.Entry,
        Expedition.TimeShortened,
        Expedition.RewardsIncreased,
        Expedition.NoBonus,
        Expedition.Complete,
        Expedition.InProgress,
        Expedition.Rewards,
        Expedition.SelectCharacter,
        Expedition.CharacterSelection,
        AdventurersGuild.Katheryne,
        AdventurersGuild.DailyCommissions,
        AdventureHandbook.DailyRewardClaimed,
        SereniteaPot.Spirit,
        SereniteaPot.TrustRank,
        SereniteaPot.RealmDepot,
        SereniteaPot.CompanionshipExpUnavailable,
        SereniteaPot.SoldOut,
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
        Artifact.GeoDmgBonus,
        Artifact.SetContains
    }.ToFrozenSet(StringComparer.Ordinal);
}

public sealed class RemainingGameTextRecognizer
{
    private static readonly string[] Keys =
    [
        GameTextKeys.Common.Use,
        GameTextKeys.Common.All,
        GameTextKeys.Common.Obtained,
        GameTextKeys.Common.Clear,
        GameTextKeys.Common.Filter,
        GameTextKeys.Common.Enter,
        GameTextKeys.Common.Leave,
        GameTextKeys.Common.Paste,
        GameTextKeys.Common.Crafting,
        GameTextKeys.Common.Confirm,
        GameTextKeys.Common.Claim,
        GameTextKeys.Party.ConfirmFilter,
        GameTextKeys.Party.ConfigurationUnavailable,
        GameTextKeys.Party.ElementalResonance,
        GameTextKeys.Party.Configuration,
        GameTextKeys.Party.Remove,
        GameTextKeys.Party.Friendship,
        GameTextKeys.Party.Replace,
        GameTextKeys.Party.Join,
        GameTextKeys.Party.Order,
        GameTextKeys.Redemption.Account,
        GameTextKeys.Redemption.GoToRedeem,
        GameTextKeys.Redemption.RedeemReward,
        GameTextKeys.Redemption.Success,
        GameTextKeys.Expedition.SelectCharacter,
        GameTextKeys.Expedition.CharacterSelection,
        GameTextKeys.WorldArea.SereniteaPot,
        GameTextKeys.LeyLine.FightSuccess,
        GameTextKeys.LeyLine.FightFailure,
        GameTextKeys.LeyLine.FightObjective,
        GameTextKeys.AutoPick.SuppressedLabel,
        GameTextKeys.AutoPick.TribeLead,
        GameTextKeys.AutoPick.TribeMarker,
        GameTextKeys.AutoPick.Frostmoon,
        GameTextKeys.AutoPick.Workshop,
        GameTextKeys.AutoPick.EggRoll,
        GameTextKeys.Character.TalentIntroduction,
        GameTextKeys.Character.NormalAttack,
        GameTextKeys.Character.ElementalSkill,
        GameTextKeys.Character.ElementalBurst,
        GameTextKeys.Character.TalentLevel,
        GameTextKeys.Character.Attribute,
        GameTextKeys.Character.Weapon,
        GameTextKeys.Character.Talent,
        GameTextKeys.GameLoading.AgePrompt,
        GameTextKeys.Inventory.EnhancementOre,
        GameTextKeys.Wood.Material,
        GameTextKeys.LeyLine.Activate,
        GameTextKeys.SereniteaPot.CompanionshipExpUnavailable,
        GameTextKeys.SereniteaPot.SoldOut,
        GameTextKeys.Common.Goodbye,
        GameTextKeys.Artifact.SetContains,
    ];

    private static readonly Regex TalentBonusNumberRegex =
        new(@"[+＋]\s*3(?:\D|$)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly string[] TalentTypeKeys =
    [
        GameTextKeys.Character.NormalAttack,
        GameTextKeys.Character.ElementalSkill,
        GameTextKeys.Character.ElementalBurst,
    ];

    private readonly FrozenDictionary<string, IReadOnlyList<string>> _rawAliases;
    private readonly FrozenDictionary<string, IReadOnlyList<string>> _aliases;

    public RemainingGameTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(matcher);
        _rawAliases = Keys.ToFrozenDictionary(
            key => key,
            key => (IReadOnlyList<string>)matcher.GetAliases(key, culture)
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Distinct(StringComparer.Ordinal)
                .ToArray()
                .AsReadOnly(),
            StringComparer.Ordinal);
        _aliases = _rawAliases.ToFrozenDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<string>)pair.Value
                .Select(GameTextNormalizer.Normalize)
                .Where(alias => alias.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
                .AsReadOnly(),
            StringComparer.Ordinal);
    }

    public string NormalizeOcrText(string? text) => GameTextNormalizer.Normalize(text);

    public bool ShouldSuppressPickup(string recognizedText)
    {
        var normalizedText = NormalizeOcrText(recognizedText);
        return IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.SuppressedLabel)
               || IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.TribeLead)
               && IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.TribeMarker)
               || IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.Frostmoon)
               && IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.Workshop)
               || IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.EggRoll)
               && IsMatchNormalized(normalizedText, GameTextKeys.AutoPick.Workshop);
    }

    public bool IsArtifactSetContains(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.Artifact.SetContains);

    public bool IsTalentIntroduction(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.Character.TalentIntroduction);

    public string GetTalentTypeKey(string recognizedText)
    {
        var normalizedText = NormalizeOcrText(recognizedText);
        foreach (var key in TalentTypeKeys)
        {
            if (IsMatchNormalized(normalizedText, key))
            {
                return key;
            }
        }

        return string.Empty;
    }

    public bool HasTalentBonus(string recognizedText)
    {
        var normalizedText = NormalizeOcrText(recognizedText);
        return IsMatchNormalized(normalizedText, GameTextKeys.Character.TalentLevel)
               && TalentBonusNumberRegex.IsMatch(recognizedText);
    }

    public bool IsObtained(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.Common.Obtained);

    public bool IsKnownWoodName(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.Wood.Material);

    public bool IsAgePrompt(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.GameLoading.AgePrompt);

    public bool IsEnhancementOreName(string itemName) =>
        IsMatchNormalized(NormalizeOcrText(itemName), GameTextKeys.Inventory.EnhancementOre);

    public bool IsActivate(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.LeyLine.Activate);

    public bool IsCompanionshipExpUnavailable(string recognizedText) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), GameTextKeys.SereniteaPot.CompanionshipExpUnavailable);

    public string GetPrimaryAlias(string key) => _rawAliases[key][0];

    public bool IsMatch(string recognizedText, string key) =>
        IsMatchNormalized(NormalizeOcrText(recognizedText), key);

    private bool IsMatchNormalized(string normalizedText, string key) =>
        normalizedText.Length > 0
        && _aliases[key].Any(alias => normalizedText.Contains(alias, StringComparison.Ordinal));
}
