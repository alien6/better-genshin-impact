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

        Assert.True(recognizer.IsAgePrompt("Classificação etária"));
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
    public void HiddenHelperApiLabels_RecognizePortugueseCharacterCraftingAndPartyUi()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsMatch("Limpar", GameTextKeys.Common.Clear));
        Assert.True(recognizer.IsMatch("Filtrar", GameTextKeys.Common.Filter));
        Assert.True(recognizer.IsMatch("Sintetizar", GameTextKeys.Common.Crafting));
        Assert.True(recognizer.IsMatch("Confirmar", GameTextKeys.Common.Confirm));
        Assert.True(recognizer.IsMatch("Confirmar Filtro", GameTextKeys.Party.ConfirmFilter));
        Assert.True(recognizer.IsMatch("Não é possível configurar a equipe no estado atual", GameTextKeys.Party.ConfigurationUnavailable));
        Assert.True(recognizer.IsMatch("Ressonância Elemental", GameTextKeys.Party.ElementalResonance));
        Assert.True(recognizer.IsMatch("Configuração da Equipe", GameTextKeys.Party.Configuration));
        Assert.True(recognizer.IsMatch("Remover", GameTextKeys.Party.Remove));
        Assert.True(recognizer.IsMatch("Amizade", GameTextKeys.Party.Friendship));
        Assert.Equal("Limpar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Clear));
        Assert.Equal("Filtrar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Filter));
        Assert.Equal("Sintetizar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Crafting));
        Assert.Equal("Confirmar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Confirm));
        Assert.Equal("Confirmar Filtro", recognizer.GetPrimaryAlias(GameTextKeys.Party.ConfirmFilter));
        Assert.Equal("Não é possível configurar a equipe no estado atual", recognizer.GetPrimaryAlias(GameTextKeys.Party.ConfigurationUnavailable));
        Assert.Equal("Ressonância Elemental", recognizer.GetPrimaryAlias(GameTextKeys.Party.ElementalResonance));
        Assert.Equal("Configuração da Equipe", recognizer.GetPrimaryAlias(GameTextKeys.Party.Configuration));
        Assert.Equal("Remover", recognizer.GetPrimaryAlias(GameTextKeys.Party.Remove));
        Assert.Equal("Amizade", recognizer.GetPrimaryAlias(GameTextKeys.Party.Friendship));
    }

    [Fact]
    public void HiddenHelperApiLabels_RecognizePortugueseExpeditionTeapotAndRedemptionUi()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsMatch("Resgatar", GameTextKeys.Common.Claim));
        Assert.True(recognizer.IsMatch("Selecionar Personagem", GameTextKeys.Expedition.SelectCharacter));
        Assert.True(recognizer.IsMatch("Entrar", GameTextKeys.Common.Enter));
        Assert.True(recognizer.IsMatch("Sair", GameTextKeys.Common.Leave));
        Assert.True(recognizer.IsMatch("Bule de Relachá", GameTextKeys.WorldArea.SereniteaPot));
        Assert.True(recognizer.IsMatch("Conta", GameTextKeys.Redemption.Account));
        Assert.True(recognizer.IsMatch("Ir para Resgatar", GameTextKeys.Redemption.GoToRedeem));
        Assert.True(recognizer.IsMatch("Resgatar Recompensa", GameTextKeys.Redemption.RedeemReward));
        Assert.True(recognizer.IsMatch("Colar", GameTextKeys.Common.Paste));
        Assert.True(recognizer.IsMatch("Resgate realizado com sucesso", GameTextKeys.Redemption.Success));
        Assert.Equal("Resgatar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Claim));
        Assert.Equal("Selecionar Personagem", recognizer.GetPrimaryAlias(GameTextKeys.Expedition.SelectCharacter));
        Assert.Equal("Entrar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Enter));
        Assert.Equal("Sair", recognizer.GetPrimaryAlias(GameTextKeys.Common.Leave));
        Assert.Equal("Bule de Relachá", recognizer.GetPrimaryAlias(GameTextKeys.WorldArea.SereniteaPot));
        Assert.Equal("Conta", recognizer.GetPrimaryAlias(GameTextKeys.Redemption.Account));
        Assert.Equal("Ir para Resgatar", recognizer.GetPrimaryAlias(GameTextKeys.Redemption.GoToRedeem));
        Assert.Equal("Resgatar Recompensa", recognizer.GetPrimaryAlias(GameTextKeys.Redemption.RedeemReward));
        Assert.Equal("Colar", recognizer.GetPrimaryAlias(GameTextKeys.Common.Paste));
        Assert.Equal("Resgate realizado com sucesso", recognizer.GetPrimaryAlias(GameTextKeys.Redemption.Success));
    }

    [Fact]
    public void ExtractedLeyLineLabels_RecognizePortugueseOutcomeAndFightObjective()
    {
        var recognizer = CreatePortugueseRecognizer();

        Assert.True(recognizer.IsMatch("Desafio concluído", GameTextKeys.LeyLine.FightSuccess));
        Assert.True(recognizer.IsMatch("Desafio Fracassado", GameTextKeys.LeyLine.FightFailure));
        Assert.True(recognizer.IsMatch("Derrote todos os inimigos", GameTextKeys.LeyLine.FightObjective));
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
            agePrompt: "classificação etária|respons",
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
            (GameTextKeys.Common.Use, culture == "pt-BR" ? "Usar" : "使用"),
            (GameTextKeys.Common.Clear, culture == "pt-BR" ? "Limpar" : "清除"),
            (GameTextKeys.Common.Filter, culture == "pt-BR" ? "Filtrar" : "筛选"),
            (GameTextKeys.Common.Crafting, culture == "pt-BR" ? "Sintetizar" : "合成"),
            (GameTextKeys.Common.Confirm, culture == "pt-BR" ? "Confirmar" : "确认"),
            (GameTextKeys.Party.ConfirmFilter, culture == "pt-BR" ? "Confirmar Filtro" : "确认筛选"),
            (GameTextKeys.Party.ConfigurationUnavailable, culture == "pt-BR" ? "Não é possível configurar a equipe no estado atual" : "当前状态不可进行队伍配置"),
            (GameTextKeys.Party.ElementalResonance, culture == "pt-BR" ? "Ressonância Elemental" : "元素共鸣"),
            (GameTextKeys.Party.Configuration, culture == "pt-BR" ? "Configuração da Equipe" : "队伍配置"),
            (GameTextKeys.Party.Remove, culture == "pt-BR" ? "Remover" : "换下"),
            (GameTextKeys.Party.Friendship, culture == "pt-BR" ? "Amizade" : "好感"),
            (GameTextKeys.Common.Claim, culture == "pt-BR" ? "Resgatar" : "领取"),
            (GameTextKeys.Expedition.SelectCharacter, culture == "pt-BR" ? "Selecionar Personagem" : "选择角色"),
            (GameTextKeys.Common.Enter, culture == "pt-BR" ? "Entrar" : "进入"),
            (GameTextKeys.Common.Leave, culture == "pt-BR" ? "Sair" : "离开"),
            (GameTextKeys.WorldArea.SereniteaPot, culture == "pt-BR" ? "Bule de Relachá" : "尘歌壶"),
            (GameTextKeys.Redemption.Account, culture == "pt-BR" ? "Conta" : "账户"),
            (GameTextKeys.Redemption.GoToRedeem, culture == "pt-BR" ? "Ir para Resgatar" : "前往兑换"),
            (GameTextKeys.Redemption.RedeemReward, culture == "pt-BR" ? "Resgatar Recompensa" : "兑换奖励"),
            (GameTextKeys.Common.Paste, culture == "pt-BR" ? "Colar" : "粘贴"),
            (GameTextKeys.Redemption.Success, culture == "pt-BR" ? "Resgate realizado com sucesso" : "兑换成功"),
            (GameTextKeys.LeyLine.FightSuccess, culture == "pt-BR" ? "Desafio concluído" : "挑战达成|战斗胜利|挑战成功"),
            (GameTextKeys.LeyLine.FightFailure, culture == "pt-BR" ? "Desafio Fracassado" : "挑战失败"),
            (GameTextKeys.LeyLine.FightObjective, culture == "pt-BR" ? "Derrote todos os inimigos" : "打倒|所有|敌人")
        };
        var matcher = GameTextTestFactory.Create(
            culture,
            entries.SelectMany(entry => entry.Item2.Split('|').Select(alias => (entry.Item1, alias))).ToArray());

        return new RemainingGameTextRecognizer(matcher);
    }
}
