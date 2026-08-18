using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace BetterGenshinImpact.Core.Localization;

/// <summary>
/// Culture-aware catalog for static game UI text used by OCR-driven automation.
/// Values are ordered from the preferred client wording to tolerated OCR variants.
/// </summary>
public static class GameTextCatalog
{
    private static readonly FrozenDictionary<string, FrozenDictionary<string, string[]>> Entries =
        new Dictionary<string, Dictionary<string, string[]>>(StringComparer.Ordinal)
        {
            [GameTextKey.Confirm] = Entry(zhHans: ["确认"], zhHant: ["確認"], en: ["Confirm"], ja: ["確認"], ptBr: ["Confirmar"]),
            [GameTextKey.Ok] = Entry(zhHans: ["确定"], zhHant: ["確定"], en: ["OK", "Confirm"], ja: ["決定", "確認"], ptBr: ["OK", "Confirmar"]),
            [GameTextKey.Cancel] = Entry(zhHans: ["取消"], zhHant: ["取消"], en: ["Cancel"], ja: ["キャンセル"], ptBr: ["Cancelar"]),
            [GameTextKey.ExitDomain] = Entry(zhHans: ["退出秘境"], zhHant: ["退出秘境"], en: ["Leave Domain", "Exit Domain"], ja: ["秘境を出る"], ptBr: ["Sair do Domínio", "Sair do domínio"]),
            [GameTextKey.ExitChallenge] = Entry(zhHans: ["退出挑战"], zhHant: ["退出挑戰"], en: ["Leave Challenge", "Exit Challenge"], ja: ["挑戦から退出"], ptBr: ["Sair do Desafio", "Sair do desafio"]),
            [GameTextKey.LeyLineDisorder] = Entry(zhHans: ["地脉异常"], zhHant: ["地脈異常"], en: ["Ley Line Disorder"], ja: ["地脈異常"], ptBr: ["Desordem das Linhas Ley"]),
            [GameTextKey.ItemExpired] = Entry(zhHans: ["物品过期"], zhHant: ["物品過期"], en: ["Item Expired", "Expired Item"], ja: ["アイテム期限切れ"], ptBr: ["Item Expirado", "Item expirado"]),
            [GameTextKey.ClaimReward] = Entry(zhHans: ["领取奖励"], zhHant: ["領取獎勵"], en: ["Claim Rewards", "Claim Reward"], ja: ["報酬を受け取る"], ptBr: ["Resgatar Recompensa", "Resgatar Recompensas", "Obter Recompensa"]),
            [GameTextKey.ClaimAll] = EntryPt(["全部领取"], ["Resgatar tudo", "Coletar tudo"], en: ["Claim All"]),
            [GameTextKey.Teleport] = Entry(zhHans: ["传送"], zhHant: ["傳送"], en: ["Teleport"], ja: ["ワープ"], ptBr: ["Teleportar", "Teletransportar"]),
            [GameTextKey.ChallengeCompleted] = Entry(zhHans: ["挑战达成"], zhHant: ["挑戰達成"], en: ["Challenge Completed", "Challenge Complete"], ja: ["挑戦達成"], ptBr: ["Desafio Concluído", "Desafio concluído"]),
            [GameTextKey.Skip] = Entry(zhHans: ["跳过"], zhHant: ["跳過"], en: ["Skip"], ja: ["スキップ"], ptBr: ["Pular"]),
            [GameTextKey.MatchingChallenge] = Entry(zhHans: ["匹配挑战"], zhHant: ["匹配挑戰"], en: ["Match", "Match Challenge"], ja: ["マッチング挑戦"], ptBr: ["Desafio Multijogador", "Desafio de Pareamento", "Parear"]),
            [GameTextKey.RapidFormation] = Entry(zhHans: ["快速编队"], zhHant: ["快速編隊"], en: ["Quick Setup", "Quick Party Setup"], ja: ["クイック編成"], ptBr: ["Formação Rápida", "Configuração Rápida"]),
            [GameTextKey.ClickAnywhereToClose] = Entry(zhHans: ["点击任意位置关闭"], zhHant: ["點擊任意位置關閉"], en: ["Click anywhere to close"], ja: ["任意の場所をクリックして閉じる"], ptBr: ["Clique em qualquer lugar para fechar", "Clique em qualquer lugar para sair"]),
            [GameTextKey.OriginalResin] = EntryPt(["原粹树脂"], ["Resina Original"], en: ["Original Resin"]),
            [GameTextKey.StartGame] = EntryPt(["开始游戏"], ["Iniciar Jogo", "Começar Jogo"], en: ["Start Game"]),
            [GameTextKey.CurrentOwned] = EntryPt(["当前拥有"], ["Possui atualmente", "Atualmente possui"], en: ["Currently Owned", "Owned"]),
            [GameTextKey.Search] = EntryPt(["搜索"], ["Buscar", "Pesquisar"], en: ["Search"]),
            [GameTextKey.Room] = EntryPt(["房间"], ["Sala"], en: ["Room"]),
            [GameTextKey.Artifact] = EntryPt(["圣遗物"], ["Artefato", "Artefatos"], en: ["Artifact", "Artifacts"]),
            [GameTextKey.Cooking] = EntryPt(["烹饪"], ["Cozinhar", "Culinária"], en: ["Cook", "Cooking"]),
            [GameTextKey.CurrentParty] = EntryPt(["当前队伍"], ["Equipe atual"], en: ["Current Party"]),
            [GameTextKey.ClickToEnter] = EntryPt(["点击进入"], ["Clique para entrar", "Clicar para entrar"], en: ["Click to Enter"]),
            [GameTextKey.GrowthRequirements] = EntryPt(["培养需求"], ["Materiais necessários", "Requisitos de desenvolvimento"], en: ["Required Materials", "Development Requirements"]),
            [GameTextKey.CancelAutoTask] = EntryPt(["取消自动任务"], ["Cancelar tarefa automática"], en: ["Cancel Auto Task"]),
            [GameTextKey.Bait] = EntryPt(["鱼饵"], ["Isca"], en: ["Bait"]),
            [GameTextKey.Revive] = EntryPt(["复活"], ["Reviver"], en: ["Revive"]),
            [GameTextKey.AutoExit] = EntryPt(["自动退出"], ["Sair automaticamente", "Saída automática"], en: ["Auto Exit", "Exit Automatically"]),
            [GameTextKey.TouchTrounceBlossom] = EntryPt(["接触征讨之花"], ["Tocar na Flor da Punição", "Toque na Flor da Punição"], en: ["Touch the Trounce Blossom"]),
            [GameTextKey.AwaitingActivation] = EntryPt(["待激活"], ["Aguardando ativação", "Pendente de ativação"], en: ["Awaiting Activation"]),
            [GameTextKey.CommissionCompleted] = EntryPt(["委托完成"], ["Comissão concluída"], en: ["Commission Complete", "Commission Completed"]),
            [GameTextKey.Commission] = EntryPt(["委托"], ["Comissão"], en: ["Commission"], zhHant: ["委託"]),
            [GameTextKey.DailyCommissionRewards] = EntryPt(["每日委托奖励"], ["Recompensas de Comissão Diária", "Recompensas das Comissões Diárias"], en: ["Daily Commission Rewards"]),
            [GameTextKey.Manage] = EntryPt(["管理"], ["Gerenciar"], en: ["Manage"]),
            [GameTextKey.ReturnToLobby] = EntryPt(["返回大厅"], ["Voltar ao Saguão", "Retornar ao Saguão"], en: ["Return to Lobby"]),
            [GameTextKey.InsufficientMaterials] = EntryPt(["材料不足"], ["Materiais insuficientes"], en: ["Insufficient Materials"]),
            [GameTextKey.AutoCook] = EntryPt(["自动烹饪"], ["Cozinhar automaticamente", "Cozimento automático"], en: ["Auto-Cook", "Auto Cook"]),
            [GameTextKey.CookingProduction] = EntryPt(["料理制作"], ["Preparar Prato", "Preparação de pratos", "Culinária"], en: ["Cooking", "Food Preparation"]),
            [GameTextKey.QueueFull] = EntryPt(["队列已满"], ["Fila cheia", "A fila está cheia"], en: ["Queue Full"])
        }.ToFrozenDictionary(
            pair => pair.Key,
            pair => pair.Value.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            StringComparer.Ordinal);

