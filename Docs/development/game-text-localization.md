# Localized game-text matching

BetterGI automation must make decisions from semantic game-text keys instead of comparing OCR output with one language's UI text. The catalog layer maps a stable key such as `resin.original` to the aliases used by the configured game culture. C# tasks, BgiVision locators, and JavaScript scripts can therefore share the same language-independent contract.

## Raw OCR is still raw

Localization does not rewrite PaddleOCR output. `Region.Text`, item names, numbers, logs, and values returned by the existing literal BgiVision APIs remain exactly as OCR produced them. The matcher creates a normalized comparison value separately: it applies Unicode compatibility/canonical normalization, lowercases invariantly, removes combining marks, and keeps letters and digits. This makes comparisons tolerant of case, accents, punctuation, and spacing without destroying the original text needed for parsing or diagnostics.

Keep data extraction and semantic decisions separate. For example, parse a resin quantity from the raw OCR line, but use `resin.original` to decide which product the line describes. Existing literal APIs such as `GetByText` and `GetByAnyText` remain available and retain their ordinal literal behavior.

## Keys, catalogs, and provenance

Semantic keys are lowercase dotted identifiers grouped by feature. C# callers use constants from `BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs`; JavaScript callers use the same string values. Do not expose a localized phrase as a key or repurpose an existing key for a different UI meaning.

Each supported catalog is an embedded JSON resource under `BetterGenshinImpact/GameTask/Localization/Catalogs`:

```json
{
  "schemaVersion": 1,
  "culture": "pt-BR",
  "entries": {
    "resin.original": [
      "Resina Original"
    ]
  }
}
```

Every catalog must contain the same keys. An entry must contain at least one nonblank raw alias; exact raw duplicates are rejected. Distinct raw spellings may normalize to the same comparison value, in which case the matcher keeps the raw aliases for compatibility and deduplicates only the normalized view.

`source-manifest.json` records provenance for every alias in every culture. Its baseline is the canonical `DimbreathBot/AnimeGameData` repository pinned at commit `26df1dfbdf05a82bbb1d97506859f3e1c40718d8`. A TextMap-derived record identifies its exact `textMapHash`:

```json
{
  "value": "Resina Original",
  "textMapHash": "<exact TextMap key>"
}
```

An OCR variant or UI fragment that is not a reliable standalone TextMap value must be explicitly curated:

```json
{
  "value": "OCR variant",
  "source": "curated",
  "reason": "Why this observed or compatibility spelling is required."
}
```

Do not copy whole TextMaps into this repository. To add or update an alias:

1. Check out AnimeGameData at the manifest's exact pinned commit and locate the phrase in the appropriate `TextMap/TextMap*.json` file.
2. Add the raw value to the matching key in all affected culture catalogs, preserving key parity across all six catalogs.
3. Add one matching provenance record per raw alias and culture. Use the exact TextMap hash when the value is present verbatim; otherwise use `source: "curated"` with a specific reason.
4. Update the pin only as a deliberate repository-wide source update: change the verifier and manifest together, revalidate every TextMap-backed alias, review the resulting catalog diff, and rerun all checks below.

Curated variants should be narrow evidence-based OCR spellings, shortened UI fragments, or compatibility aliases. Record what screen/flow or legacy behavior requires the variant. Do not add speculative translations or use `curated` to conceal a TextMap mismatch.

## Culture resolution and failures

The configured game language supplies the default culture; matching does not depend on the process thread culture. An explicitly passed C# culture overrides that default.

Catalog lookup first tries the exact culture name, then its neutral parent. For example, `fr-CA` may use an available `fr` catalog. There is no cross-language or arbitrary default fallback. If neither catalog exists, matching throws `InvalidOperationException`. An unknown key throws `KeyNotFoundException` containing the key and requested culture. Missing culture configuration, null collections, blank semantic keys, and invalid JavaScript collection values also fail explicitly rather than silently returning a misleading non-match.

## C# APIs

Inject `IGameTextMatcher` into task code and use `GameTextKeys` constants:

```csharp
public sealed class ResinPanel(IGameTextMatcher gameTextMatcher)
{
    public bool IsOriginalResin(string rawOcrText) =>
        gameTextMatcher.IsMatch(rawOcrText, GameTextKeys.Resin.Original);

    public bool AnyLineIsOriginalResin(IEnumerable<string> rawOcrLines) =>
        gameTextMatcher.IsAnyMatch(rawOcrLines, GameTextKeys.Resin.Original);

    public bool SplitLinesFormLabel(IEnumerable<string> rawOcrFragments) =>
        gameTextMatcher.IsCombinedMatch(rawOcrFragments, GameTextKeys.Resin.Original);
}
```

