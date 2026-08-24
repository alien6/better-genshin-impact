# PT-BR OCR, Automation, and Script Localization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make BetterGI OCR-dependent first-party automations and scripts work with Genshin Impact configured for Portuguese (Brazil), while preserving existing Chinese, English, Traditional Chinese, French, and Japanese behavior.

**Architecture:** Add a central, immutable semantic game-text catalog and matcher. Resolve the configured game culture at the matching boundary, normalize OCR and aliases without changing raw OCR output, expose semantic-key operations through BgiVision and ClearScript, then migrate each first-party OCR decision behind focused recognizer classes. Keep literal/internal protocol strings outside this layer and record that classification in an auditable allowlist.

**Tech Stack:** .NET 8, C# 12, WPF, Microsoft.Extensions.DependencyInjection, Newtonsoft.Json, PaddleOCR/ONNX, OpenCvSharp, Microsoft.ClearScript V8, xUnit 2.5.3, PowerShell, Git/GitHub CLI.

**Spec:** `Docs/superpowers/specs/2026-08-21-pt-br-ocr-automation-design.md`

## Global Constraints

- Work only on `feat/pt-br-ocr-automation`, stacked on `i18n/pt-br-completo` while PR #2 is open.
- Do not merge either pull request. If PR #2 merges, retarget this branch's PR to `main` only after checking the resulting diff and checks.
- Follow red-green-refactor for every production behavior change: add one focused failing test, run it and inspect the expected failure, implement the minimum behavior, run it green, then refactor.
- Preserve raw OCR output. Localization occurs only in semantic matching APIs.
- Never change `CurrentCulture` or `CurrentUICulture` to resolve game aliases. Do not use `CultureHelper.WithCultureGet` in the new layer.
- Use FormKC, invariant lowercase, diacritic removal, and removal of whitespace, punctuation, and symbols for matching. Preserve letters, ideographs, and digits. Do not add edit-distance or unbounded fuzzy matching.
- Resolve an exact culture first and then its neutral parent. Do not fall back across unrelated languages.
- Keep catalogs immutable and aliases pre-normalized after startup; do not normalize catalogs in OCR retry loops.
- Use Newtonsoft.Json for catalog parsing to match the repository's established dependency.
- Preserve existing public literal-text BgiVision and script APIs. Semantic APIs are additive.
- Do not translate internal command names, combat-script syntax, character IDs, route names, log protocols, launcher window titles, or text sent into the game.
- Stage exact files only. Never use `git add .` or `git add -A`.
- At each review gate, run `git diff --check` and inspect `git status --short` before committing.

---

## Task 1: Build the normalization and semantic matching core

**Files:**

- Create: `BetterGenshinImpact/GameTask/Localization/IGameTextMatcher.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/IGameCultureProvider.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/GameTextNormalizer.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/GameTextCatalog.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/IGameTextCatalogProvider.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/EmbeddedGameTextCatalogProvider.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/GameTextMatcher.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextNormalizerTests.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextMatcherTests.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextTestFactory.cs`

- [ ] **Step 1: Write normalization tests first**

Cover compatibility composition, case, accents, spaces, punctuation, digits, ideographs, and empty input:

```csharp
[Theory]
[InlineData("Resina Original", "resinaoriginal")]
[InlineData("  EXPEDIÇÃO! ", "expedicao")]
[InlineData("Síntese", "sintese")]
[InlineData("２× Recompensa", "2recompensa")]
[InlineData("地脉 异常", "地脉异常")]
[InlineData("", "")]
public void Normalize_ReturnsStableComparableText(string input, string expected)
{
    Assert.Equal(expected, GameTextNormalizer.Normalize(input));
}
```

- [ ] **Step 2: Run the focused normalization test and confirm RED**

Run:

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter FullyQualifiedName~GameTextNormalizerTests
```

Expected: compilation fails because `GameTextNormalizer` does not exist.

- [ ] **Step 3: Implement the normalizer**

Implement FormKC before FormD so full-width digits normalize, then filter Unicode categories:

```csharp
internal static class GameTextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var decomposed = value.Normalize(NormalizationForm.FormKC)
            .Normalize(NormalizationForm.FormD)
            .ToLowerInvariant();
        var builder = new StringBuilder(decomposed.Length);
        foreach (var rune in decomposed.EnumerateRunes())
        {
            var category = Rune.GetUnicodeCategory(rune);
            if (category is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            if (Rune.IsLetterOrDigit(rune))
            {
                builder.Append(rune.ToString());
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool Contains(string? recognizedText, string normalizedAlias) =>
        normalizedAlias.Length > 0 && Normalize(recognizedText).Contains(normalizedAlias, StringComparison.Ordinal);
}
```

- [ ] **Step 4: Run normalization tests and confirm GREEN**

- [ ] **Step 5: Write matcher contract tests first**

Use in-memory real catalogs, not mocked match results. Cover:

```csharp
[Fact]
public void IsMatch_NormalizesPortugueseOcrNoise()
{
    var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));
    Assert.True(sut.IsMatch("  RESINA\nORIGINAL! ", "resin.original"));
}

[Fact]
public void IsAnyMatch_MatchesOneOcrRegion()
{
    var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));
    Assert.True(sut.IsAnyMatch(["Vida", "Resina Original"], "resin.original"));
}

[Fact]
public void IsCombinedMatch_MatchesWordsSplitAcrossRegions()
{
    var sut = GameTextTestFactory.Create("pt-BR", ("domain.challenge_completed", "Desafio concluído"));
    Assert.True(sut.IsCombinedMatch(["Desafio", "concluído"], "domain.challenge_completed"));
}

[Fact]
public void EmptyOcr_ReturnsFalse()
{
    var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));
    Assert.False(sut.IsMatch("", "resin.original"));
    Assert.False(sut.IsAnyMatch(["", "  "], "resin.original"));
    Assert.False(sut.IsCombinedMatch(["", "  "], "resin.original"));
}

[Fact]
public void UnknownKey_ThrowsKeyNotFoundException()
{
    var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));
    var error = Assert.Throws<KeyNotFoundException>(() => sut.GetAliases("missing.key"));
    Assert.Contains("missing.key", error.Message);
    Assert.Contains("pt-BR", error.Message);
}

[Fact]
public void MissingCulture_ThrowsInvalidOperationException()
{
    var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));
    Assert.Throws<InvalidOperationException>(() => sut.GetAliases("resin.original", new CultureInfo("de-DE")));
}

