using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.AutoArtifactSalvage;

public sealed class ArtifactTextRecognizer
{
    private static readonly FrozenDictionary<ArtifactAffixType, string> AffixKeys =
        new Dictionary<ArtifactAffixType, string>
        {
            [ArtifactAffixType.ATK] = GameTextKeys.Artifact.Atk,
            [ArtifactAffixType.DEF] = GameTextKeys.Artifact.Def,
            [ArtifactAffixType.HP] = GameTextKeys.Artifact.Hp,
            [ArtifactAffixType.CRITRate] = GameTextKeys.Artifact.CritRate,
            [ArtifactAffixType.CRITDMG] = GameTextKeys.Artifact.CritDmg,
            [ArtifactAffixType.ElementalMastery] = GameTextKeys.Artifact.ElementalMastery,
            [ArtifactAffixType.EnergyRecharge] = GameTextKeys.Artifact.EnergyRecharge,
            [ArtifactAffixType.HealingBonus] = GameTextKeys.Artifact.HealingBonus,
            [ArtifactAffixType.PhysicalDMGBonus] = GameTextKeys.Artifact.PhysicalDmgBonus,
            [ArtifactAffixType.PyroDMGBonus] = GameTextKeys.Artifact.PyroDmgBonus,
            [ArtifactAffixType.HydroDMGBonus] = GameTextKeys.Artifact.HydroDmgBonus,
            [ArtifactAffixType.DendroDMGBonus] = GameTextKeys.Artifact.DendroDmgBonus,
            [ArtifactAffixType.ElectroDMGBonus] = GameTextKeys.Artifact.ElectroDmgBonus,
            [ArtifactAffixType.AnemoDMGBonus] = GameTextKeys.Artifact.AnemoDmgBonus,
            [ArtifactAffixType.CryoDMGBonus] = GameTextKeys.Artifact.CryoDmgBonus,
            [ArtifactAffixType.GeoDMGBonus] = GameTextKeys.Artifact.GeoDmgBonus
        }.ToFrozenDictionary();

    private readonly IGameTextMatcher _matcher;
    private readonly CultureInfo? _culture;

    public ArtifactTextRecognizer(IGameTextMatcher matcher, CultureInfo? culture = null)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        _culture = culture;
    }

    public bool IsQuickSelect(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Artifact.QuickSelect, _culture);

    public bool IsStarLabel(string recognizedText, int star) =>
        _matcher.IsMatch(recognizedText, GetStarKey(star), _culture);

    public bool TryGetAffixType(string recognizedText, out ArtifactAffixType affixType)
    {
        foreach (var (candidateType, key) in AffixKeys)
        {
            if (_matcher.IsMatch(recognizedText, key, _culture))
            {
                affixType = candidateType;
                return true;
            }
        }

        affixType = default;
        return false;
    }

    private static string GetStarKey(int star) => star switch
    {
        1 => GameTextKeys.Artifact.Star1,
        2 => GameTextKeys.Artifact.Star2,
        3 => GameTextKeys.Artifact.Star3,
        4 => GameTextKeys.Artifact.Star4,
        _ => throw new ArgumentOutOfRangeException(nameof(star), star, "Artifact star must be from 1 through 4.")
    };
}