`GetAliases` exposes the culture's raw aliases for recognizers that must snapshot or compose them. Prefer `IsMatch`, `IsAnyMatch`, or a focused recognizer when a direct semantic decision is sufficient.

BgiVision resolves semantic keys when the locator is created and preserves the raw `Region.Text` in results:

```csharp
var page = new BvPage(cancellationToken);

Region resin = (await page
    .GetByTextKey(GameTextKeys.Resin.Original, panelRect)
    .WaitFor()).First();

Region action = (await page
    .GetByAnyTextKey(
        new[] { GameTextKeys.Common.Use, GameTextKeys.Common.Confirm },
        panelRect)
    .WaitFor()).First();

string rawOcrText = resin.Text;
```

Use `GetByTextKey` for one meaning and `GetByAnyTextKey` when any of several semantic meanings is acceptable. Invalid or unknown keys fail when the locator is constructed. Literal `GetByText`/`GetByAnyText` calls are intentionally unchanged for language-independent strings and compatibility.

## JavaScript API

Downloaded scripts receive the restricted read-only `gameText` host object:

```javascript
const aliases = gameText.aliases("resin.original");
const isResin = gameText.matches(rawOcrText, "resin.original");
const anyResin = gameText.matchesAny(
  ["Vida", "Resina Original"],
  "resin.original"
);
log.info(`Configured game culture: ${gameText.culture}`);
```

- `aliases(key)` returns a JavaScript array containing the configured culture's raw aliases.
- `matches(text, key)` applies the shared normalized contains comparison.
- `matchesAny(texts, key)` requires a nonempty array/collection of strings; scalar or mixed values throw `ArgumentException`.
- `culture` reports the configured game culture and cannot be changed by scripts.

The same catalog/key and culture failures described above are surfaced to scripts. BgiVision's literal script APIs remain compatible.

## Repeatable validation

Run these commands from the repository root. Catalog provenance requires a separate AnimeGameData checkout at the pinned commit:

```powershell
rtk pwsh -NoProfile -File scripts/game-text/Verify-GameTextCatalog.ps1 `
  -AnimeGameDataPath C:\src\AnimeGameData
```

Run the audit's fail-closed regression fixtures, then the final source audit:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass `
  -File scripts/game-text/Test-Audit-LanguageDependentRecognition.ps1
rtk pwsh -NoProfile -File scripts/game-text/Audit-LanguageDependentRecognition.ps1
```

Run the OCR/catalog and focused automation tests, then the complete unit suite:

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj `
  -c Release -p:Platform=x64 `
  --filter "FullyQualifiedName~GameText|FullyQualifiedName~PaddleOcrServiceTests|FullyQualifiedName~AutoDomainTests|FullyQualifiedName~AutoBossTests|FullyQualifiedName~AutoLeyLineOutcropTests|FullyQualifiedName~AutoStygianOnslaughtTests|FullyQualifiedName~AutoSkipTests"

rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj `
  -c Release -p:Platform=x64
```

Build the requested Windows x64 Release solution and verify the executable under the Release x64 output tree:

```powershell
rtk dotnet build BetterGenshinImpact.sln -c Release -p:Platform=x64
```

Any full-suite failure must be investigated and reproduced at the exact stacked base in an isolated worktree/output before it is classified as unrelated to the branch.

## Stacked pull request workflow

This change is stacked on the Portuguese UI-localization branch. While PR #2 is open, keep the feature PR based on `i18n/pt-br-completo`; compare with `i18n/pt-br-completo...HEAD` and do not merge upstream `main` into the feature branch, because that pollutes the stacked diff.

After PR #2 merges, fetch the updated remote, move the stack onto current `main` using the repository's agreed rebase/retarget workflow, inspect the new base/head diff, and rerun the catalog verifier, audit regression, final audit, focused tests, full unit suite, and Windows x64 Release build. Resolve conflicts as semantic catalog changes rather than accepting generated JSON wholesale. Publish or update a single draft feature PR, record its dependency and validation evidence, inspect remote checks, and leave merging and readiness decisions to human review. Never enable automatic merge for this stack.
