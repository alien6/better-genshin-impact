#!/usr/bin/env python3
"""Generate BetterGI's bundled pt-BR UI dictionary from en.json.

Canonical Chinese keys remain unchanged. Curated key overrides have highest
priority, followed by source-text overrides, cache, existing clean PT-BR text,
and finally source-language-auto machine translation for uncovered entries.
Runtime BetterGI never calls an online translator.
"""

from __future__ import annotations

import argparse
import json
import re
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
I18N = ROOT / "BetterGenshinImpact" / "User" / "I18n"
EN_PATH = I18N / "en.json"
PT_PATH = I18N / "pt-BR.json"
TOOLS_I18N = ROOT / "tools" / "i18n"
CACHE_PATH = TOOLS_I18N / "pt-BR.machine-cache.json"
OVERRIDES_PATH = TOOLS_I18N / "pt-BR.overrides.json"
KEY_OVERRIDES_PATH = TOOLS_I18N / "pt-BR.key-overrides.json"

CJK_RE = re.compile(r"[\u3400-\u4dbf\u4e00-\u9fff]")
TRANSLATABLE_RE = re.compile(r"[A-Za-z\u3400-\u4dbf\u4e00-\u9fff]")
TOKEN_RE = re.compile(
    r"(https?://\S+|\{[^{}\r\n]+\}|%\d*\$?[sdif]|\\[nrt]|"
    r"BetterGI|Genshin Impact|Starward|HoYoLAB|PaddleOCR|Paddle|OCR|"
    r"ServerChan|Discord|Telegram|OneBot|Feishu|WebSocket|Webhook|"
    r"JavaScript|Windows|BitBlt|Wine|SMTP|HTML|API|UID|FPS|CMD|JS)",
    re.IGNORECASE,
)

TERM_REPLACEMENTS = {
    "Resina original": "Resina Original",
    "Resina condensada": "Resina Condensada",
    "Resina frágil": "Resina Frágil",
    "Resina transitória": "Resina Transitória",
    "Guilda dos Aventureiros": "Guilda de Aventureiros",
    "Guilda de aventureiros": "Guilda de Aventureiros",
    "Bule Serenitea": "Bule de Relachá",
    "Pote Serenitea": "Bule de Relachá",
    "Serenitea Pot": "Bule de Relachá",
    "Artefacto": "Artefato",
    "Artefactos": "Artefatos",
}


def load_json(path: Path) -> dict[str, str]:
    if not path.exists():
        return {}
    with path.open("r", encoding="utf-8-sig") as handle:
        data = json.load(handle)
    if not isinstance(data, dict):
        raise RuntimeError(f"{path} must contain a JSON object")
    return {str(key): str(value) for key, value in data.items()}


def save_json(path: Path, data: dict[str, str]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def protect_tokens(text: str) -> tuple[str, dict[str, str]]:
    protected: dict[str, str] = {}

    def replace(match: re.Match[str]) -> str:
        marker = f"ZXQPH{len(protected):03d}QXZ"
        protected[marker] = match.group(0)
        return marker

    return TOKEN_RE.sub(replace, text), protected


def restore_tokens(text: str, protected: dict[str, str]) -> str:
    for marker, original in protected.items():
        text = text.replace(marker, original)
        text = text.replace(marker.lower(), original)
    return text


def normalize_terms(text: str) -> str:
    for source, target in TERM_REPLACEMENTS.items():
        text = text.replace(source, target)
    return text.strip()


def google_translate(text: str) -> str:
    protected_text, protected = protect_tokens(text)
    params = urllib.parse.urlencode({
        "client": "gtx",
        "sl": "auto",
        "tl": "pt",
        "dt": "t",
        "q": protected_text,
    })
    url = "https://translate.googleapis.com/translate_a/single?" + params
    request = urllib.request.Request(url, headers={"User-Agent": "BetterGI-PTBR-Generator/1.2"})

    last_error: Exception | None = None
    for attempt in range(6):
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                payload = json.loads(response.read().decode("utf-8"))
            translated = "".join(segment[0] for segment in payload[0] if segment and segment[0])
            return normalize_terms(restore_tokens(translated, protected))
        except (urllib.error.URLError, urllib.error.HTTPError, TimeoutError, json.JSONDecodeError) as exc:
            last_error = exc
            time.sleep(min(2 ** attempt, 20))
    raise RuntimeError(f"Translation failed after retries: {text!r}") from last_error


def validate(en: dict[str, str], pt: dict[str, str]) -> list[str]:
    errors: list[str] = []
    missing = sorted(set(en) - set(pt))
    extra = sorted(set(pt) - set(en))
    if missing:
        errors.append(f"missing {len(missing)} keys; first: {missing[:10]}")
    if extra:
        errors.append(f"extra {len(extra)} keys; first: {extra[:10]}")

    empty = [key for key in en if not pt.get(key, "").strip()]
    if empty:
        errors.append(f"empty {len(empty)} values; first: {empty[:10]}")

    cjk_values = [key for key, value in pt.items() if CJK_RE.search(value)]
    if cjk_values:
        errors.append(f"PT-BR values still containing CJK: {len(cjk_values)}; first: {cjk_values[:10]}")
    return errors


def generate() -> None:
    en = load_json(EN_PATH)
    existing_pt = load_json(PT_PATH)
    cache = load_json(CACHE_PATH)
    overrides = load_json(OVERRIDES_PATH)
    key_overrides = load_json(KEY_OVERRIDES_PATH)

    unknown_override_keys = sorted(set(key_overrides) - set(en))
    if unknown_override_keys:
        print(f"Warning: {len(unknown_override_keys)} key overrides are not currently present in en.json: {unknown_override_keys[:20]}")

    result: dict[str, str] = {}
    translated_now = 0
    reused = 0

    for index, (key, source_text) in enumerate(en.items(), start=1):
        if key in key_overrides:
            value = key_overrides[key]
        elif source_text in overrides:
            value = overrides[source_text]
        elif source_text in cache and not CJK_RE.search(cache[source_text]):
            value = cache[source_text]
            reused += 1
        elif key in existing_pt and existing_pt[key].strip() and not CJK_RE.search(existing_pt[key]):
            value = existing_pt[key]
        elif not TRANSLATABLE_RE.search(source_text):
            value = source_text
        else:
            value = google_translate(source_text)
            cache[source_text] = value
            translated_now += 1
            if translated_now % 20 == 0:
                save_json(CACHE_PATH, dict(sorted(cache.items())))
                print(f"Translated {translated_now} new/mixed values ({index}/{len(en)})")
                time.sleep(0.3)
        result[key] = normalize_terms(value)

    save_json(CACHE_PATH, dict(sorted(cache.items())))
    save_json(PT_PATH, result)
    errors = validate(en, result)
    if errors:
        raise RuntimeError("; ".join(errors))
    print(f"PT-BR UI generated: {len(result)} keys; translated now={translated_now}; cache reused={reused}; key overrides={len(key_overrides)}")


def check() -> int:
    en = load_json(EN_PATH)
    pt = load_json(PT_PATH)
    errors = validate(en, pt)
    if errors:
        for error in errors:
            print("ERROR:", error)
        return 2
    print(f"PT-BR UI coverage OK: {len(pt)}/{len(en)} keys")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser()
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--generate", action="store_true")
    mode.add_argument("--check", action="store_true")
    args = parser.parse_args()
    if args.generate:
        generate()
        return 0
    return check()


if __name__ == "__main__":
    raise SystemExit(main())
