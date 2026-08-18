#!/usr/bin/env python3
"""Audit BetterGI core for functional Simplified-Chinese text dependencies."""
from __future__ import annotations

import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "BetterGenshinImpact"
REPORT = ROOT / "reports" / "ptbr-core-hardcodes.json"

CJK_RE = re.compile(r"[\u3400-\u4dbf\u4e00-\u9fff]")
CS_STRING_RE = re.compile(r'(?<!@)"(?P<value>(?:\\.|[^"\\])*)"|@"(?P<verbatim>(?:""|[^"])*)"')
FUNCTIONAL_TOKENS = (
    "OcrMatch", "AllContainMatchText", "OneContainMatchText", "RegexMatchText",
    ".Contains(", ".Equals(", "==", "!=", "FirstOrDefault", "WaitForElement",
    "ChooseTalkOption", "SingleSelectText", ".Text",
)
IGNORE_TOKENS = (
    "WithCultureGet", "LogInformation", "LogDebug", "LogWarning", "LogError",
    "Logger.", "_logger.", "Notify.", "ThemedMessageBox",
)
JSON_MATCH_KEYS = {"allContainMatchText", "oneContainMatchText", "regexMatchText", "text", "matchText"}


def line_comment_index(line: str) -> int:
    quote = False
    escaped = False
    for i, ch in enumerate(line[:-1]):
        if quote:
            if escaped:
                escaped = False
            elif ch == "\\":
                escaped = True
            elif ch == '"':
                quote = False
            continue
        if ch == '"':
            quote = True
            continue
        if ch == "/" and line[i + 1] == "/":
            return i
    return -1


def scan_cs(path: Path) -> list[dict[str, object]]:
    findings: list[dict[str, object]] = []
    try:
        text = path.read_text(encoding="utf-8-sig")
    except (UnicodeDecodeError, OSError):
        return findings

    in_block_comment = False
    for number, original_line in enumerate(text.splitlines(), start=1):
        line = original_line
        if in_block_comment:
            end = line.find("*/")
            if end == -1:
                continue
            line = line[end + 2:]
            in_block_comment = False

        while "/*" in line:
            start = line.find("/*")
            end = line.find("*/", start + 2)
            if end == -1:
                line = line[:start]
                in_block_comment = True
                break
            line = line[:start] + line[end + 2:]

        idx = line_comment_index(line)
        if idx >= 0:
            line = line[:idx]
        if not CJK_RE.search(line) or any(token in line for token in IGNORE_TOKENS):
            continue
        if not any(token in line for token in FUNCTIONAL_TOKENS):
            continue

        literals: list[str] = []
        for match in CS_STRING_RE.finditer(line):
            value = match.group("value") if match.group("value") is not None else match.group("verbatim")
            if value and CJK_RE.search(value):
                literals.append(value.replace('""', '"'))
        if literals:
            findings.append({
                "path": path.relative_to(ROOT).as_posix(),
                "line": number,
                "literals": literals,
                "source": original_line.strip()[:600],
            })
    return findings


def walk_json(node: object, found: list[str]) -> None:
    if isinstance(node, dict):
        for key, value in node.items():
            if key in JSON_MATCH_KEYS:
                if isinstance(value, str) and CJK_RE.search(value):
                    found.append(value)
                elif isinstance(value, list):
                    found.extend(item for item in value if isinstance(item, str) and CJK_RE.search(item))
            walk_json(value, found)
    elif isinstance(node, list):
        for value in node:
            walk_json(value, found)


def scan_json(path: Path) -> list[dict[str, object]]:
    try:
        data = json.loads(path.read_text(encoding="utf-8-sig"))
    except (UnicodeDecodeError, json.JSONDecodeError, OSError):
        return []
    found: list[str] = []
    walk_json(data, found)
    if not found:
        return []
    return [{
        "path": path.relative_to(ROOT).as_posix(),
        "line": None,
        "literals": sorted(set(found)),
        "source": "recognition JSON text matcher",
    }]


def main() -> int:
    findings: list[dict[str, object]] = []
    for path in SOURCE_ROOT.rglob("*.cs"):
        findings.extend(scan_cs(path))
    for path in SOURCE_ROOT.rglob("Recognition.json"):
        findings.extend(scan_json(path))

    frequency: Counter[str] = Counter()
    file_frequency: Counter[str] = Counter()
    for finding in findings:
        frequency.update(finding["literals"])
        file_frequency[finding["path"]] += 1

    report = {
        "functional_cjk_count": len(findings),
        "literal_frequency": dict(frequency.most_common()),
        "file_frequency": dict(file_frequency.most_common()),
        "findings": findings,
    }
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    # ASCII-safe stdout for Windows runners; report remains readable UTF-8.
    print(json.dumps({
        "functional_cjk_count": len(findings),
        "top_literals": frequency.most_common(40),
        "top_files": file_frequency.most_common(30),
        "report": REPORT.relative_to(ROOT).as_posix(),
    }, ensure_ascii=True, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