[Fact]
public void SpecificCulture_FallsBackOnlyToNeutralParent()
{
    var sut = GameTextTestFactory.CreateForCultures(
        "pt-BR",
        ("pt", "resin.original", "Resina"),
        ("en", "resin.original", "Original Resin"));
    Assert.True(sut.IsMatch("Resina", "resin.original", new CultureInfo("pt-PT")));
    Assert.False(sut.IsMatch("Original Resin", "resin.original", new CultureInfo("pt-PT")));
}

[Fact]
public void Matcher_DoesNotChangeThreadCultures()
{
    var beforeCulture = CultureInfo.CurrentCulture;
    var beforeUiCulture = CultureInfo.CurrentUICulture;
    var sut = GameTextTestFactory.Create("pt-BR", ("resin.original", "Resina Original"));
    _ = sut.IsMatch("Resina Original", "resin.original");
    Assert.Same(beforeCulture, CultureInfo.CurrentCulture);
    Assert.Same(beforeUiCulture, CultureInfo.CurrentUICulture);
}
```

- [ ] **Step 6: Run matcher tests and confirm RED**

- [ ] **Step 7: Implement immutable catalogs and matcher**

Use the approved public contract exactly:

```csharp
public interface IGameTextMatcher
{
    IReadOnlyList<string> GetAliases(string key, CultureInfo? culture = null);
    bool IsMatch(string recognizedText, string key, CultureInfo? culture = null);
    bool IsAnyMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null);
    bool IsCombinedMatch(IEnumerable<string> recognizedTexts, string key, CultureInfo? culture = null);
}
```

`EmbeddedGameTextCatalogProvider` must parse every embedded `*.json` once, validate `schemaVersion == 1`, validate the filename culture against the document culture, reject empty/duplicate normalized aliases, and publish frozen/read-only dictionaries. `GameTextMatcher` must resolve the passed culture or `IGameCultureProvider.CurrentCulture`, exact name first and neutral parent second. It must look up normalized aliases once and use ordinal containment for OCR text.

- [ ] **Step 8: Run both focused test classes and confirm GREEN**

- [ ] **Step 9: Review and commit**

```powershell
rtk git diff --check
rtk git status --short
rtk git add BetterGenshinImpact/GameTask/Localization/IGameTextMatcher.cs BetterGenshinImpact/GameTask/Localization/IGameCultureProvider.cs BetterGenshinImpact/GameTask/Localization/GameTextNormalizer.cs BetterGenshinImpact/GameTask/Localization/GameTextCatalog.cs BetterGenshinImpact/GameTask/Localization/IGameTextCatalogProvider.cs BetterGenshinImpact/GameTask/Localization/EmbeddedGameTextCatalogProvider.cs BetterGenshinImpact/GameTask/Localization/GameTextMatcher.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextNormalizerTests.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextMatcherTests.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextTestFactory.cs
rtk git commit -m "feat: add semantic game text matcher"
```

---

## Task 2: Add stable keys, multilingual catalogs, and source provenance

**Files:**

- Create: `BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/en.json`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json`
- Create: `BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json`
- Modify: `BetterGenshinImpact/BetterGenshinImpact.csproj`
- Modify: `Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj`
- Create: `scripts/game-text/Verify-GameTextCatalog.ps1`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextCatalogTests.cs`

- [ ] **Step 1: Write catalog contract tests first**

Tests must assert:

1. required cultures are exactly `zh-Hans`, `zh-Hant`, `en`, `ja`, `fr`, `pt-BR`;
2. every `GameTextKeys.All` key exists in every required catalog;
3. every entry has at least one alias whose normalized form is non-empty;
4. normalized aliases within an entry are unique;
5. every key has a `source-manifest.json` record;
6. the manifest baseline is exactly AnimeGameData commit `26df1dfbdf05a82bbb1d97506859f3e1c40718d8`;
7. `pt-BR` includes `Resina Original`, `Desafio concluído`, `Expedição`, `Sintetizar`, and `Reviver` under their semantic keys.

- [ ] **Step 2: Run `GameTextCatalogTests` and confirm RED**

- [ ] **Step 3: Add the initial key set and catalogs**

Use `System.Collections.Frozen` and nested constant groups while keeping the underlying dotted strings stable:

```csharp
public static class GameTextKeys
{
    public static class Common
    {
        public const string Revive = "common.revive";
        public const string Confirm = "common.confirm";
        public const string Cancel = "common.cancel";
        public const string Use = "common.use";
        public const string ClickAnywhereToClose = "common.click_anywhere_to_close";
    }

    public static class Resin
    {
        public const string Original = "resin.original";
        public const string Condensed = "resin.condensed";
        public const string Fragile = "resin.fragile";
        public const string Transient = "resin.transient";
        public const string Replenish = "resin.replenish";
        public const string Insufficient = "resin.insufficient";
    }

    public static IReadOnlySet<string> All { get; } = new[]
    {
        Common.Revive,
        Common.Confirm,
        Common.Cancel,
        Common.Use,
        Common.ClickAnywhereToClose,
        Resin.Original,
        Resin.Condensed,
        Resin.Fragile,
        Resin.Transient,
        Resin.Replenish,
        Resin.Insufficient
    }.ToFrozenSet(StringComparer.Ordinal);
}
```

Add feature groups as Tasks 7-14 introduce them. Every production key addition in those tasks must update all six catalogs and the source manifest in the same commit.

Catalog schema:

```json
{
  "schemaVersion": 1,
  "culture": "pt-BR",
  "entries": {
    "resin.original": ["Resina Original"]
  }
}
```

The source manifest must record `textMapHash` for TextMap-backed strings and `source: "curated"` plus a short reason for UI/OCR variants not present in TextMap. Do not copy whole TextMaps into BetterGI.

- [ ] **Step 4: Embed only runtime catalogs**

Add:

```xml
<ItemGroup>
  <EmbeddedResource Include="GameTask\Localization\Catalogs\*.json"
                    Exclude="GameTask\Localization\Catalogs\source-manifest.json" />
</ItemGroup>
```

Link `source-manifest.json` into the unit-test output without shipping it as a runtime catalog:

```xml
<None Include="..\..\BetterGenshinImpact\GameTask\Localization\Catalogs\source-manifest.json"
      Link="Assets\GameText\source-manifest.json"
      CopyToOutputDirectory="PreserveNewest" />
