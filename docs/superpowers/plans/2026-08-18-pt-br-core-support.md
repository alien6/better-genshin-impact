# BetterGI PT-BR Core Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add first-class Brazilian Portuguese support to BetterGI while preserving Chinese/English/Japanese behavior and providing a stable semantic text API for scripts.

**Architecture:** Reuse the existing `GameCultureInfoName`, `UiCultureInfoName`, `IStringLocalizer`, `CultureHelper`, and PaddleOCR culture mapping. Add `pt-BR` as a selectable culture, add a semantic game-text catalog/API exposed through `Genshin`, and migrate critical automation away from hardcoded CJK literals incrementally.

**Tech Stack:** .NET / C#, WPF, CommunityToolkit.Mvvm, Microsoft.Extensions.Localization, PaddleOCR, BetterGI script host.

**Spec:** `/mnt/data/bettergi-ptbr-design.md` (source design approved in ChatGPT session; this repository plan is the executable version)

## Global Constraints

- Keep `zh-Hans`, `zh-Hant`, `en`, and `ja` support intact.
- Keep UI culture and game culture independent.
- Portuguese OCR must use existing Latin OCR models where supported; do not create a new OCR engine.
- Do not silently search Chinese-only text when the configured game culture is `pt-BR`.
- Prefer semantic keys over language-specific literals for functional automation.
- Preserve upstream architecture and minimize merge conflicts.
- Success criterion from repository instructions: `dotnet build BetterGenshinImpact.sln -c Debug` must compile when a build environment is available.

---

### Task 1: Register pt-BR in selectable language lists

**Files:**
- Modify: `BetterGenshinImpact/ViewModel/Pages/CommonSettingsPageViewModel.cs`
- Test: existing settings/localization tests or a new focused unit test if needed

**Interfaces:**
- Consumes: `CultureInfoNameToKVPConverter.GetDisplayName(string)`
- Produces: UI/game language dictionaries containing `pt-BR`

- [ ] Add `pt-BR` to the language collection currently containing `zh-Hans`, `zh-Hant`, `en`, and `ja`.
- [ ] Confirm the UI and game language selectors bind to this collection without coupling the two settings.
- [ ] Add/adjust unit coverage asserting `pt-BR` is accepted.
- [ ] Run relevant tests/build.
- [ ] Commit as `feat(i18n): register pt-BR culture`.

### Task 2: Verify Portuguese OCR routes to the Latin Paddle model

**Files:**
- Inspect/modify only if needed: `BetterGenshinImpact/Core/Recognition/OCR/Paddle/PaddleOcrService.cs`
- Test: `Test/BetterGenshinImpact.UnitTest/...` focused OCR culture test

**Interfaces:**
- Consumes: `PaddleOcrService.PaddleOcrModelType.FromCultureInfo(CultureInfo)`
- Produces: deterministic `pt-BR -> V5Latin` mapping

- [ ] Write a failing/guard test using `new CultureInfo("pt-BR")`.
- [ ] Assert `FromCultureInfo(...)` returns `V5Latin`.
- [ ] If current code already passes, retain the test as regression coverage and avoid production-code changes.
- [ ] Run the focused test/build.
- [ ] Commit as `test(ocr): cover pt-BR Latin model mapping`.

### Task 3: Add Brazilian Portuguese UI translation file bootstrap

**Files:**
- Create: `BetterGenshinImpact/User/I18n/pt-BR.json`
- Inspect: `BetterGenshinImpact/Service/JsonTranslationService.cs`

**Interfaces:**
- Consumes: existing JSON dictionary translation loader
- Produces: local bundled `pt-BR` UI translations with graceful fallback for missing entries

- [ ] Use the existing translation JSON schema exactly.
- [ ] Seed high-value settings/navigation/common-action strings first; do not change canonical source keys.
- [ ] Validate JSON deserializes as `Dictionary<string,string>`.
- [ ] Verify missing keys retain existing fallback behavior.
- [ ] Commit as `feat(i18n): add pt-BR translation resource`.

### Task 4: Add semantic game-text catalog

**Files:**
- Create: `BetterGenshinImpact/Core/Localization/GameTextKey.cs`
- Create: `BetterGenshinImpact/Core/Localization/GameTextCatalog.cs`
- Test: new focused unit tests under `Test/BetterGenshinImpact.UnitTest`

**Interfaces:**
- Produces:
  - `GameTextKey` semantic identifiers
  - `GameTextCatalog.Get(string key, CultureInfo culture) -> string`
  - `GameTextCatalog.GetAll(string key, CultureInfo culture) -> IReadOnlyList<string>`
