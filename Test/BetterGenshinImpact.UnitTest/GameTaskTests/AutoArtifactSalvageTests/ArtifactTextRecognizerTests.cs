using System.Globalization;
using BetterGenshinImpact.GameTask.AutoArtifactSalvage;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoArtifactSalvageTests;

public class ArtifactTextRecognizerTests
{
    [Theory]
    [InlineData("Escolha rápida", true)]
    [InlineData("Seleção rápida", false)]
    [InlineData("Selecionar", false)]
    public void IsQuickSelect_MatchesPortugueseOcrText(string recognizedText, bool expected)
    {
        var sut = Create("pt-BR");

        Assert.Equal(expected, sut.IsQuickSelect(recognizedText));
    }

    [Theory]
    [InlineData("Artefatos de 1 estrela", 1, true)]
    [InlineData("Artefatos de 4 estrelas", 4, true)]
    [InlineData("Artefatos de 4 estrelas", 3, false)]
    public void IsStarLabel_MatchesPortugueseStarText(string recognizedText, int star, bool expected)
    {
        var sut = Create("pt-BR");

        Assert.Equal(expected, sut.IsStarLabel(recognizedText, star));
    }

    [Theory]
    [InlineData("VIDA", ArtifactAffixType.HP)]
    [InlineData("Proficiência Elemental", ArtifactAffixType.ElementalMastery)]
    [InlineData("Bônus de Dano Electro", ArtifactAffixType.ElectroDMGBonus)]
    [InlineData("Dano Crítico", ArtifactAffixType.CRITDMG)]
    public void TryGetAffixType_MapsPortugueseMainStat(
        string recognizedText,
        ArtifactAffixType expected)
    {
        var sut = Create("pt-BR");

        Assert.True(sut.TryGetAffixType(recognizedText, out var actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(LegacyArtifactResourceValues))]
    public void Catalog_PreservesEveryLegacyArtifactResourceValue(
        string culture,
        string key,
        string legacyValue)
    {
        var matcher = CreateMatcher(culture);

        Assert.Contains(legacyValue, matcher.GetAliases(key));
    }

    public static IEnumerable<object[]> LegacyArtifactResourceValues()
    {
        var rows = new (string Key, string ZhHans, string ZhHant, string En, string Fr)[]
        {
            ("artifact.quick_select", "快速选择", "快速選擇", "Quick.*Select", "Sélection.*rapide"),
            ("artifact.star_1", "1星圣遗物", "一星聖遺物", "1-Star.*Artifacts", "Artéfact.*1★?"),
            ("artifact.star_2", "2星圣遗物", "二星聖遺物", "2-Star.*Artifacts", "Artéfact.*2★?"),
            ("artifact.star_3", "3星圣遗物", "三星聖遺物", "3-Star.*Artifacts", "Artéfact.*3★?"),
            ("artifact.star_4", "4星圣遗物", "四星聖遺物", "4-Star.*Artifacts", "Artéfact.*4★?"),
            ("artifact.affix.energy_recharge", "元素充能效率", "元素充能效率", "Energy Recharge", "Recharge d'énergie"),
            ("artifact.affix.elemental_mastery", "元素精通", "元素精通", "Elemental Mastery", "Maîtrise élémentaire"),
            ("artifact.affix.cryo_dmg_bonus", "冰元素伤害加成", "冰元素傷害加成", "Cryo DMG Bonus", "Bonus de DGT Cryo"),
            ("artifact.affix.geo_dmg_bonus", "岩元素伤害加成", "岩元素傷害加成", "Geo DMG Bonus", "Bonus de DGT Géo"),
            ("artifact.affix.atk", "攻击力", "攻擊力", "ATK", "ATQ"),
            ("artifact.affix.crit_dmg", "暴击伤害", "暴擊傷害", "CRIT DMG", "DGT CRIT"),
            ("artifact.affix.crit_rate", "暴击率", "暴擊率", "CRIT Rate", "Taux CRIT"),
            ("artifact.affix.hydro_dmg_bonus", "水元素伤害加成", "水元素傷害加成", "Hydro DMG Bonus", "Bonus de DGT Hydro"),
            ("artifact.affix.healing_bonus", "治疗加成", "治療加成", "Healing Bonus", "Bonus de soins"),
            ("artifact.affix.pyro_dmg_bonus", "火元素伤害加成", "火元素傷害加成", "Pyro DMG Bonus", "Bonus de DGT Pyro"),
            ("artifact.affix.physical_dmg_bonus", "物理伤害加成", "物理傷害加成", "Physical DMG Bonus", "Bonus de DGT physiques"),
            ("artifact.affix.hp", "生命值", "生命值", "HP", "PV"),
            ("artifact.affix.dendro_dmg_bonus", "草元素伤害加成", "草元素傷害加成", "Dendro DMG Bonus", "Bonus de DGT Dendro"),
            ("artifact.affix.def", "防御力", "防禦力", "DEF", "DÉF"),
            ("artifact.affix.electro_dmg_bonus", "雷元素伤害加成", "雷元素傷害加成", "Electro DMG Bonus", "Bonus de DGT Électro"),
            ("artifact.affix.anemo_dmg_bonus", "风元素伤害加成", "風元素傷害加成", "Anemo DMG Bonus", "Bonus de DGT Anémo")
        };

        foreach (var row in rows)
        {
            yield return ["zh-Hans", row.Key, row.ZhHans];
            yield return ["zh-Hant", row.Key, row.ZhHant];
            yield return ["en", row.Key, row.En];
            yield return ["fr", row.Key, row.Fr];
        }
    }

    private static ArtifactTextRecognizer Create(string culture) => new(CreateMatcher(culture));

    private static GameTextMatcher CreateMatcher(string culture) =>
        new(new FixedCultureProvider(CultureInfo.GetCultureInfo(culture)), new EmbeddedGameTextCatalogProvider());

    private sealed class FixedCultureProvider(CultureInfo currentCulture) : IGameCultureProvider
    {
        public CultureInfo CurrentCulture { get; } = currentCulture;
    }
}
