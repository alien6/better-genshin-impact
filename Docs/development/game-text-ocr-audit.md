# GameTask language-dependent recognition audit

## Scope and repeatable check

The Task 14 audit covers every tracked C# source below `BetterGenshinImpact/GameTask`. The read-only checker runs `rg` over the tracked file list from Git, finds literal comparisons on recognition-adjacent lines (`Text`, `Ocr`, `OcrResult`, or `Region` identifiers), derives a stable surrounding symbol, and compares the resulting `path::symbol::literal` identifiers with `game-text-non-ocr-allowlist.json`.

Run from the repository root:

```powershell
rtk pwsh scripts/game-text/Audit-LanguageDependentRecognition.ps1
```

The successful Task 14 baseline is:

```text
0 unclassified OCR-dependent literal comparisons
21 intentional non-OCR literals
```

The command exits non-zero when a new candidate is unclassified or an allowlist entry becomes stale. The allowlist deliberately does not use line numbers as identity.

## Semantic OCR migrations

| Flow | OCR meaning | Result |
| --- | --- | --- |
| `AutoPickTrigger.DoNotPick` | pickup suppression labels and compound tribe/workshop rules | Migrated to immutable, normalized aliases under `auto_pick.*`. |
| `GetGridIconsTask` | artifact “set includes” marker | Migrated to `artifact.set_contains`; raw recognized item and flower names stay raw. |
| `CharacterDevelopmentTask` | category tabs, talent info, talent type, and talent-level bonus label | Migrated to `character.*`; the level and `+3` number remain separately parsed. |
| `AutoAlbumTask.StartOneAlbum` | music album “All” page label | Ruling 11 migration to existing `common.all`. `AutoMusicGameTask` itself has no OCR text gate; its configuration values and logs remain unchanged. |
| `AutoWoodTask` | obtained marker and recognized wood product names | Migrated to `common.obtained` and `wood.material`; quantities and OCR-extracted material text remain separate. |
| `GameLoadingTrigger` | age-rating/guardian splash prompt | Migrated to `game_loading.age_prompt`; launcher window titles remain literal. |
| `CountInventoryItem` | enhancement-ore product names | Migrated to `inventory.enhancement_ore`; recognized item names and counts remain raw. |
| `LowerHeadThenWalkToTask` | activate prompt | Migrated to existing `ley_line.activate`. |
| `GoToSereniteaPotTask` | sold-out, unavailable companionship EXP, and goodbye option | Migrated to `serenitea_pot.*` and `common.goodbye`. |
| `AutoDomainTask.PressUseResin` | use button | Ruling 11 migration to existing `common.use`; raw resin names and 20/40 quantities remain unchanged. |

The shared recognizer snapshots raw aliases and pre-normalized aliases once from the central singleton matcher. Each OCR string is normalized once at its recognition boundary; catalog resolution is not performed inside OCR result loops.

## Intentional literal classifications

The machine-readable allowlist is authoritative for the 21 candidates and records file, stable symbol, category, and reason. The inventory groups are:

- **Raw name/number/free-form extraction:** resin costs `20`/`40`, inventory count fallback `1`, and the compact `Lv` marker used to locate numeric talent rows. These values are language-independent parsing inputs, not semantic UI branches.
- **Internal identifier/protocol/window title:** `SkillCdText`, `.json`, and the bilibili/agreement/login launcher-title fragments. These identify program objects, formats, or native windows rather than OCR game text.
- **Configuration/user string:** the `无` sentinel in the expedition-country configuration is evaluated before recognition begins.
- **Logs/public behavior:** four AutoDomain diagnostic templates happen to share a conditional line with a text-related identifier but are never compared with OCR output.

Raw item names in `GetGridIconsTask`, raw weapon names/levels in character development, redemption-code input, combat syntax, character IDs, route labels, AutoGenius configuration values, logs, protocols, public APIs, clicking, timing, and launcher class/title behavior were audited and intentionally preserved.

## Dead/commented recognition code

The commented AutoPick OCR image-dump block and its obsolete literal comparison were removed. No other commented literal comparison in the audited scope is classified as live recognition behavior.

## Catalog provenance

All six supported catalogs contain the new semantic keys. Normal attack, elemental skill, and elemental burst aliases are pinned to exact TextMap hashes from AnimeGameData commit `26df1dfbdf05a82bbb1d97506859f3e1c40718d8`. OCR UI fragments and material/product labels that are not reliable standalone TextMap values are explicitly marked `curated` with reasons in `source-manifest.json`.
