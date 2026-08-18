using System.Collections.Frozen;
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
            [GameTextKey.Confirm] = Entry(
                zhHans: ["确认"], zhHant: ["確認"], en: ["Confirm"], ja: ["確認"], ptBr: ["Confirmar"]),
            [GameTextKey.Cancel] = Entry(
                zhHans: ["取消"], zhHant: ["取消"], en: ["Cancel"], ja: ["キャンセル"], ptBr: ["Cancelar"]),
            [GameTextKey.ExitDomain] = Entry(
                zhHans: ["退出秘境"], zhHant: ["退出秘境"], en: ["Leave Domain", "Exit Domain"], ja: ["秘境を出る"], ptBr: ["Sair do Domínio", "Sair do domínio"]),
            [GameTextKey.ExitChallenge] = Entry(
                zhHans: ["退出挑战"], zhHant: ["退出挑戰"], en: ["Leave Challenge", "Exit Challenge"], ja: ["挑戦から退出"], ptBr: ["Sair do Desafio", "Sair do desafio"]),
            [GameTextKey.LeyLineDisorder] = Entry(
                zhHans: ["地脉异常"], zhHant: ["地脈異常"], en: ["Ley Line Disorder"], ja: ["地脈異常"], ptBr: ["Desordem das Linhas Ley"]),
            [GameTextKey.ItemExpired] = Entry(
                zhHans: ["物品过期"], zhHant: ["物品過期"], en: ["Item Expired", "Expired Item"], ja: ["アイテム期限切れ"], ptBr: ["Item Expirado", "Item expirado"]),
            [GameTextKey.ClaimReward] = Entry(
                zhHans: ["领取奖励"], zhHant: ["領取獎勵"], en: ["Claim Rewards", "Claim Reward"], ja: ["報酬を受け取る"], ptBr: ["Resgatar Recompensa", "Resgatar Recompensas", "Obter Recompensa"]),
            [GameTextKey.Teleport] = Entry(
                zhHans: ["传送"], zhHant: ["傳送"], en: ["Teleport"], ja: ["ワープ"], ptBr: ["Teleportar", "Teletransportar"]),
            [GameTextKey.ChallengeCompleted] = Entry(
                zhHans: ["挑战达成"], zhHant: ["挑戰達成"], en: ["Challenge Completed", "Challenge Complete"], ja: ["挑戦達成"], ptBr: ["Desafio Concluído", "Desafio concluído"]),
            [GameTextKey.Skip] = Entry(
                zhHans: ["跳过"], zhHant: ["跳過"], en: ["Skip"], ja: ["スキップ"], ptBr: ["Pular"]),
            [GameTextKey.MatchingChallenge] = Entry(
                zhHans: ["匹配挑战"], zhHant: ["匹配挑戰"], en: ["Match", "Match Challenge"], ja: ["マッチング挑戦"], ptBr: ["Desafio Multijogador", "Desafio de Pareamento", "Parear"]),
            [GameTextKey.RapidFormation] = Entry(
                zhHans: ["快速编队"], zhHant: ["快速編隊"], en: ["Quick Setup", "Quick Party Setup"], ja: ["クイック編成"], ptBr: ["Formação Rápida", "Configuração Rápida"]),
            [GameTextKey.ClickAnywhereToClose] = Entry(
                zhHans: ["点击任意位置关闭"], zhHant: ["點擊任意位置關閉"], en: ["Click anywhere to close"], ja: ["任意の場所をクリックして閉じる"], ptBr: ["Clique em qualquer lugar para fechar", "Clique em qualquer lugar para sair"])
        }.ToFrozenDictionary(
            pair => pair.Key,
            pair => pair.Value.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            StringComparer.Ordinal);

    public static string Get(string key, CultureInfo culture)
    {
        return GetAll(key, culture)[0];
    }

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

    public static bool IsDefined(string key, CultureInfo culture)
    {
        return Entries.TryGetValue(key, out var byCulture)
               && byCulture.ContainsKey(NormalizeCulture(culture));
    }

    public static IReadOnlyCollection<string> Keys => Entries.Keys;

    private static Dictionary<string, string[]> Entry(
        string[] zhHans,
        string[] zhHant,
        string[] en,
        string[] ja,
        string[] ptBr)
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

    private static string NormalizeCulture(CultureInfo culture)
    {
        var name = culture.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return "zh-Hans";
        }

        if (name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase)
            || name.Equals("zh-TW", StringComparison.OrdinalIgnoreCase)
            || name.Equals("zh-HK", StringComparison.OrdinalIgnoreCase)
            || name.Equals("zh-MO", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-Hant";
        }

        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-Hans";
        }

        if (name.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            return "pt-BR";
        }

        if (name.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        if (name.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
        {
            return "ja";
        }

        return name;
    }

    private static KeyNotFoundException MissingKey(string key, CultureInfo culture)
    {
        return new KeyNotFoundException(
            $"Game text key '{key}' is not defined for culture '{culture.Name}'. " +
            "Automation text matching does not fall back to Chinese for unsupported cultures.");
    }
}
