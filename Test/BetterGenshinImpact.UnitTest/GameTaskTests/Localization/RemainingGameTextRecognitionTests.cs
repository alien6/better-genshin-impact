using BetterGenshinImpact.GameTask.Localization;
using Xunit;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.Localization;

public sealed class RemainingGameTextRecognitionTests
{
    [Fact]
    public void PickupSuppression_PreservesCompoundRulesAndRecognizesPortugueseLabels()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.ShouldSuppressPickup("Oficina de Bolo Crocante Clinc-Clanc"));
        Assert.True(recognizer.ShouldSuppressPickup("Estou com Reputação da tribo"));
        Assert.False(recognizer.ShouldSuppressPickup("Estou com uma oficina"));
    }

    [Fact]
    public void TalentInfo_ClassifiesPortugueseTypesAndSeparatesBonusNumberParsing()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsTalentIntroduction("Informações do Talento"));
        Assert.True(recognizer.IsMatch("Atributos", GameTextKeys.Character.Attribute));
        Assert.Equal("Talentos", recognizer.GetPrimaryAlias(GameTextKeys.Character.Talent));
        Assert.Equal(GameTextKeys.Character.NormalAttack, recognizer.GetTalentTypeKey("Ataque Normal"));
        Assert.Equal(GameTextKeys.Character.ElementalSkill, recognizer.GetTalentTypeKey("Habilidade Elemental"));
        Assert.Equal(GameTextKeys.Character.ElementalBurst, recognizer.GetTalentTypeKey("Supremo"));
        Assert.True(recognizer.HasTalentBonus("Nível de Talento + 3"));
        Assert.False(recognizer.HasTalentBonus("Nível de Talento + 2"));
    }

    [Fact]
    public void ArtifactSetAndObtainedWoodText_RecognizePortugueseWhileKeepingRawNames()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsArtifactSetContains("O conjunto inclui"));
        Assert.True(recognizer.IsObtained("Obtido\nMadeira de Pinheiro × 20"));
        Assert.True(recognizer.IsKnownWoodName("Madeira de Pinheiro"));
        Assert.False(recognizer.IsKnownWoodName("20"));
    }

    [Fact]
    public void SplashPrompt_RecognizesPortugueseAgeAndGuardianFragments()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsAgePrompt("Classificação indicativa"));
        Assert.True(recognizer.IsAgePrompt("Orientação dos responsáveis"));
    }

    [Theory]
    [InlineData("Minério de Refinamento")]
    [InlineData("Minério de Refinamento Fino")]
    [InlineData("Minério de Refinamento Místico")]
    public void EnhancementOreNames_AreSemanticProductNames(string itemName)
    {
        Assert.True(CreatePortugueseRecognizer().IsEnhancementOreName(itemName));
    }

    [Fact]
    public void Activate_UsesTheExistingSemanticKey()
    {
        Assert.True(CreatePortugueseRecognizer().IsActivate("Ativar"));
    }

    [Fact]
    public void CompanionshipExp_RecognizesPortugueseUnavailableMessage()
    {
        Assert.True(CreatePortugueseRecognizer().IsCompanionshipExpUnavailable(
            "Não é possível resgatar EXP de Amizade"));
        Assert.True(CreatePortugueseRecognizer().IsMatch("Esgotado", GameTextKeys.SereniteaPot.SoldOut));
        Assert.True(CreatePortugueseRecognizer().IsMatch("Adeus", GameTextKeys.Common.Goodbye));
    }

    [Fact]
    public void MusicAlbumAndDomainUse_RecognizeExistingPortugueseCommonLabels()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsMatch("Tudo", GameTextKeys.Common.All));
        Assert.True(recognizer.IsMatch("Usar", GameTextKeys.Common.Use));
    }

    [Fact]
    public void SimplifiedChineseCompatibility_CoversEveryMigratedBranch()
    {
        var recognizer = CreateSimplifiedChineseRecognizer();

        Assert.True(recognizer.ShouldSuppressPickup("叮铃哐啷蛋卷工坊"));
        Assert.True(recognizer.ShouldSuppressPickup("我在悬木人声望"));
        Assert.True(recognizer.IsArtifactSetContains("套装包含"));
        Assert.True(recognizer.IsTalentIntroduction("天赋介绍"));
        Assert.Equal(GameTextKeys.Character.NormalAttack, recognizer.GetTalentTypeKey("普通攻击"));
        Assert.True(recognizer.HasTalentBonus("天赋等级 + 3"));
        Assert.True(recognizer.IsObtained("获得\n杉木×20"));
        Assert.True(recognizer.IsKnownWoodName("杉木"));
        Assert.True(recognizer.IsAgePrompt("适龄提示"));
        Assert.True(recognizer.IsEnhancementOreName("精锻用魔矿"));
        Assert.True(recognizer.IsActivate("激活"));
        Assert.True(recognizer.IsCompanionshipExpUnavailable("无法领取好感经验"));
    }

    private static RemainingGameTextRecognizer CreatePortugueseRecognizer() =>
        CreateRecognizer(
            "pt-BR",
            suppressed: "Clinc-Clanc",
            tribeLead: "Estou com",
            tribeMarker: "Reputação",
            frostmoon: "Lua Gelada",
            workshop: "Oficina",
            eggRoll: "Bolo Crocante",
            setContains: "O conjunto inclui",
            talentIntroduction: "Informações do Talento",
            normalAttack: "Ataque Normal",
            elementalSkill: "Habilidade Elemental",
            elementalBurst: "Supremo",
            talentLevel: "Nível de Talento",
            obtained: "Obtido",
            agePrompt: "classificação etária|responsável",
            ore: "Minério de Refinamento|Minério de Refinamento Fino|Minério de Refinamento Místico",
            wood: "Madeira de Pinheiro",
            activate: "Ativar",
            companionshipUnavailable: "Não é possível resgatar EXP de Amizade");

    private static RemainingGameTextRecognizer CreateSimplifiedChineseRecognizer() =>
        CreateRecognizer(
            "zh-Hans",
            suppressed: "长时间|聚所|叮铃|眶螂|西风成垒|望崖营壁|魔女的花园|月谕圣牌",
            tribeLead: "我在",
            tribeMarker: "声望|回声|悬木人|流泉",
            frostmoon: "霜月",
            workshop: "坊",
            eggRoll: "蛋卷",
            setContains: "套装包含",
            talentIntroduction: "天赋介绍",
            normalAttack: "普通攻击",
            elementalSkill: "元素战技",
            elementalBurst: "元素爆发",
            talentLevel: "天赋等级",
            obtained: "获得",
            agePrompt: "适龄|监护",
            ore: "精锻用杂矿|精锻用良矿|精锻用魔矿",
            wood: "杉木",
            activate: "激活",
            companionshipUnavailable: "无法领取好感经验");

    private static RemainingGameTextRecognizer CreateRecognizer(
        string culture,
        string suppressed,
        string tribeLead,
        string tribeMarker,
        string frostmoon,
        string workshop,
        string eggRoll,
        string setContains,
        string talentIntroduction,
        string normalAttack,
        string elementalSkill,
        string elementalBurst,
        string talentLevel,
        string obtained,
        string agePrompt,
        string ore,
        string wood,
        string activate,
        string companionshipUnavailable)
    {
        var entries = new[]
        {
            (GameTextKeys.AutoPick.SuppressedLabel, suppressed),
            (GameTextKeys.AutoPick.TribeLead, tribeLead),
            (GameTextKeys.AutoPick.TribeMarker, tribeMarker),
            (GameTextKeys.AutoPick.Frostmoon, frostmoon),
            (GameTextKeys.AutoPick.Workshop, workshop),
            (GameTextKeys.AutoPick.EggRoll, eggRoll),
            (GameTextKeys.Artifact.SetContains, setContains),
            (GameTextKeys.Character.TalentIntroduction, talentIntroduction),
            (GameTextKeys.Character.Attribute, culture == "pt-BR" ? "Atributos" : "属性"),
            (GameTextKeys.Character.Weapon, culture == "pt-BR" ? "Arma" : "武器"),
            (GameTextKeys.Character.Talent, culture == "pt-BR" ? "Talentos" : "天赋"),
            (GameTextKeys.Character.NormalAttack, normalAttack),
            (GameTextKeys.Character.ElementalSkill, elementalSkill),
            (GameTextKeys.Character.ElementalBurst, elementalBurst),
            (GameTextKeys.Character.TalentLevel, talentLevel),
            (GameTextKeys.Common.Obtained, obtained),
            (GameTextKeys.GameLoading.AgePrompt, agePrompt),
            (GameTextKeys.Inventory.EnhancementOre, ore),
            (GameTextKeys.Wood.Material, wood),
            (GameTextKeys.LeyLine.Activate, activate),
            (GameTextKeys.SereniteaPot.CompanionshipExpUnavailable, companionshipUnavailable),
            (GameTextKeys.SereniteaPot.SoldOut, culture == "pt-BR" ? "Esgotado" : "已售"),
            (GameTextKeys.Common.Goodbye, culture == "pt-BR" ? "Adeus" : "再见"),
            (GameTextKeys.Common.All, culture == "pt-BR" ? "Tudo" : "全部"),
            (GameTextKeys.Common.Use, culture == "pt-BR" ? "Usar" : "使用")
        };
        var matcher = GameTextTestFactory.Create(
            culture,
            entries.SelectMany(entry => entry.Item2.Split('|').Select(alias => (entry.Item1, alias))).ToArray());

        return new RemainingGameTextRecognizer(matcher);
    }
}