```

- [ ] **Step 5: Add an offline provenance verifier**

`Verify-GameTextCatalog.ps1` accepts `-AnimeGameDataPath`, verifies that checkout's HEAD is the pinned commit, loads `TextMapCHS/CHT/EN/JP/FR/PT`, and checks each manifest hash/value against the committed aliases. It must never download data or mutate catalogs.

Run when the pinned checkout is available:

```powershell
rtk pwsh scripts/game-text/Verify-GameTextCatalog.ps1 -AnimeGameDataPath C:\src\AnimeGameData
```

- [ ] **Step 6: Run catalog and matcher tests and confirm GREEN**

- [ ] **Step 7: Review and commit**

```powershell
rtk git diff --check
rtk git add BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json BetterGenshinImpact/BetterGenshinImpact.csproj Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj scripts/game-text/Verify-GameTextCatalog.ps1 Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextCatalogTests.cs
rtk git commit -m "feat: add multilingual game text catalogs"
```

---

## Task 3: Bind the matcher to the configured game culture through DI

**Files:**

- Create: `BetterGenshinImpact/GameTask/Localization/ConfiguredGameCultureProvider.cs`
- Create: `BetterGenshinImpact/GameTask/Localization/GameTextServiceCollectionExtensions.cs`
- Modify: `BetterGenshinImpact/App.xaml.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/ConfiguredGameCultureProviderTests.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextServiceCollectionExtensionsTests.cs`

- [ ] **Step 1: Write RED tests for live culture selection and singleton registration**

Use a mutable string delegate so tests do not mutate `TaskContext`:

```csharp
var configured = "pt-BR";
var sut = new ConfiguredGameCultureProvider(() => configured);
Assert.Equal("pt-BR", sut.CurrentCulture.Name);
configured = "en-US";
Assert.Equal("en-US", sut.CurrentCulture.Name);
```

Also assert invalid configured names throw `InvalidOperationException` containing the invalid name, and that `AddGameTextLocalization(() => configured)` resolves one singleton `IGameTextMatcher` and one singleton catalog provider.

- [ ] **Step 2: Run focused tests and confirm RED**

- [ ] **Step 3: Implement provider and registration extension**

The app registration must read the live configuration on each lookup without mutating thread culture:

```csharp
services.AddGameTextLocalization(
    () => configService.Get().OtherConfig.GameCultureInfoName);
```

Register the embedded provider and matcher as singletons. Call the extension immediately after `services.AddLocalization()` in `App.xaml.cs`.

- [ ] **Step 4: Run focused tests and confirm GREEN**

- [ ] **Step 5: Commit**

```powershell
rtk git add BetterGenshinImpact/GameTask/Localization/ConfiguredGameCultureProvider.cs BetterGenshinImpact/GameTask/Localization/GameTextServiceCollectionExtensions.cs BetterGenshinImpact/App.xaml.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/ConfiguredGameCultureProviderTests.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextServiceCollectionExtensionsTests.cs
rtk git commit -m "feat: bind game text matcher to configured culture"
```

---

## Task 4: Add semantic-key operations to BgiVision

**Files:**

- Modify: `BetterGenshinImpact/Core/BgiVision/BvPage.cs`
- Modify: `BetterGenshinImpact/Core/BgiVision/BvLocator.cs`
- Modify: `BetterGenshinImpact/Core/BgiVision/BvFlow.cs`
- Modify: `BetterGenshinImpact/Core/BgiVision/BvFlowAction.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/CoreTests/BgiVisionTests/BvGameTextTests.cs`

- [ ] **Step 1: Write RED tests for locator resolution and diagnostics**

Construct `BvPage` with a real test matcher and assert:

```csharp
var locator = page.GetByTextKey(GameTextKeys.Resin.Original);
Assert.Equal(GameTextKeys.Resin.Original, locator.GameTextKey);
Assert.Contains("Resina Original", locator.AnyTexts);
Assert.True(locator.MatchesOcrText("RESINA-ORIGINAL"));
Assert.Contains("resin.original", locator.DescribeTarget());
Assert.Contains("Resina Original", locator.DescribeTarget());
```

Also test `GetByAnyTextKey` de-duplicates aliases, unknown keys fail immediately, `WaitUntilTextKey`/`WaitUntilAnyTextKey` construct flow steps, and all existing literal APIs retain ordinal literal behavior.

- [ ] **Step 2: Run `BvGameTextTests` and confirm RED**

- [ ] **Step 3: Implement additive BgiVision APIs**

Add an injectable constructor while preserving the existing call shape:

```csharp
public BvPage(CancellationToken cancellationToken = default, IGameTextMatcher? gameTextMatcher = null)
{
    _cancellationToken = cancellationToken;
    _gameTextMatcher = gameTextMatcher;
}

private IGameTextMatcher GameTextMatcher => _gameTextMatcher ??=
    App.GetService<IGameTextMatcher>()
    ?? throw new InvalidOperationException("IGameTextMatcher is not registered.");

public BvLocator GetByTextKey(string key, Rect rect = default);
public BvLocator GetByAnyTextKey(object keys, Rect rect = default);
```

Add to both `BvFlow` and `BvFlowAction`:

```csharp
public BvFlow WaitUntilTextKey(string key, Rect rect = default, int? timeout = null, int? retryInterval = null);
public BvFlow WaitUntilAnyTextKey(object keys, Rect rect = default, int? timeout = null, int? retryInterval = null);
```

Resolve `GameTextMatcher` only from semantic methods so `new BvPage()` and every existing literal API continue to work before application DI is available. `BvLocator` must store the semantic key(s), raw aliases, and pre-normalized aliases. Clone all three. Semantic locators use normalized containment; literal locators preserve their current ordinal containment. Timeout descriptions for semantic locators include both key and aliases.

- [ ] **Step 4: Run BgiVision tests plus existing BgiVision-related tests and confirm GREEN**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~BvGameTextTests|FullyQualifiedName~BgiVision"
```

- [ ] **Step 5: Commit**

```powershell
rtk git add BetterGenshinImpact/Core/BgiVision/BvPage.cs BetterGenshinImpact/Core/BgiVision/BvLocator.cs BetterGenshinImpact/Core/BgiVision/BvFlow.cs BetterGenshinImpact/Core/BgiVision/BvFlowAction.cs Test/BetterGenshinImpact.UnitTest/CoreTests/BgiVisionTests/BvGameTextTests.cs
rtk git commit -m "feat: add semantic text APIs to BgiVision"
```

