using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace BetterGenshinImpact.Core.Localization;

/// <summary>
/// PT-BR translations for script-specific game text that does not yet have a
/// stable semantic key. TextMap entries come from shared CHS/PT hashes; a small
/// alias table covers Traditional-Chinese spellings of already-known labels.
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
            // TextMap has "{0}分钟" -> "{0}m".
            ["分钟"] = ["m"],
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
            ["设置"] = ["Ajustes", "Configurações"]
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
