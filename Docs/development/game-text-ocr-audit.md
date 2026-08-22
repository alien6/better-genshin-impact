# GameTask language-dependent recognition audit

For the catalog schema, culture resolution, C#/BgiVision and JavaScript APIs, alias maintenance, validation commands, and stacked pull-request workflow, see [Localized game-text matching](game-text-localization.md).

## Scope and repeatable check

The Task 14 audit covers every tracked C# source below `BetterGenshinImpact/GameTask`. The read-only checker runs `rg` over the tracked file list from Git, then parses complete helper invocations and recognition comparisons across line boundaries. It covers `ContainsText`/`RegionHasText`, `TryClickText`/`TryClickAnyText`, `WaitUntilText`, `GetByText`/`GetByAnyText`, `FindRectByText`, `Bv.Find`/`FindF`, direct text/OCR/region/title comparisons, and both array initializers and C# collection expressions consumed by extracted OCR-text comparisons. Each candidate is assigned a stable `path::symbol::literal` identifier and checked against `game-text-non-ocr-allowlist.json`.

Run from the repository root:

```powershell
rtk pwsh scripts/game-text/Audit-LanguageDependentRecognition.ps1
```

The fail-closed regression fixtures can be run with:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass -File scripts/game-text/Test-Audit-LanguageDependentRecognition.ps1
```

The successful Task 14 baseline is:

```text
0 unclassified OCR-dependent literal comparisons
16 intentional non-OCR literals
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
| Character selection and party setup | clear/filter, confirm-filter, party-state/title, elemental resonance, remove/replace/join, order, and friendship labels | Ruling 12 migration to `common.*` and `party.*`; character and filter names remain raw inputs. |
| `CraftMaterialTask` | filter, crafting, and confirmation labels | Ruling 12 migration to existing/new `common.*`; TextMap material/product names and numeric quantities remain separate. |
| `ExpeditionTask` | claim, select-character button, and character-selection title labels | Ruling 12 migration to `common.claim`, `expedition.select_character`, and `expedition.character_selection`. |
| `AutoLeyLineOutcropTask` | fight-success, fight-failure, and objective fragments | Ruling 12 migration to `ley_line.fight_*`; resin numbers and combat behavior remain unchanged. |
| `QuickSereniteaPotTask.Done` | enter/leave and Serenitea Pot interaction labels | Ruling 12 migration to `common.*` and `world_area.serenitea_pot`. |
| `UseRedemptionCodeTask` | account, redemption-navigation, paste/clear, and success labels | Ruling 12 migration to `redemption.*` and `common.*`; redemption-code input remains untouched. |

The shared recognizer snapshots raw aliases and pre-normalized aliases once from the central singleton matcher. Each OCR string is normalized once at its recognition boundary; catalog resolution is not performed inside OCR result loops.

## Intentional literal classifications

The machine-readable allowlist is authoritative for the 16 candidates and records file, stable symbol, category, and reason. The inventory groups are:

- **Raw name/number/free-form extraction:** resin costs `20`/`40`, inventory count fallback `1`, and the compact `Lv` marker used to locate numeric talent rows. These values are language-independent parsing inputs, not semantic UI branches.
- **Internal identifier/protocol/window title:** the two burst-ready classifier class labels and the bilibili/agreement/login launcher-title fragments. These identify model classes or native windows rather than OCR game text.

Raw item names in `GetGridIconsTask`, raw weapon names/levels in character development, redemption-code input, combat syntax, character IDs, route labels, AutoGenius configuration values, logs, protocols, public APIs, clicking, timing, and launcher class/title behavior were audited and intentionally preserved.

## Dead/commented recognition code

The commented AutoPick OCR image-dump block and its obsolete literal comparison were removed. No other commented literal comparison in the audited scope is classified as live recognition behavior.

## Catalog provenance

All six supported catalogs contain the new semantic keys. Normal attack, elemental skill, and elemental burst aliases are pinned to exact TextMap hashes from AnimeGameData commit `26df1dfbdf05a82bbb1d97506859f3e1c40718d8`. OCR UI fragments and material/product labels that are not reliable standalone TextMap values are explicitly marked `curated` with reasons in `source-manifest.json`.