---

## Task 5: Expose semantic matching to JavaScript

**Files:**

- Create: `BetterGenshinImpact/Core/Script/Dependence/GameTextScriptApi.cs`
- Modify: `BetterGenshinImpact/Core/Script/EngineExtend.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/CoreTests/ScriptTests/GameTextScriptApiTests.cs`

- [ ] **Step 1: Write RED tests for the direct API and ClearScript bridge**

Cover the exact JavaScript surface:

```javascript
gameText.aliases("resin.original")
gameText.matches("RESINA ORIGINAL", "resin.original")
gameText.matchesAny(["Vida", "Resina Original"], "resin.original")
gameText.culture
```

Use a real `GameTextMatcher`. For the bridge test, create `V8ScriptEngine`, call a small internal `EngineExtend.AddGameTextHostObject(engine, matcher)` method, and evaluate the expressions above. Do not initialize unrelated script host objects.

- [ ] **Step 2: Run focused tests and confirm RED**

- [ ] **Step 3: Implement the script facade and registration**

Return a `string[]` from `aliases` for reliable ClearScript marshaling. `matchesAny` must use `BvPage.ParseCollection<string>` or an equivalent validated conversion so empty/non-string arrays produce descriptive argument errors. `culture` returns the effective game-culture name.

```csharp
public sealed class GameTextScriptApi(
    IGameTextMatcher matcher,
    IGameCultureProvider cultureProvider)
{
    public string[] aliases(string key) => matcher.GetAliases(key).ToArray();

    public bool matches(string recognizedText, string key) =>
        matcher.IsMatch(recognizedText, key);

    public bool matchesAny(object recognizedTexts, string key) =>
        matcher.IsAnyMatch(BvPage.ParseCollection<string>(recognizedTexts, nameof(recognizedTexts)), key);

    public string culture => cultureProvider.CurrentCulture.Name;
}
```

Call `AddGameTextHostObject` from `InitHost`; preserve all existing host objects.

- [ ] **Step 4: Run focused tests and confirm GREEN**

- [ ] **Step 5: Commit**

```powershell
rtk git add BetterGenshinImpact/Core/Script/Dependence/GameTextScriptApi.cs BetterGenshinImpact/Core/Script/EngineExtend.cs Test/BetterGenshinImpact.UnitTest/CoreTests/ScriptTests/GameTextScriptApiTests.cs
rtk git commit -m "feat: expose localized game text to scripts"
```

---

## Task 6: Prove the Portuguese PaddleOCR path with a real fixture

**Files:**

- Modify: `Test/BetterGenshinImpact.UnitTest/CoreTests/RecognitionTests/OCRTests/PaddleOcrServiceTests.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/Assets/OCR/pt-BR-resina-original.png`
- Modify: `Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj`
- Modify only if the RED test proves a defect: `BetterGenshinImpact/Core/Recognition/OCR/Paddle/PaddleOcrService.cs`

- [ ] **Step 1: Add a model-selection test**

```csharp
[Fact]
public void FromCultureInfo_PtBr_UsesV5Latin()
{
    Assert.Same(
        PaddleOcrService.PaddleOcrModelType.V5Latin,
        PaddleOcrService.PaddleOcrModelType.FromCultureInfo(new CultureInfo("pt-BR")));
}
```

- [ ] **Step 2: Add and commit the fixture source image**

Create a deterministic white-background, black-text PNG containing `Resina Original` using the same Arial rendering parameters as the existing OCR theory. Commit the rendered PNG so OCR regressions do not depend on installed-font changes. Copy it to the test output directory from the test project.

- [ ] **Step 3: Write the fixture OCR test**

```csharp
[Fact]
public void PaddleOcrService_PtBrFixture_RecognizesPortugueseText()
{
    using var mat = Cv2.ImRead(Path.Combine(AppContext.BaseDirectory, "Assets", "OCR", "pt-BR-resina-original.png"));
    var actual = paddle.Get("pt-BR").Ocr(mat);
    Assert.Matches("(?i)Resina\\s*Original", actual);
}
```

- [ ] **Step 4: Run both tests**

Expected model-selection result: GREEN with current code. The fixture test establishes the real integration baseline. If it is RED, record the actual OCR output before making the smallest model/configuration correction; do not loosen the assertion beyond benign whitespace/case variation.

