# PT-BR OCR, Automation, and Script Localization Design

**Status:** Approved in chat on 2026-08-21

**Target branch:** `feat/pt-br-ocr-automation`

**Base branch:** `i18n/pt-br-completo` (PR #2)

**Pull request strategy:** Open a stacked pull request against `i18n/pt-br-completo`; do not merge automatically.

If PR #2 is merged while this work is in progress, retarget the follow-up pull request to `main` after verifying the resulting diff and checks. Do not rewrite or expand PR #2 merely to publish this follow-up.

## Context

PR #2 localizes the BetterGI user interface into Brazilian Portuguese while intentionally preserving Chinese strings used by OCR and automation. The follow-up work makes those game-facing recognition paths operate when Genshin Impact itself is configured for `pt-BR`.

The initial audit found:

- `OcrFactory` already resolves `pt-BR` to the PaddleOCR V5 Latin recognition model through `PaddleOcrModelType.FromCultureInfo`;
- changing `GameCultureInfoName` unloads the existing OCR service, so the next use creates a model for the newly selected language;
- at least 139 Chinese string-comparison calls remain across 33 `GameTask` source files, although some compare internal names or logs rather than OCR output;
- 11 task-local resource groups support `zh-Hans`, `zh-Hant`, `en`, and `fr`, but none provides `pt-BR` values;
- `BvPage`, `BvFlow`, and `BvLocator` expose literal-text OCR operations to JavaScript, with no stable semantic alias API;
- the raw OCR result is also consumed for names, numbers, quantities, and free-form text, so translating or rewriting OCR output globally would be unsafe.

## Goals

1. Make first-party OCR-dependent automations recognize the game UI in `pt-BR`.
2. Preserve existing Chinese and English recognition behavior and retain existing `zh-Hant`, French, and Japanese behavior when a migrated flow already supports or can source those languages.
3. Keep raw OCR text unchanged and apply localization only at text-matching boundaries.
4. Provide stable, culture-aware APIs for C# automation and JavaScript scripts.
5. Make language coverage auditable and maintainable when Genshin or BetterGI changes upstream.
6. Add unit, integration, catalog, and build validation proportional to the affected behavior.

## Non-goals

- Translating BetterGI UI text already covered by PR #2.
- Translating OCR results into a canonical language.
- Rewriting internal command names, combat-script syntax, character identifiers, route names, log protocols, launcher window titles, or other non-OCR contracts.
- Automatically modifying third-party script repositories. Existing scripts remain compatible; the new API gives script authors a language-independent migration path.
- Adding runtime network access to AnimeGameData or any other external localization source.

## Chosen Architecture

### 1. Stable semantic keys

First-party code refers to semantic identifiers rather than display text. Keys use lowercase dotted names grouped by feature, for example:

- `domain.challenge_completed`
- `domain.ley_line_disorder`
- `resin.original`
- `common.revive`
- `expedition.reward`

A `GameTextKeys` constants class supplies compile-time-safe values to C# callers. JavaScript callers use the same string identifiers.

Keys remain stable when displayed wording changes. New upstream wording is added as an alias instead of renaming the key.

### 2. Per-culture alias catalogs

Embedded JSON catalogs live together under a dedicated game-text localization directory. Each culture file maps a semantic key to one or more accepted aliases:

```json
{
  "schemaVersion": 1,
  "culture": "pt-BR",
  "entries": {
    "domain.challenge_completed": [
      "Desafio concluído"
    ],
    "resin.original": [
      "Resina Original"
    ]
  }
}
```

Required catalogs for migrated first-party flows are `zh-Hans`, `zh-Hant`, `en`, `ja`, `fr`, and `pt-BR`. TextMap-backed values are validated against matching hashes in the Genshin release data. Curated OCR variants may be added alongside the displayed value when a fixture or captured failure demonstrates the need.

The initial terminology baseline is AnimeGameData commit `26df1dfbdf05a82bbb1d97506859f3e1c40718d8` from 2026-08-16. It provides aligned `TextMapCHS`, `TextMapCHT`, `TextMapEN`, `TextMapJP`, `TextMapFR`, and `TextMapPT` files. The repository is a development-time terminology source only; compact reviewed aliases are committed into BetterGI.

### 3. Culture resolution

The matcher resolves aliases using `OtherConfig.GameCultureInfoName`, never the BetterGI UI culture. Resolution follows this order:

1. exact culture, such as `pt-BR`;
2. neutral parent, such as `pt`, when a parent catalog exists;
3. fail with a diagnostic identifying the semantic key and requested culture.

There is no cross-language fallback. In particular, a missing Portuguese alias cannot silently fall back to Chinese, because that would turn a catalog defect into a recognition timeout.

### 4. Text normalization and matching

`GameTextMatcher` keeps the OCR value observable but compares normalized copies. Normalization:

1. applies Unicode compatibility normalization;
2. converts case using invariant rules;
3. removes combining diacritical marks for comparison only;
4. removes Unicode whitespace and punctuation;
5. preserves letters, ideographs, and digits.

This makes `Desafio concluído`, `DESAFIO CONCLUIDO`, and OCR output split by spaces or punctuation comparable without maintaining speculative misspelling lists. It does not apply edit-distance or unrestricted fuzzy matching, which could create false positives in small regions.

The matcher supports three explicit operations:

- match one OCR string against any alias for one semantic key;
- match any OCR region against one semantic key;
- concatenate ordered OCR regions and match the combined text against one semantic key.

Aliases use normalized containment. Callers choose region bounds and whether to use individual or combined text, preserving the current recognition geometry and ordering logic.

## Components and Interfaces

### Catalog model and loader

The catalog model validates `schemaVersion`, culture identity, non-empty keys, non-empty aliases, duplicate aliases after normalization, and conflicting duplicate keys. Catalogs are loaded once and exposed through immutable collections.

Malformed embedded data throws a descriptive exception during matcher initialization. The exception contains the resource name, culture, and offending key when available.

### Matcher

The central matcher is a small, side-effect-free service once catalogs and culture are supplied. Its public contract is equivalent to:

```csharp
public interface IGameTextMatcher
{
    IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null);
    bool IsMatch(string recognizedText, string key, CultureInfo? culture = null);
    bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null);
    bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null);
}
```

The production instance obtains the configured game culture through a narrow provider. Tests instantiate the matcher with explicit cultures and in-memory catalogs.

Catalogs and their normalized aliases are materialized into immutable lookup tables once. Match operations do not mutate shared state or change thread culture, so concurrent OCR consumers and multiple game sessions can use the singleton safely. Alias normalization is never repeated inside a capture retry loop.

### BgiVision integration

Existing literal APIs remain unchanged. New culture-aware methods resolve aliases when a locator is created:

```csharp
page.GetByTextKey(GameTextKeys.Domain.ChallengeCompleted, rect);
page.GetByAnyTextKey(keys, rect);
flow.WaitUntilTextKey(GameTextKeys.Domain.ChallengeCompleted, rect);
```

Resolved aliases reuse the existing `BvLocator` any-text filtering path. A locator therefore has a deterministic alias snapshot for its lifetime, and changing game language takes effect for locators created by the next task or script run.

### JavaScript integration

`EngineExtend` exposes a restricted `gameText` host object with:

- `gameText.aliases(key)`;
- `gameText.matches(text, key)`;
- `gameText.matchesAny(texts, key)`;
- `gameText.culture` as read-only diagnostic information.

`BvPage` and `BvFlow` expose the key-based locator/wait methods listed above. No file, network, catalog mutation, or arbitrary type access is added to the script host.

### First-party automation integration

OCR-derived comparisons migrate to `GameTextKeys` and `IGameTextMatcher`. Existing task-local `.resx` recognition values are folded into the central catalog so one semantic term has one alias source. UI/localized log resources remain separate.

Raw OCR used for numeric parsing, artifact names, character names, inventory quantities, and free-form output stays unchanged.

## Migration Scope

Every Chinese string comparison found by the audit is classified into one of these groups:

1. **Game OCR text:** migrate to a semantic key and culture-aware matcher.
2. **Localized game text already in task `.resx`:** move recognition aliases to the shared catalog and keep behavior for every existing locale.
3. **Template/image recognition:** keep unchanged because it is not text matching.
4. **Internal contract:** keep unchanged and document why it is language-independent.
5. **User-facing BetterGI text:** leave to the PR #2 translation layer rather than the game-text matcher.
6. **External launcher or platform window text:** keep separate from Genshin game-language matching and preserve current behavior.

Priority migration flows are:

1. Domains, bosses, Ley Line Outcrops, and Stygian Onslaught reward/resin flows;
2. expeditions, daily rewards, Battle Pass rewards, and Serenitea Pot interactions;
3. fishing, revival, inventory/artifact operations, auto-pick, and loading-state OCR;
4. AutoSkip and other trigger paths that inspect game dialogue or option text;
5. BgiVision and JavaScript APIs used by downloaded scripts.

The final audit report records migrated keys and justified remaining Chinese comparisons. A remaining literal that reads OCR output is a release blocker unless it is explicitly shown to be language-independent, such as a digit-only parse.

## Data Flow

```text
Game frame
  -> culture-selected PaddleOCR model
  -> raw OcrResult / OcrResultRegion.Text
  -> caller selects relevant region(s)
  -> GameTextMatcher resolves aliases for GameCultureInfoName
  -> normalized containment match
  -> automation or script decision
```

Raw OCR values continue into logs and diagnostics. A failure therefore reports both the semantic key and the actual recognized text without altering either.

## Error Handling and Diagnostics

- Unknown semantic key: throw `KeyNotFoundException` with key and culture.
- Missing culture catalog or missing aliases for a known key: throw `InvalidOperationException` with key, requested culture, and attempted parent culture.
- Invalid catalog JSON or schema: throw a catalog-load exception with resource and validation details.
- Empty OCR text: return `false`; it is a normal recognition miss.
- OCR engine failure: preserve existing engine exceptions and lifecycle behavior.
- Script API misuse: throw `ArgumentException` with the invalid key or collection parameter.

Timeout messages from key-based locators include the semantic key and resolved aliases. This distinguishes an OCR miss from a localization-catalog defect.

## Upstream Maintenance

- Semantic keys never embed a translated phrase.
- Alias catalogs are data files, keeping language churn out of automation logic.
- New wording is appended while an older supported game version may still display it.
- A documented audit command accepts local TextMap files and reports exact matches, changed translations, ambiguous Chinese source strings, and catalog entries without a TextMap source.
- Catalog tests require every `GameTextKeys` value used by first-party code to exist in all supported catalogs.
- Literal BgiVision and script APIs remain available, so upstream scripts do not break when the semantic API is introduced.

## Testing Strategy

Development follows red-green-refactor for each behavior.

### Unit tests

- normalization of case, compatibility characters, spaces, punctuation, and Portuguese diacritics;
- exact and parent-culture resolution without cross-language fallback;
- unknown-key, missing-culture, malformed-catalog, duplicate-key, and empty-alias diagnostics;
- individual, any-region, and combined-region matching;
- `pt-BR` mapping to PaddleOCR V5 Latin and preservation of Chinese/English model mappings;
- immutable alias snapshots and duplicate-alias handling;
- key-based BgiVision resolution while existing literal APIs retain their behavior.

### Catalog tests

- schema validation for every embedded catalog;
- coverage of every first-party `GameTextKeys` constant in `zh-Hans`, `zh-Hant`, `en`, `ja`, `fr`, and `pt-BR`;
- no empty or normalization-equivalent aliases within one key;
- documented TextMap source or explicit curated-source reason for every alias group.

### Recognition and consumer regressions

- a committed Portuguese OCR fixture containing accented game terminology exercises the V5 Latin recognizer;
- representative matcher regressions cover Domains, Bosses, Ley Lines, Stygian Onslaught, Expeditions, Fishing, Revival, and inventory-related text;
- JavaScript host tests cover alias resolution, match calls, invalid keys, and unchanged literal BvPage behavior.

### Final verification

- run the complete `BetterGenshinImpact.UnitTest` suite;
- run the repository's relevant additional tests when their prerequisites are available;
- build `BetterGenshinImpact/BetterGenshinImpact.csproj` in `Release` with `Platform=x64`;
- run the permanent OCR-literal audit and review every remaining finding;
- inspect the final staged diff and PR checks before marking the PR ready for review.

## Documentation Deliverables

- architecture and maintenance guide for the game-text catalogs;
- script-author migration guide with literal and semantic API examples;
- OCR-dependent string audit report with classifications and exclusions;
- PR description summarizing affected automations, tests, build evidence, compatibility, and known validation limits.

## Acceptance Criteria

The implementation is ready for review when all of the following are true:

1. `pt-BR` selects the Latin OCR model in an automated test.
2. Every migrated first-party key has aliases for all required cultures.
3. Priority OCR-dependent automations use semantic keys rather than Chinese game-text literals.
4. BgiVision and JavaScript support semantic keys without breaking literal APIs.
5. Portuguese normalization handles accents, case, spacing, and punctuation deterministically.
6. The Portuguese OCR fixture and all matcher/consumer regressions pass.
7. Every remaining audited Chinese comparison is documented as non-OCR or language-independent.
8. The complete unit-test command and Windows Release x64 build exit successfully.
9. Documentation describes catalog updates and script migration.
10. A stacked pull request is open and ready for review, with no automatic merge.