    public static string Get(string key, CultureInfo culture) => GetAll(key, culture)[0];

    public static IReadOnlyList<string> GetAll(string key, CultureInfo culture)
    {
        if (!Entries.TryGetValue(key, out var byCulture))
        {
            throw MissingKey(key, culture);
        }

        var normalized = NormalizeCulture(culture);
        if (!byCulture.TryGetValue(normalized, out var values) || values.Length == 0)
        {
            throw MissingKey(key, culture);
        }

        return values;
    }

    public static bool IsDefined(string key, CultureInfo culture) =>
        Entries.TryGetValue(key, out var byCulture) && byCulture.ContainsKey(NormalizeCulture(culture));

    public static IReadOnlyCollection<string> Keys => Entries.Keys;

    private static Dictionary<string, string[]> Entry(string[] zhHans, string[] zhHant, string[] en, string[] ja, string[] ptBr)
    {
        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["zh-Hans"] = zhHans,
            ["zh-Hant"] = zhHant,
            ["en"] = en,
            ["ja"] = ja,
            ["pt-BR"] = ptBr
        };
    }

    /// <summary>
    /// Add a PT-BR mapping while preserving the historical Simplified-Chinese literal
    /// for languages whose exact game wording has not yet been audited.
    /// </summary>
    private static Dictionary<string, string[]> EntryPt(string[] zhHans, string[] ptBr, string[]? en = null, string[]? ja = null, string[]? zhHant = null)
    {
        return Entry(zhHans, zhHant ?? zhHans, en ?? zhHans, ja ?? zhHans, ptBr);
    }

    private static string NormalizeCulture(CultureInfo culture)
    {
        var name = culture.Name;
        if (string.IsNullOrWhiteSpace(name)) return "zh-Hans";

        if (name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase)
            || name.Equals("zh-TW", StringComparison.OrdinalIgnoreCase)
            || name.Equals("zh-HK", StringComparison.OrdinalIgnoreCase)
            || name.Equals("zh-MO", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-Hant";
        }
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return "zh-Hans";
        if (name.StartsWith("pt", StringComparison.OrdinalIgnoreCase)) return "pt-BR";
        if (name.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return "en";
        if (name.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return "ja";
        return name;
    }

    private static KeyNotFoundException MissingKey(string key, CultureInfo culture)
    {
        return new KeyNotFoundException(
            $"Game text key '{key}' is not defined for culture '{culture.Name}'. " +
            "Automation text matching does not fall back to Chinese for unsupported cultures.");
    }
}