- [ ] **Step 5: Run all Paddle OCR tests and commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter FullyQualifiedName~PaddleOcrServiceTests
rtk git add Test/BetterGenshinImpact.UnitTest/CoreTests/RecognitionTests/OCRTests/PaddleOcrServiceTests.cs Test/BetterGenshinImpact.UnitTest/Assets/OCR/pt-BR-resina-original.png Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj
rtk git commit -m "test: cover Portuguese Paddle OCR"
```

If production code changed, stage that exact file and use `fix: select Latin OCR model for pt-BR` instead.

---

## Task 7: Migrate shared BgiVision status and common jobs

**Files:**

- Create: `BetterGenshinImpact/GameTask/Common/GameText/CommonJobTextRecognizer.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/CommonTests/CommonJobTextRecognizerTests.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/BgiVision/BvStatus.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/GoToCraftingBenchTask.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/GoToAdventurersGuildTask.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/ClaimEncounterPointsRewardsTask.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/ClaimBattlePassRewardsTask.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/CheckRewardsTask.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/GoToSereniteaPotTask.cs`
- Delete after usages are gone: the four culture `.resx` files for `BvResxHelper`, `GoToCraftingBenchTask`, `GoToAdventurersGuildTask`, `ClaimEncounterPointsRewardsTask`, `ClaimBattlePassRewardsTask`, and `CheckRewardsTask`
- Modify: all six catalog JSON files, `GameTextKeys.cs`, and `source-manifest.json`

- [ ] **Step 1: Add RED recognizer tests for every common-job decision**

Add Portuguese and one legacy-language case for: revive, synthesize/crafting, Katheryne, daily commissions, expeditions, claim all (combined words), today's reward already claimed, Tubby/teapot spirit, trust rank, and Realm Depot. Include a negative case such as `Resina Original` not matching `common.revive`.

- [ ] **Step 2: Add semantic keys and all six culture aliases; run RED until recognizer exists**

- [ ] **Step 3: Implement `CommonJobTextRecognizer` and migrate consumers**

Inject or resolve `IGameTextMatcher` once per task instance. Replace only OCR-result comparisons. Use `IsCombinedMatch` where UI words are split across OCR regions. Preserve NPC interaction strings that are typed/sent or used as internal identifiers.

- [ ] **Step 4: Remove migrated localizer fields and resource files**

Before deletion, verify every alias from each `.resx` is represented in the matching culture catalog. Then confirm:

```powershell
rtk rg -n "IStringLocalizer|WithCultureGet" BetterGenshinImpact/GameTask/Common/BgiVision/BvStatus.cs BetterGenshinImpact/GameTask/Common/Job
```

Expected: no migrated recognition usage in the listed files.

- [ ] **Step 5: Run focused tests and common-job tests; commit**

```powershell
rtk git diff --check
rtk git add -- BetterGenshinImpact/GameTask/Common/GameText/CommonJobTextRecognizer.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/CommonTests/CommonJobTextRecognizerTests.cs BetterGenshinImpact/GameTask/Common/BgiVision/BvStatus.cs BetterGenshinImpact/GameTask/Common/Job/GoToCraftingBenchTask.cs BetterGenshinImpact/GameTask/Common/Job/GoToAdventurersGuildTask.cs BetterGenshinImpact/GameTask/Common/Job/ClaimEncounterPointsRewardsTask.cs BetterGenshinImpact/GameTask/Common/Job/ClaimBattlePassRewardsTask.cs BetterGenshinImpact/GameTask/Common/Job/CheckRewardsTask.cs BetterGenshinImpact/GameTask/Common/Job/GoToSereniteaPotTask.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/GameTextCatalogTests.cs
rtk git add -- BetterGenshinImpact/GameTask/Common/BgiVision/BvResxHelper.zh-Hans.resx BetterGenshinImpact/GameTask/Common/BgiVision/BvResxHelper.zh-Hant.resx BetterGenshinImpact/GameTask/Common/BgiVision/BvResxHelper.en.resx BetterGenshinImpact/GameTask/Common/BgiVision/BvResxHelper.fr.resx BetterGenshinImpact/GameTask/Common/Job/GoToCraftingBenchTask.zh-Hans.resx BetterGenshinImpact/GameTask/Common/Job/GoToCraftingBenchTask.zh-Hant.resx BetterGenshinImpact/GameTask/Common/Job/GoToCraftingBenchTask.en.resx BetterGenshinImpact/GameTask/Common/Job/GoToCraftingBenchTask.fr.resx BetterGenshinImpact/GameTask/Common/Job/GoToAdventurersGuildTask.zh-Hans.resx BetterGenshinImpact/GameTask/Common/Job/GoToAdventurersGuildTask.zh-Hant.resx BetterGenshinImpact/GameTask/Common/Job/GoToAdventurersGuildTask.en.resx BetterGenshinImpact/GameTask/Common/Job/GoToAdventurersGuildTask.fr.resx BetterGenshinImpact/GameTask/Common/Job/ClaimEncounterPointsRewardsTask.zh-Hans.resx BetterGenshinImpact/GameTask/Common/Job/ClaimEncounterPointsRewardsTask.zh-Hant.resx BetterGenshinImpact/GameTask/Common/Job/ClaimEncounterPointsRewardsTask.en.resx BetterGenshinImpact/GameTask/Common/Job/ClaimEncounterPointsRewardsTask.fr.resx BetterGenshinImpact/GameTask/Common/Job/ClaimBattlePassRewardsTask.zh-Hans.resx BetterGenshinImpact/GameTask/Common/Job/ClaimBattlePassRewardsTask.zh-Hant.resx BetterGenshinImpact/GameTask/Common/Job/ClaimBattlePassRewardsTask.en.resx BetterGenshinImpact/GameTask/Common/Job/ClaimBattlePassRewardsTask.fr.resx BetterGenshinImpact/GameTask/Common/Job/CheckRewardsTask.zh-Hans.resx BetterGenshinImpact/GameTask/Common/Job/CheckRewardsTask.zh-Hant.resx BetterGenshinImpact/GameTask/Common/Job/CheckRewardsTask.en.resx BetterGenshinImpact/GameTask/Common/Job/CheckRewardsTask.fr.resx
rtk git commit -m "feat: localize common OCR job matching"
```

---

## Task 8: Migrate the existing resource-backed fishing, pathing, and artifact flows

**Files:**

- Create: `BetterGenshinImpact/GameTask/AutoFishing/FishingTextRecognizer.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoFishing/AutoFishingTaskParam.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoFishing/Behaviours.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoFishing/Behaviours.PartII.cs`
- Create: `BetterGenshinImpact/GameTask/AutoTrackPath/TrackPathTextRecognizer.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoTrackPath/TpTaskParam.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoTrackPath/TpTask.cs`
- Create: `BetterGenshinImpact/GameTask/AutoArtifactSalvage/ArtifactTextRecognizer.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTaskParam.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTask.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoFishingTests/FishingTextRecognizerTests.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoTrackPathTests/TrackPathTextRecognizerTests.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoArtifactSalvageTests/ArtifactTextRecognizerTests.cs`
- Modify: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoArtifactSalvageTests/AutoArtifactSalvageTaskTests.cs`
- Delete after migration: the four culture `.resx` files for `AutoFishingTask`, `TpTask`, and `AutoArtifactSalvageTask`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Inventory the exact current resource values and add catalog coverage tests**

Map every resource entry one-to-one before deleting it. Add Portuguese aliases sourced from `TextMapPT`; keep curated OCR regex variants only when existing tests or screenshots demonstrate them.

- [ ] **Step 2: Add RED behavior tests**

Extend the real AutoArtifactSalvage OCR/stat tests with `pt-BR` main-stat and star-label cases. Add focused fishing/path tests that pass OCR text through the real matcher. Preserve the public parameter constructors used by scripts; if removing `StringLocalizer` would break a constructor, retain a compatibility overload that delegates to the new implementation.

- [ ] **Step 3: Migrate production matching and run GREEN tests**

Use semantic matcher operations only for OCR-derived text. Do not migrate fish names, route labels, or artifact enum/config identifiers unless they originate from OCR in the affected branch.

- [ ] **Step 4: Remove unused resource files and localizer plumbing**

Run:

```powershell
rtk rg -n "IStringLocalizer|WithCultureGet" BetterGenshinImpact/GameTask/AutoFishing BetterGenshinImpact/GameTask/AutoTrackPath BetterGenshinImpact/GameTask/AutoArtifactSalvage
```

