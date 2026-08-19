using Microsoft.ClearScript;

namespace BetterGenshinImpact.Core.Script;

/// <summary>
/// Installs an additive JavaScript compatibility layer for historical scripts
/// that compare OCR text against embedded Chinese literals. Native JavaScript
/// behavior always runs first. Only a failed search for a CJK string attempts
/// culture-aware variants from genshin.getLegacyTexts().
/// </summary>
public static class LegacyScriptLocalizationCompatibility
{
    public const string BootstrapScript = """
(() => {
    if (globalThis.__betterGiLegacyLocalizationInstalled) return;
    globalThis.__betterGiLegacyLocalizationInstalled = true;

    const cjk = /[\u3400-\u4dbf\u4e00-\u9fff]/;
    const originalIncludes = String.prototype.includes;
    const originalStartsWith = String.prototype.startsWith;
    const originalEndsWith = String.prototype.endsWith;
    const originalIndexOf = String.prototype.indexOf;

    function localizedVariants(searchValue) {
        if (typeof searchValue !== 'string' || !cjk.test(searchValue)) return null;
        try {
            // BetterGI exposes host members in lower camel case, while a bare
            // ClearScript host object (as used by unit tests and third-party
            // hosts) can expose the original CLR member name. Supporting both
            // keeps this compatibility layer independent of host naming policy.
            let resolver = null;
            if (genshin && typeof genshin.getLegacyTexts === 'function') {
                resolver = value => genshin.getLegacyTexts(value);
            } else if (genshin && typeof genshin.GetLegacyTexts === 'function') {
                resolver = value => genshin.GetLegacyTexts(value);
            }
            if (!resolver) return null;

            const values = resolver(searchValue);
            if (!values) return null;

            // CLR arrays may expose Length while JavaScript arrays expose length.
            const rawLength = typeof values.length === 'number'
                ? values.length
                : (typeof values.Length === 'number' ? values.Length : null);
            if (rawLength === null) return null;

            const normalized = [];
            for (let i = 0; i < rawLength; i++) {
                normalized.push(String(values[i]));
            }
            return normalized;
        } catch (_) {
            return null;
        }
    }

    String.prototype.includes = function(searchValue, position) {
        if (originalIncludes.call(this, searchValue, position)) return true;
        const variants = localizedVariants(searchValue);
        if (!variants) return false;
        for (let i = 0; i < variants.length; i++) {
            const candidate = variants[i];
            if (candidate !== searchValue && originalIncludes.call(this, candidate, position)) return true;

            // Some legacy OCR helpers normalize the recognized text with
            // toLowerCase() before searching for the original Chinese literal.
            // Only after the native search fails and only for an audited CJK
            // literal, also try the localized candidate in the same casing.
            const lowerCandidate = candidate.toLocaleLowerCase();
            if (lowerCandidate !== candidate && originalIncludes.call(this, lowerCandidate, position)) return true;
        }
        return false;
    };

    String.prototype.startsWith = function(searchValue, position) {
        if (originalStartsWith.call(this, searchValue, position)) return true;
        const variants = localizedVariants(searchValue);
        if (!variants) return false;
        for (let i = 0; i < variants.length; i++) {
            const candidate = variants[i];
            if (candidate !== searchValue && originalStartsWith.call(this, candidate, position)) return true;
        }
        return false;
    };

    String.prototype.endsWith = function(searchValue, endPosition) {
        if (originalEndsWith.call(this, searchValue, endPosition)) return true;
        const variants = localizedVariants(searchValue);
        if (!variants) return false;
        for (let i = 0; i < variants.length; i++) {
            const candidate = variants[i];
            if (candidate !== searchValue && originalEndsWith.call(this, candidate, endPosition)) return true;
        }
        return false;
    };

    String.prototype.indexOf = function(searchValue, fromIndex) {
        const originalResult = originalIndexOf.call(this, searchValue, fromIndex);
        if (originalResult !== -1) return originalResult;
        const variants = localizedVariants(searchValue);
        if (!variants) return -1;
        let best = -1;
        for (let i = 0; i < variants.length; i++) {
            const candidate = variants[i];
            if (candidate === searchValue) continue;
            const found = originalIndexOf.call(this, candidate, fromIndex);
            if (found !== -1 && (best === -1 || found < best)) best = found;
        }
        return best;
    };
})();
""";

    public static void Install(IScriptEngine engine)
    {
        engine.Execute(BootstrapScript);
    }
}
