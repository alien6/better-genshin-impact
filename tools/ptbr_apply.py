#!/usr/bin/env python3
from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SETTINGS_VM = ROOT / "BetterGenshinImpact" / "ViewModel" / "Pages" / "CommonSettingsPageViewModel.cs"

OLD_LANGUAGE_BLOCK = '''    [ObservableProperty] private FrozenDictionary<string, string> _languageDict =
        new[] { "zh-Hans", "zh-Hant", "en", "ja" }
            .ToFrozenDictionary(c => c, c => CultureInfoNameToKVPConverter.GetDisplayName(c));
'''

NEW_LANGUAGE_BLOCK = '''    [ObservableProperty] private FrozenDictionary<string, string> _languageDict =
        BetterGenshinImpact.Core.Localization.SupportedCultures.Names
            .ToFrozenDictionary(c => c, c => CultureInfoNameToKVPConverter.GetDisplayName(c));
'''


def patch_settings_language_list() -> bool:
    text = SETTINGS_VM.read_text(encoding="utf-8-sig")
    if NEW_LANGUAGE_BLOCK in text:
        return False
    if OLD_LANGUAGE_BLOCK not in text:
        raise RuntimeError("Could not locate CommonSettingsPageViewModel language dictionary block")
    text = text.replace(OLD_LANGUAGE_BLOCK, NEW_LANGUAGE_BLOCK, 1)
    SETTINGS_VM.write_text(text, encoding="utf-8", newline="\n")
    return True


def main() -> int:
    changed = patch_settings_language_list()
    print(f"CommonSettingsPageViewModel language list patched: {changed}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