Expected: no recognition-localization usage in those directories.

- [ ] **Step 5: Commit**

```powershell
rtk git diff --check
rtk git add -- BetterGenshinImpact/GameTask/AutoFishing/FishingTextRecognizer.cs BetterGenshinImpact/GameTask/AutoFishing/AutoFishingTaskParam.cs BetterGenshinImpact/GameTask/AutoFishing/Behaviours.cs BetterGenshinImpact/GameTask/AutoFishing/Behaviours.PartII.cs BetterGenshinImpact/GameTask/AutoTrackPath/TrackPathTextRecognizer.cs BetterGenshinImpact/GameTask/AutoTrackPath/TpTaskParam.cs BetterGenshinImpact/GameTask/AutoTrackPath/TpTask.cs BetterGenshinImpact/GameTask/AutoArtifactSalvage/ArtifactTextRecognizer.cs BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTaskParam.cs BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTask.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoFishingTests/FishingTextRecognizerTests.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoTrackPathTests/TrackPathTextRecognizerTests.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoArtifactSalvageTests/ArtifactTextRecognizerTests.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoArtifactSalvageTests/AutoArtifactSalvageTaskTests.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json
rtk git add -- BetterGenshinImpact/GameTask/AutoFishing/AutoFishingTask.zh-Hans.resx BetterGenshinImpact/GameTask/AutoFishing/AutoFishingTask.zh-Hant.resx BetterGenshinImpact/GameTask/AutoFishing/AutoFishingTask.en.resx BetterGenshinImpact/GameTask/AutoFishing/AutoFishingTask.fr.resx BetterGenshinImpact/GameTask/AutoTrackPath/TpTask.zh-Hans.resx BetterGenshinImpact/GameTask/AutoTrackPath/TpTask.zh-Hant.resx BetterGenshinImpact/GameTask/AutoTrackPath/TpTask.en.resx BetterGenshinImpact/GameTask/AutoTrackPath/TpTask.fr.resx BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTask.zh-Hans.resx BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTask.zh-Hant.resx BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTask.en.resx BetterGenshinImpact/GameTask/AutoArtifactSalvage/AutoArtifactSalvageTask.fr.resx
rtk git commit -m "feat: migrate resource-backed OCR flows"
```

---

## Task 9: Migrate domain recognition

**Files:**

- Create: `BetterGenshinImpact/GameTask/AutoDomain/DomainTextRecognizer.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoDomainTests/DomainTextRecognizerTests.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.cs`
- Delete after migration: `BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.zh-Hans.resx`
- Delete after migration: `BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.zh-Hant.resx`
- Delete after migration: `BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.en.resx`
- Delete after migration: `BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.fr.resx`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Write RED tests for every domain OCR decision**

Cover challenge completed, auto leave, skip, ley-line disorder, click anywhere to close, quick select, two-star artifact, petrified tree, resin quantity/prompt, use, and cancel. Test `pt-BR`, `zh-Hans`, and `en`; catalog parity covers the remaining cultures.

- [ ] **Step 2: Add aliases and implement `DomainTextRecognizer`**

Give methods intention-revealing names such as `IsChallengeCompleted`, `IsPetrifiedTree`, and `IsResinUsePrompt`. Methods delegate to stable keys and contain no literal displayed text.

- [ ] **Step 3: Replace domain OCR comparisons only**

Leave exception messages and internal action identifiers unchanged. Resolve the recognizer once in the task constructor; never resolve DI services inside retry loops.

- [ ] **Step 4: Run domain, catalog, and matcher tests; delete migrated resx; commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~AutoDomainTests|FullyQualifiedName~GameTextCatalogTests"
rtk git add -- BetterGenshinImpact/GameTask/AutoDomain/DomainTextRecognizer.cs BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoDomainTests/DomainTextRecognizerTests.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.zh-Hans.resx BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.zh-Hant.resx BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.en.resx BetterGenshinImpact/GameTask/AutoDomain/AutoDomainTask.fr.resx
rtk git commit -m "feat: localize domain OCR recognition"
```

---

## Task 10: Migrate boss reward and resin recognition

**Files:**

- Create: `BetterGenshinImpact/GameTask/AutoBoss/BossTextRecognizer.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoBossTests/BossTextRecognizerTests.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoBoss/AutoBossTask.cs`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Write RED tests**

Cover full-resin recovery, recovery-time wording, use quantity, touch Trounce Blossom, use/add resin, click blank area to continue, supplement resin, original resin, and insufficient resin. Include split-region cases and negative lookalikes.

- [ ] **Step 2: Implement recognizer and migrate only OCR-derived branches**

Keep numeric parsing separate: semantic matching selects the prompt/state, then existing numeric parsing extracts quantities. Do not treat numbers as translated aliases.

- [ ] **Step 3: Run focused tests and commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~AutoBossTests|FullyQualifiedName~GameTextCatalogTests"
rtk git add -- BetterGenshinImpact/GameTask/AutoBoss/BossTextRecognizer.cs BetterGenshinImpact/GameTask/AutoBoss/AutoBossTask.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoBossTests/BossTextRecognizerTests.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json
rtk git commit -m "feat: localize boss OCR recognition"
```

---

## Task 11: Migrate Ley Line Outcrop recognition

**Files:**

- Create: `BetterGenshinImpact/GameTask/AutoLeyLineOutcrop/LeyLineTextRecognizer.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoLeyLineOutcropTests/LeyLineTextRecognizerTests.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoLeyLineOutcrop/AutoLeyLineOutcropTask.cs`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Write RED tests for the full Ley Line state machine vocabulary**

Cover original/condensed/transient/fragile resin, replenish, double reward/2x, touch/activate/select, Ley Line/Outcrop, Blossom of Wealth/Revelation, revive, use, stop, and the 40-resin prompt. Preserve the known OCR typo alias only as a curated alias with a manifest reason.

- [ ] **Step 2: Implement a focused recognizer**

Expose compound decisions such as `IsRewardBlossomPrompt(IEnumerable<string>)` and `IsAllowedResinOption(string)`. Use `IsCombinedMatch` for OCR fragments. Pre-resolve aliases through the singleton matcher.

- [ ] **Step 3: Replace audited OCR comparisons**

Do not change the line that compares internal action names or exception text. Retain existing resin priority and click behavior.

