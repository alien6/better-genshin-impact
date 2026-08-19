using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace BetterGenshinImpact.Core.Localization;

/// <summary>
/// PT-BR translations for script-specific game text that does not yet have a
/// stable semantic key. Most entries come from shared TextMap CHS/PT hashes;
/// context-audited entries are limited to terms whose PT wording is visible in
/// paired TextMap phrases. Traditional-Chinese aliases share the audited value.
/// </summary>
public static class GameLiteralCatalog
{
    private static readonly FrozenDictionary<string, string[]> PtBr =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["世界"] = ["Mundo"],
            ["保存配置"] = ["Salvar Ajustes"],
            ["停止"] = ["Pausar"],
            ["切换账号"] = ["Alterar conta"],
            ["删除"] = ["Excluir"],
            ["单人挑战"] = ["Desafio Single player"],
            ["原石"] = ["Gema Essencial"],
            ["合成"] = ["Sintetizar"],
            ["售罄"] = ["Esgotado"],
            ["地脉"] = ["Linha Ley"],
            ["复苏"] = ["Reviver"],
            ["多人游戏申请"] = ["Solicitação de Modo Multijogador"],
            ["大厅"] = ["Salão"],
            ["尘歌壶"] = ["Bule de Relachá"],
            ["已售罄"] = ["Esgotado"],
            ["征讨领域"] = ["Domínio Dizimado"],
            ["所有"] = ["Todos"],
            ["按键"] = ["Comandos"],
            ["按鍵"] = ["Comandos"],
            ["提瓦特"] = ["Teyvat"],
            ["查看资料"] = ["Ver perfil"],
            ["浓缩树脂"] = ["Resina Condensada"],
            ["激活"] = ["Ativar"],
            ["点击领取"] = ["Pressione para resgatar"],
            ["申请加入"] = ["Solicitar Entrada"],
            ["秒"] = ["s"],
            // TextMap: "{0}分钟" -> "{0}m".
            ["分钟"] = ["m"],
            // Context-audited TextMap prompts consistently use "Pressione" for
            // clickable UI actions; "Toque" is retained as an accepted OCR variant.
            ["点击"] = ["Pressione", "Toque"],
            ["空月祝福"] = ["Bênção da Lua Nova"],
            ["继续"] = ["Continuar"],
            ["脆弱树脂"] = ["Resina Frágil"],
            ["自动"] = ["Automático"],
            ["获得"] = ["Obtido"],
            ["装备"] = ["Equipar"],
            ["角色选择"] = ["Seleção de Personagem"],
            ["返回"] = ["Voltar"],
            ["进入游戏"] = ["Conectar"],
            ["追踪"] = ["Rastrear"],
            ["须臾树脂"] = ["Resina Transiente"],
            ["再见"] = ["Adeus", "Tchau"],
            ["多人游戏"] = ["Modo Multijogador", "Multijogador"],
            ["开始挑战"] = ["Iniciar", "Iniciar Desafio"],
            ["打倒所有敌人"] = ["Derrotar todos os oponentes", "Derrote todos os inimigos", "Derrote todos os oponentes"],
            ["替换"] = ["Substituir", "Trocar"],
            ["设置"] = ["Ajustes", "Configurações"],

            // Context-audited interaction/Ley Line terms.
            ["使用"] = ["Usar", "Use"],
            ["补充"] = ["Recarregar", "Reabastecer", "Reabastecimento"],
            ["激活地脉之花"] = ["Ativar a Flor da Linha Ley", "Ativar Flor da Linha Ley"],
            ["选择激活方式"] = ["Selecionar modo de ativação", "Escolha o método de ativação"],
            ["接触"] = ["Toque", "Tocar"],
            ["之花"] = ["Flor"],
            ["溢口"] = ["Abertura", "Aberturas"],
            ["双倍"] = ["dobro", "em dobro", "2x"],
            ["2倍产出"] = ["2x", "2 vezes", "dobro"],
            ["2倍"] = ["2x", "2 vezes", "dobro"],
            ["20个"] = ["20"],
            ["40个"] = ["40"],
            ["晶蝶"] = ["Borboleta de Cristal", "Borboletas de Cristal"],
            ["装置"] = ["dispositivo", "mecanismo"],
            ["挂起来吧"] = ["Pendure-o.", "Pendure-o"],
            ["申请造访"] = ["Inscrever-se para visitar o Bule de Relachá", "Se registrar para visitar o Bule de Relachá"],
            ["申請造訪"] = ["Inscrever-se para visitar o Bule de Relachá", "Se registrar para visitar o Bule de Relachá"],
            ["委託"] = ["Comissão"],

            // Genius Invokation TCG wording used by OCR-driven scripts. The
            // variants preserve the in-game PT-BR terminology while tolerating
            // title/result wording differences across TCG screens.
            ["初始手牌"] = ["Mão Inicial", "Mão inicial"],
            ["重投骰子"] = ["Rolar Novamente", "Rolar novamente", "Rolagem"],
            ["出战角色"] = ["Personagem em Combate", "Personagem em combate"],
            ["对局胜利"] = ["Vitória", "Vitória na Partida", "Vitória na partida"],
            ["对局失败"] = ["Derrota", "Derrota na Partida", "Derrota na partida"],

            // Chest UI wording used by OCR-heavy scripts.
            ["箱"] = ["Baú"],
            ["珍贵"] = ["Precioso"],
            ["珍貴"] = ["Precioso"]
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static string Get(string canonicalZhHans, CultureInfo culture) =>
        GetAll(canonicalZhHans, culture)[0];

    public static IReadOnlyList<string> GetAll(string canonicalZhHans, CultureInfo culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalZhHans);
        var name = culture.Name;
        if (name.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            if (PtBr.TryGetValue(canonicalZhHans, out var values) && values.Length > 0)
            {
                return values;
            }

            throw new KeyNotFoundException(
                $"Game literal '{canonicalZhHans}' has no audited PT-BR mapping. " +
                "It is intentionally not machine-guessed for functional OCR.");
        }

        return [canonicalZhHans];
    }

    public static bool HasPortugueseMapping(string canonicalZhHans) => PtBr.ContainsKey(canonicalZhHans);

    public static IReadOnlyCollection<string> PortugueseKeys => PtBr.Keys;
}
