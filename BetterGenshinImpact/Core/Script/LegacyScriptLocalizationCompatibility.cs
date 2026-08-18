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
            const values = genshin.getLegacyTexts(searchValue);
            if (!values || typeof values.length !== 'number') return null;
            return values;
        } catch (_) {
            return null;
        }
    }

    String.prototype.includes = function(searchValue, position) {
        if (originalIncludes.call(this, searchValue, position)) return true;
        const variants = localizedVariants(searchValue);
        if (!variants) return false;
        for (let i = 0; i < variants.length; i++) {
            const candidate = String(variants[i]);
            if (candidate !== searchValue && originalIncludes.call(this, candidate, position)) return true;
        }
        return false;
    };

    String.prototype.startsWith = function(searchValue, position) {
        if (originalStartsWith.call(this, searchValue, position)) return true;
        const variants = localizedVariants(searchValue);
        if (!variants) return false;
        for (let i = 0; i < variants.length; i++) {
            const candidate = String(variants[i]);
            if (candidate !== searchValue && originalStartsWith.call(this, candidate, position)) return true;
        }
        return false;
    };

    String.prototype.endsWith = function(searchValue, endPosition) {
        if (originalEndsWith.call(this, searchValue, endPosition)) return true;
        const variants = localizedVariants(searchValue);
        if (!variants) return false;
        for (let i = 0; i < variants.length; i++) {
            const candidate = String(variants[i]);
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
            const candidate = String(variants[i]);
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