- [ ] **Step 4: Run focused tests and commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~AutoLeyLineOutcropTests|FullyQualifiedName~GameTextCatalogTests"
rtk git add -- BetterGenshinImpact/GameTask/AutoLeyLineOutcrop/LeyLineTextRecognizer.cs BetterGenshinImpact/GameTask/AutoLeyLineOutcrop/AutoLeyLineOutcropTask.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoLeyLineOutcropTests/LeyLineTextRecognizerTests.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json
rtk git commit -m "feat: localize ley line OCR recognition"
```

---

## Task 12: Migrate Stygian Onslaught recognition

**Files:**

- Create: `BetterGenshinImpact/GameTask/AutoStygianOnslaught/StygianTextRecognizer.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoStygianOnslaughtTests/StygianTextRecognizerTests.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoStygianOnslaught/AutoStygianOnslaughtTask.cs`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Write RED tests**

Cover return, challenge failed, retry, blossom/reward, resin, preview, start, solo, event name, event overview, event phase ended, insufficient/supplement resin, and activate. Where event wording is version-specific, store the current displayed wording plus a stable key; do not encode the event title into control-flow identifiers.

- [ ] **Step 2: Implement and migrate**

Use semantic matches for OCR results and leave internal enum/task values intact. Add a test proving a former Chinese literal no longer gates a Portuguese branch.

- [ ] **Step 3: Run focused tests and commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~AutoStygianOnslaughtTests|FullyQualifiedName~GameTextCatalogTests"
rtk git add -- BetterGenshinImpact/GameTask/AutoStygianOnslaught/StygianTextRecognizer.cs BetterGenshinImpact/GameTask/AutoStygianOnslaught/AutoStygianOnslaughtTask.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoStygianOnslaughtTests/StygianTextRecognizerTests.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json
rtk git commit -m "feat: localize stygian OCR recognition"
```

---

## Task 13: Migrate expedition, daily, and dialogue-skip recognition

**Files:**

- Create: `BetterGenshinImpact/GameTask/AutoSkip/ExpeditionTextRecognizer.cs`
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoSkipTests/ExpeditionTextRecognizerTests.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoSkip/ExpeditionTask.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoSkip/AutoSkipTrigger.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoPathing/PathExecutor.cs`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Write RED tests for expedition states and trigger classification**

Cover time shortened, rewards increased, no bonus, expedition complete, in progress, daily commission, exploration/dispatch, exclusions, and exploration-dispatch rewards. Include full OCR strings and fragmented OCR-region lists.

- [ ] **Step 2: Implement shared expedition recognizer**

Both AutoSkip and PathExecutor must use the same semantic keys. Do not duplicate aliases inside feature classes.

- [ ] **Step 3: Migrate call sites, run tests, and commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~AutoSkipTests|FullyQualifiedName~GameTextCatalogTests"
rtk git add -- BetterGenshinImpact/GameTask/AutoSkip/ExpeditionTextRecognizer.cs BetterGenshinImpact/GameTask/AutoSkip/ExpeditionTask.cs BetterGenshinImpact/GameTask/AutoSkip/AutoSkipTrigger.cs BetterGenshinImpact/GameTask/AutoPathing/PathExecutor.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/AutoSkipTests/ExpeditionTextRecognizerTests.cs BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json
rtk git commit -m "feat: localize expedition OCR recognition"
```

---

## Task 14: Finish the OCR comparison audit and migrate remaining first-party flows

**Files:**

- Modify: `BetterGenshinImpact/GameTask/AutoPick/AutoPickTrigger.cs`
- Modify: `BetterGenshinImpact/GameTask/GetGridIcons/GetGridIconsTask.cs`
- Modify: `BetterGenshinImpact/GameTask/CharacterDevelopment/CharacterDevelopmentTask.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoMusicGame/AutoMusicGameTask.cs`
- Modify: `BetterGenshinImpact/GameTask/AutoWood/AutoWoodTask.cs`
- Modify: `BetterGenshinImpact/GameTask/GameLoading/GameLoading.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/CountInventoryItem.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/LowerHeadThenWalkToTask.cs`
- Modify: `BetterGenshinImpact/GameTask/Common/Job/GoToSereniteaPotTask.cs` if the companionship-reward OCR branch remains
- Create: `Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/RemainingGameTextRecognitionTests.cs`
- Create: `scripts/game-text/Audit-LanguageDependentRecognition.ps1`
- Create: `Docs/development/game-text-non-ocr-allowlist.json`
- Create: `Docs/development/game-text-ocr-audit.md`
- Modify: all six catalogs, keys, and manifest

- [ ] **Step 1: Re-run the source audit and classify every match**

Search `GameTask` for Chinese/English literal comparisons adjacent to OCR results (`Text`, `Ocr`, `OcrResult`, `Regions`). Classify each as:

- migrated semantic OCR match;
- raw name/number/free-form extraction that must remain raw;
- internal identifier/protocol/window title that must remain literal;
- dead/commented code to remove.

Record every intentional literal exception by file, stable surrounding symbol, category, and reason in `game-text-non-ocr-allowlist.json`. Do not key the allowlist only by line number.

- [ ] **Step 2: Write RED tests for remaining genuine OCR branches**

Cover pickup suppression labels, talent info, music-game labels, obtained/wood text, splash age/guardian text, inventory enhancement-ore names, activate, and companionship EXP. Add catalog keys only for branches that truly consume OCR.

- [ ] **Step 3: Migrate genuine OCR branches**

For item/product names sourced from TextMap, use semantic keys and retain numeric extraction separately. Keep launcher window-class/title comparisons, redemption-code input, combat scripts, character IDs, log parsing, path labels, and AutoGenius configuration strings literal and allowlisted.

- [ ] **Step 4: Implement the repeatable audit script**

The script must run `rg` patterns over tracked `GameTask/*.cs`, compare candidates to the JSON allowlist, and exit non-zero for unclassified candidates or stale allowlist entries. It is read-only.

Run:

```powershell
rtk pwsh scripts/game-text/Audit-LanguageDependentRecognition.ps1
```

Expected: `0 unclassified OCR-dependent literal comparisons` and a printed count for intentional non-OCR literals.

