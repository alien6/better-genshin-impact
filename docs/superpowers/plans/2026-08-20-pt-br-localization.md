# PT-BR Localization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add complete Brazilian Portuguese UI localization to BetterGI without replacing Chinese source strings or changing runtime behavior.

**Architecture:** Reuse the existing `JsonTranslationService` and its culture-specific JSON catalogs. Add `pt-BR` as a selectable UI culture, provide a catalog with the same source-key coverage as the current English catalog, and keep placeholders/format tokens intact. Existing missing-key reporting remains the mechanism for detecting future upstream strings that need translation.

**Tech Stack:** C#/.NET/WPF, JSON localization catalogs, CommunityToolkit MVVM.

**Spec:** User-approved design in the ChatGPT conversation on 2026-08-20.

## Global Constraints

- Preserve all existing languages and behavior.
- Do not translate internal IDs, paths, command names, format placeholders, or values consumed by application logic.
- Use Brazilian Portuguese (`pt-BR`) for UI copy.
- Keep the implementation easy to rebase onto future upstream updates.

---

### Task 1: Register Brazilian Portuguese UI culture

**Files:**
- Modify: `BetterGenshinImpact/View/Converters/CultureInfoNameToKVPConverter.cs`
- Modify: `BetterGenshinImpact/ViewModel/Pages/CommonSettingsPageViewModel.cs`

**Interfaces:**
- Consumes: existing `CultureInfoNameToKVPConverter.GetDisplayName(string)` and language dictionary initialization.
- Produces: selectable culture key `pt-BR` displayed as `Português (Brasil)`.

- [ ] Add `pt-BR` to the culture display-name switch.
- [ ] Add `pt-BR` to the UI language dictionary.
- [ ] Verify existing culture entries remain unchanged.

### Task 2: Add complete PT-BR translation catalog

**Files:**
- Create: `BetterGenshinImpact/User/I18n/pt-BR.json`

**Interfaces:**
- Consumes: current Chinese source keys represented by `en.json`.
- Produces: `pt-BR` translations loadable by `JsonTranslationService`.

- [ ] Build the catalog from the current `en.json` key set.
- [ ] Translate every user-visible value to Brazilian Portuguese.
- [ ] Preserve format placeholders, line breaks, symbols, hotkey names, and technical identifiers.
- [ ] Validate JSON structure and key coverage against `en.json`.

### Task 3: Validate integration

**Files:**
- Test: localization files and modified C# sources.

**Interfaces:**
- Consumes: Tasks 1-2.
- Produces: evidence that the branch is internally consistent.

- [ ] Confirm `pt-BR.json` parses as JSON.
- [ ] Confirm PT-BR key set matches the current English catalog key set.
- [ ] Confirm all translated values are non-empty.
- [ ] Confirm placeholders appearing in source translations are preserved.
- [ ] Run repository build/tests when supported by the available environment; otherwise report the environment limitation explicitly.
- [ ] Review the final diff and open a pull request to `main`.
