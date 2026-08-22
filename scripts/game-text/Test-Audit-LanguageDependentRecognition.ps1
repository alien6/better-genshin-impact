[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$auditScript = Join-Path $repositoryRoot 'scripts\game-text\Audit-LanguageDependentRecognition.ps1'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Invoke-AuditFixture([string]$source, [object[]]$allowlistEntries) {
    $temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("bgi-game-text-audit-" + [guid]::NewGuid().ToString('N'))
    try {
        $gameTaskDirectory = Join-Path $temporaryRoot 'BetterGenshinImpact\GameTask\Fixture'
        $scriptDirectory = Join-Path $temporaryRoot 'scripts\game-text'
        $docsDirectory = Join-Path $temporaryRoot 'Docs\development'
        $null = New-Item -ItemType Directory -Path $gameTaskDirectory, $scriptDirectory, $docsDirectory
        [System.IO.File]::WriteAllText((Join-Path $gameTaskDirectory 'AuditFixture.cs'), $source, $utf8NoBom)
        Copy-Item -LiteralPath $auditScript -Destination (Join-Path $scriptDirectory 'Audit-LanguageDependentRecognition.ps1')
        $allowlist = [ordered]@{ schemaVersion = 1; entries = $allowlistEntries }
        [System.IO.File]::WriteAllText(
            (Join-Path $docsDirectory 'game-text-non-ocr-allowlist.json'),
            (ConvertTo-Json -InputObject $allowlist -Depth 10),
            $utf8NoBom)

        & git -C $temporaryRoot init --quiet
        if ($LASTEXITCODE -ne 0) { throw 'Could not initialize audit fixture repository.' }
        & git -C $temporaryRoot config core.autocrlf false
        if ($LASTEXITCODE -ne 0) { throw 'Could not configure audit fixture repository.' }
        & git -C $temporaryRoot add -- 'BetterGenshinImpact/GameTask/Fixture/AuditFixture.cs'
        if ($LASTEXITCODE -ne 0) { throw 'Could not track audit fixture source.' }

        Push-Location $temporaryRoot
        try {
            $output = @(& pwsh -NoProfile -File (Join-Path $scriptDirectory 'Audit-LanguageDependentRecognition.ps1') 2>&1)
            return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output -join "`n" }
        }
        finally {
            Pop-Location
        }
    }
    finally {
        if (Test-Path -LiteralPath $temporaryRoot) {
            $resolvedTemporaryRoot = (Resolve-Path -LiteralPath $temporaryRoot).Path
            $systemTemporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
            if (-not $resolvedTemporaryRoot.StartsWith($systemTemporaryRoot, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Refusing to remove unexpected fixture path '$resolvedTemporaryRoot'."
            }
            Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force
        }
    }
}

function Assert-NegativeAudit([string]$name, [pscustomobject]$result, [string]$expectedOutput) {
    if ($result.ExitCode -eq 0) {
        $script:assertionFailures.Add("${name}: audit unexpectedly succeeded.`n$($result.Output)")
        return
    }
    if ($result.Output.IndexOf($expectedOutput, [StringComparison]::Ordinal) -lt 0) {
        $script:assertionFailures.Add("${name}: output did not contain '$expectedOutput'.`n$($result.Output)")
    }
}

$assertionFailures = [System.Collections.Generic.List[string]]::new()

$regionHasTextResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class RegionHasTextGate
{
    public bool Detect(OcrResult result) => result.RegionHasText("角色选择");
}
'@ @()
Assert-NegativeAudit 'RegionHasText helper fixture' $regionHasTextResult 'UNCLASSIFIED'

$tryClickAnyTextResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class TryClickAnyTextGate
{
    public bool Detect(BvPage page) => TryClickAnyText(page, new[] { "更换", "加入" }, default);
}
'@ @()
Assert-NegativeAudit 'TryClickAnyText helper fixture' $tryClickAnyTextResult 'UNCLASSIFIED'

$waitUntilTextResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class WaitUntilTextGate
{
    public BvFlow Detect(BvFlow flow) => flow.WaitUntilText("顺序");
}
'@ @()
Assert-NegativeAudit 'WaitUntilText helper fixture' $waitUntilTextResult 'UNCLASSIFIED'

$collectionExpressionResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class CollectionExpressionGate
{
    public bool Detect(string ocrText)
    {
        string[] labels = ["更换", "加入"];
        return labels.Any(ocrText.Contains);
    }
}
'@ @()
Assert-NegativeAudit 'C# collection expression fixture' $collectionExpressionResult 'UNCLASSIFIED'

$helperResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class HelperGate
{
    public bool Detect(object capture) => ContainsText(capture, "确认筛选");
}
'@ @()
Assert-NegativeAudit 'helper API fixture' $helperResult 'UNCLASSIFIED'

$multilineResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class MultilineGate
{
    public object Detect(object page) => page.GetByText(
        "合成");
}
'@ @()
Assert-NegativeAudit 'multiline helper fixture' $multilineResult 'UNCLASSIFIED'

$extractedComparisonResult = Invoke-AuditFixture @'
namespace Fixture;
public sealed class ExtractedGate
{
    public bool Detect(string ocrText)
    {
        var labels = new[] { "挑战达成", "战斗胜利" };
        var failures = new[] { "战斗失败" };
        return labels.Any(ocrText.Contains) || failures.Any(ocrText.Contains);
    }
}
'@ @()
Assert-NegativeAudit 'extracted-text comparison fixture' $extractedComparisonResult 'UNCLASSIFIED'
if ($extractedComparisonResult.Output.IndexOf('战斗失败', [StringComparison]::Ordinal) -lt 0) {
    $assertionFailures.Add("extracted-text comparison fixture: second extracted collection was missed.`n$($extractedComparisonResult.Output)")
}

$staleId = 'BetterGenshinImpact/GameTask/Fixture/AuditFixture.cs::Missing::stale'
$staleResult = Invoke-AuditFixture 'namespace Fixture; public sealed class NoGate { }' @(
    [ordered]@{
        id = $staleId
        path = 'BetterGenshinImpact/GameTask/Fixture/AuditFixture.cs'
        symbol = 'Missing'
        literal = 'stale'
        category = 'internal-identifier'
        reason = 'Fixture intentionally has no matching candidate.'
    })
Assert-NegativeAudit 'stale allowlist fixture' $staleResult '1 stale allowlist entries'

if ($assertionFailures.Count -gt 0) {
    throw ($assertionFailures -join "`n`n")
}

Write-Host 'Audit regression fixtures passed: RegionHasText, TryClickAnyText, WaitUntilText, C# collection expression, helper API, multiline invocation, extracted comparison, and stale allowlist all fail closed.'