- [ ] **Step 5: Run focused tests and commit**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~RemainingGameTextRecognitionTests|FullyQualifiedName~GameTextCatalogTests"
rtk git add -- BetterGenshinImpact/GameTask/AutoPick/AutoPickTrigger.cs BetterGenshinImpact/GameTask/GetGridIcons/GetGridIconsTask.cs BetterGenshinImpact/GameTask/CharacterDevelopment/CharacterDevelopmentTask.cs BetterGenshinImpact/GameTask/AutoMusicGame/AutoMusicGameTask.cs BetterGenshinImpact/GameTask/AutoWood/AutoWoodTask.cs BetterGenshinImpact/GameTask/GameLoading/GameLoading.cs BetterGenshinImpact/GameTask/Common/Job/CountInventoryItem.cs BetterGenshinImpact/GameTask/Common/Job/LowerHeadThenWalkToTask.cs BetterGenshinImpact/GameTask/Common/Job/GoToSereniteaPotTask.cs Test/BetterGenshinImpact.UnitTest/GameTaskTests/Localization/RemainingGameTextRecognitionTests.cs scripts/game-text/Audit-LanguageDependentRecognition.ps1 Docs/development/game-text-non-ocr-allowlist.json Docs/development/game-text-ocr-audit.md BetterGenshinImpact/GameTask/Localization/GameTextKeys.cs BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hans.json BetterGenshinImpact/GameTask/Localization/Catalogs/zh-Hant.json BetterGenshinImpact/GameTask/Localization/Catalogs/en.json BetterGenshinImpact/GameTask/Localization/Catalogs/ja.json BetterGenshinImpact/GameTask/Localization/Catalogs/fr.json BetterGenshinImpact/GameTask/Localization/Catalogs/pt-BR.json BetterGenshinImpact/GameTask/Localization/Catalogs/source-manifest.json
rtk git commit -m "feat: finish game text OCR migration"
```

---

## Task 15: Document APIs, validate the complete branch, review, and publish the stacked PR

**Files:**

- Create: `Docs/development/game-text-localization.md`
- Modify: the most relevant script API documentation index found under `Docs/`

- [ ] **Step 1: Write user/developer documentation**

Document:

- why raw OCR remains unchanged;
- semantic keys and catalog schema;
- exact/neutral culture fallback and failure behavior;
- how to add/update aliases from the pinned AnimeGameData source;
- how to record curated OCR variants;
- C# examples using `IGameTextMatcher` and BgiVision semantic APIs;
- JavaScript examples using `gameText`;
- how to run catalog, OCR, audit, unit-test, and Windows x64 build checks;
- stacked PR/base-branch policy and upstream update workflow.

- [ ] **Step 2: Run the catalog and literal audits**

```powershell
rtk pwsh scripts/game-text/Audit-LanguageDependentRecognition.ps1
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Release -p:Platform=x64 --filter "FullyQualifiedName~GameText|FullyQualifiedName~PaddleOcrServiceTests|FullyQualifiedName~AutoDomainTests|FullyQualifiedName~AutoBossTests|FullyQualifiedName~AutoLeyLineOutcropTests|FullyQualifiedName~AutoStygianOnslaughtTests|FullyQualifiedName~AutoSkipTests"
```

Expected: audit has zero unclassified candidates and all focused tests pass.

- [ ] **Step 3: Run the full unit suite**

```powershell
rtk dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Release -p:Platform=x64
```

Expected: all tests pass. Investigate every failure; do not label unrelated failures without reproducing them on the base branch.

- [ ] **Step 4: Run the requested Windows x64 build**

```powershell
rtk dotnet build BetterGenshinImpact.sln -c Release -p:Platform=x64
```

Expected: build succeeds with zero errors. Record warning count in the PR without claiming pre-existing warnings were introduced by this branch.

- [ ] **Step 5: Verify the final diff**

```powershell
rtk git diff --check
rtk git status --short
rtk git diff --stat i18n/pt-br-completo...HEAD
rtk git log --oneline i18n/pt-br-completo..HEAD
```

Confirm no UI-translation files unrelated to OCR leaked into the follow-up, no whole TextMaps/model binaries were committed, all new catalog files are embedded, and all intended test assets are tracked.

- [ ] **Step 6: Request an independent code review**

Use `superpowers:requesting-code-review` with a reviewer subagent over the exact base/head commit range. Because repository `AGENTS.md` requires review findings in Simplified Chinese, instruct the reviewer to report findings in Simplified Chinese and prioritize crashes, deadlocks, resources, async behavior, `IDisposable`, and shared state. Apply valid feedback using `superpowers:receiving-code-review`, rerun affected tests, then rerun the full verification commands above.

- [ ] **Step 7: Commit documentation and review fixes**

```powershell
rtk git add Docs/development/game-text-localization.md Docs/development/game-text-ocr-audit.md Docs/development/game-text-non-ocr-allowlist.json
rtk git commit -m "docs: explain localized OCR matching"
```

If review fixes changed code, use a separate precise commit and list its exact files.

- [ ] **Step 8: Re-check PR #2 and choose the current base**

```powershell
rtk gh pr view 2 --repo alien6/better-genshin-impact --json state,isDraft,baseRefName,headRefName,mergeable,url
rtk git fetch origin
```

If PR #2 remains open, base this PR on `i18n/pt-br-completo`. If PR #2 is merged, compare against `origin/main`, retarget to `main`, and rerun tests/build on the resulting history before publishing.

- [ ] **Step 9: Push and create or update one draft PR**

```powershell
rtk git push -u origin feat/pt-br-ocr-automation
rtk gh pr list --repo alien6/better-genshin-impact --head alien6:feat/pt-br-ocr-automation --state all --json number,state,isDraft,url
```

If no matching PR exists:

```powershell
rtk gh pr create --repo alien6/better-genshin-impact --base i18n/pt-br-completo --head feat/pt-br-ocr-automation --draft --title "feat: support PT-BR OCR and automation" --body-file ../pr-body-pt-br-ocr.md
```

Create `../pr-body-pt-br-ocr.md` with `apply_patch`. Its body must include summary, stacked dependency on PR #2, architecture, migrated flows, catalog source commit, compatibility guarantees, test/build evidence, known limitations, and an explicit `No automatic merge` note. If a matching PR exists, update it instead of creating another. Delete the temporary file with `apply_patch` after the PR operation.

- [ ] **Step 10: Inspect remote checks and report without merging**

```powershell
rtk gh pr view --repo alien6/better-genshin-impact --json number,state,isDraft,baseRefName,headRefName,mergeable,url,statusCheckRollup
```

Stop with the PR open for human review. Do not run `gh pr merge` and do not enable auto-merge.
