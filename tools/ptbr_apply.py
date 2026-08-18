#!/usr/bin/env python3
from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SETTINGS_VM = ROOT / "BetterGenshinImpact" / "ViewModel" / "Pages" / "CommonSettingsPageViewModel.cs"
BV_STATUS = ROOT / "BetterGenshinImpact" / "GameTask" / "Common" / "BgiVision" / "BvStatus.cs"

OLD_LANGUAGE_BLOCK = '''    [ObservableProperty] private FrozenDictionary<string, string> _languageDict =
        new[] { "zh-Hans", "zh-Hant", "en", "ja" }
            .ToFrozenDictionary(c => c, c => CultureInfoNameToKVPConverter.GetDisplayName(c));
'''

NEW_LANGUAGE_BLOCK = '''    [ObservableProperty] private FrozenDictionary<string, string> _languageDict =
        BetterGenshinImpact.Core.Localization.SupportedCultures.Names
            .ToFrozenDictionary(c => c, c => CultureInfoNameToKVPConverter.GetDisplayName(c));
'''

OLD_REVIVE_BLOCK = '''        using var r = list.FirstOrDefault(r => r.Text.Contains("复苏"));
        if (r != null)
'''

NEW_REVIVE_BLOCK = '''        CultureInfo cultureInfo = new CultureInfo(TaskContext.Instance().Config.OtherConfig.GameCultureInfoName);
        IStringLocalizer stringLocalizer = App.GetService<IStringLocalizer<BvResxHelper>>() ?? throw new Exception();
        string revival = stringLocalizer.WithCultureGet(cultureInfo, "复苏");
        using var r = list.FirstOrDefault(r => r.Text.Contains(revival));
        if (r != null)
'''


def replace_once(path: Path, old: str, new: str, label: str) -> bool:
    text = path.read_text(encoding="utf-8-sig")
    if new in text:
        return False
    if old not in text:
        raise RuntimeError(f"Could not locate {label} in {path}")
    newline = "\r\n" if "\r\n" in text else "\n"
    updated = text.replace(old, new, 1).replace("\r\n", "\n").replace("\r", "\n")
    path.write_text(updated.replace("\n", newline), encoding="utf-8", newline="")
    return True


def patch_settings_language_list() -> bool:
    return replace_once(SETTINGS_VM, OLD_LANGUAGE_BLOCK, NEW_LANGUAGE_BLOCK, "language dictionary block")


def patch_revive_text() -> bool:
    return replace_once(BV_STATUS, OLD_REVIVE_BLOCK, NEW_REVIVE_BLOCK, "revive OCR block")


def main() -> int:
    changes = {
        "language_list": patch_settings_language_list(),
        "revive_text": patch_revive_text(),
    }
    print(changes)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