- Initial keys: `confirm`, `cancel`, `exit_domain`, `exit_challenge`, `ley_line_disorder`, `item_expired`, `claim_reward`, `teleport`, `challenge_completed`, `skip`, `matching_challenge`, `rapid_formation`, `click_anywhere_to_close`

- [ ] Write tests for `zh-Hans`, `en`, and `pt-BR` lookup.
- [ ] Write a test for unknown-key diagnostics/failure behavior.
- [ ] Implement a small immutable catalog with culture normalization (`pt-BR` -> exact first, then neutral culture where explicitly defined).
- [ ] Do not silently substitute Chinese for Portuguese automation matching.
- [ ] Run tests/build.
- [ ] Commit as `feat(i18n): add semantic game text catalog`.

### Task 5: Expose game language and semantic text to scripts

**Files:**
- Modify: `BetterGenshinImpact/Core/Script/Dependence/Genshin.cs`
- Modify: script declaration file if maintained in this repo
- Test: focused host/API tests if available

**Interfaces:**
- Produces script API:
  - `genshin.gameCulture`
  - `genshin.getText(key)`
  - `genshin.getTexts(key)`

- [ ] Add a read-only script-facing culture property sourced from `TaskContext.Instance().Config.OtherConfig.GameCultureInfoName`.
- [ ] Add `GetText(string key)` delegating to `GameTextCatalog`.
- [ ] Add `GetTexts(string key)` returning accepted text variants for the configured culture.
- [ ] Keep existing script methods source-compatible.
- [ ] Add API declaration/documentation if required by repository conventions.
- [ ] Run tests/build.
- [ ] Commit as `feat(script): expose localized game text API`.

### Task 6: Add semantic OCR matching helpers for scripts

**Files:**
- Modify: `BetterGenshinImpact/Core/Script/Dependence/Genshin.cs` and/or the established script vision helper location
- Test: focused helper tests

**Interfaces:**
- Produces:
  - `genshin.findTextKey(key, region)`
  - `genshin.findTextKeyAndClick(key, region)`

- [ ] Reuse existing OCR/recognition primitives rather than implementing OCR again.
- [ ] Match against `getTexts(key)` variants.
- [ ] Return clear diagnostics when the key is not defined for configured game culture.
- [ ] Preserve cancellation/error conventions used by existing script APIs.
- [ ] Run tests/build.
- [ ] Commit as `feat(script): add semantic OCR text helpers`.

### Task 7: Migrate one critical core flow as the reference implementation

**Files:**
- Modify: `BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.cs`
- Test: AutoDomain localization tests

**Interfaces:**
- Consumes: existing `IStringLocalizer`/`CultureHelper` and/or `GameTextCatalog`
- Produces: no direct functional dependency on Chinese literals for the selected critical UI prompts

- [ ] Replace critical prompt literals with culture-aware lookups while preserving existing localized-string behavior.
- [ ] Leave domain/entity-name migration for a later dedicated task unless required for the tested flow.
- [ ] Add tests covering at least `zh-Hans` and `pt-BR` lookup paths.
- [ ] Run tests/build.
- [ ] Commit as `refactor(i18n): localize AutoDomain functional text`.

### Task 8: Add hardcoded game-text regression scanner

**Files:**
- Create: `tools/i18n/scan-game-text-hardcodes.*` using repository-appropriate scripting language
- Optionally modify: `.github/workflows/...` only if CI conventions permit

**Interfaces:**
- Produces report categories: `NEW_HARDCODED_GAME_TEXT`, `MISSING_PT_BR_KEY`, `NEW_SCRIPT_OCR_LITERAL`, `NEW_TEXT_TEMPLATE`, `NEW_ENTITY_NAME_DEPENDENCY`

- [ ] Scan only functional matching contexts to avoid flooding on logs/comments.
- [ ] Provide allowlist support for intentional literals.
- [ ] Make local invocation deterministic.
- [ ] Add CI integration only after local scanner behavior is stable.
- [ ] Commit as `chore(i18n): add hardcoded text scanner`.

### Task 9: Verification

**Files:** none unless fixes are required

- [ ] Run `dotnet build BetterGenshinImpact.sln -c Debug` where supported.
- [ ] Run focused unit tests added above.
- [ ] Verify no changes landed on `main`.
- [ ] Review diff for unrelated generated/build files.
- [ ] Document any tests that cannot be run in the current environment.
