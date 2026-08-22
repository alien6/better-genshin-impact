[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$allowlistPath = Join-Path $repositoryRoot 'Docs\development\game-text-non-ocr-allowlist.json'
$trackedFiles = @(& git -C $repositoryRoot ls-files -- ':(glob)BetterGenshinImpact/GameTask/**/*.cs')
if ($LASTEXITCODE -ne 0 -or $trackedFiles.Count -eq 0) {
    throw 'Could not enumerate tracked GameTask C# files.'
}

$candidatePattern = '(?i)(?:\b(?:\w*(?:text|ocr|region)\w*)\b.*(?:contains|startswith|endswith|equals|==|!=)|(?:contains|startswith|endswith|equals|==|!=).*\b(?:\w*(?:text|ocr|region)\w*)\b).*"(?:[^"\\]|\\.)+"'
$rgOutput = @(& rg --json --line-number --no-messages --color never --regexp $candidatePattern -- $trackedFiles)
if ($LASTEXITCODE -notin @(0, 1)) {
    throw "rg failed while auditing GameTask sources (exit $LASTEXITCODE)."
}

function Get-SurroundingSymbol([string[]]$lines, [int]$lineIndex) {
    $controlWords = @('if', 'for', 'foreach', 'while', 'switch', 'catch', 'using', 'lock', 'static')
    for ($index = $lineIndex; $index -ge 0; $index--) {
        $line = $lines[$index]
        if ($line -match '\b(?:public|private|protected|internal)\b') {
            $symbols = @([regex]::Matches($line, '\b(?<symbol>[A-Za-z_]\w*)\s*\(') | ForEach-Object { $_.Groups['symbol'].Value } | Where-Object { $controlWords -notcontains $_ })
            if ($symbols.Count -gt 0) { return $symbols[-1] }
        }
    }
    return '<type-initializer>'
}

$fileCache = @{}
$candidateById = [ordered]@{}
foreach ($jsonLine in $rgOutput) {
    $record = $jsonLine | ConvertFrom-Json
    if ($record.type -ne 'match') { continue }
    $path = ([string]$record.data.path.text).Replace('\', '/')
    $lineNumber = [int]$record.data.line_number
    $sourceLine = [string]$record.data.lines.text
    if ($sourceLine.TrimStart().StartsWith('//', [StringComparison]::Ordinal)) { continue }

    if (-not $fileCache.ContainsKey($path)) {
        $fileCache[$path] = @(Get-Content -LiteralPath (Join-Path $repositoryRoot $path) -Encoding UTF8)
    }
    $symbol = Get-SurroundingSymbol $fileCache[$path] ($lineNumber - 1)
    foreach ($literalMatch in [regex]::Matches($sourceLine, '"(?<literal>(?:[^"\\]|\\.)+)"')) {
        $literal = $literalMatch.Groups['literal'].Value
        if ([string]::IsNullOrWhiteSpace($literal)) { continue }
        $id = "$path::$symbol::$literal"
        if (-not $candidateById.Contains($id)) {
            $candidateById[$id] = [ordered]@{
                id = $id
                path = $path
                symbol = $symbol
                literal = $literal
                line = $lineNumber
            }
        }
    }
}

if (-not (Test-Path -LiteralPath $allowlistPath -PathType Leaf)) {
    throw "Allowlist '$allowlistPath' was not found."
}
$allowlist = Get-Content -LiteralPath $allowlistPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($allowlist.schemaVersion -ne 1) { throw 'Unsupported allowlist schemaVersion.' }

$allowedById = @{}
foreach ($entry in @($allowlist.entries)) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.id) -or
        [string]::IsNullOrWhiteSpace([string]$entry.path) -or
        [string]::IsNullOrWhiteSpace([string]$entry.symbol) -or
        [string]::IsNullOrWhiteSpace([string]$entry.literal) -or
        [string]::IsNullOrWhiteSpace([string]$entry.category) -or
        [string]::IsNullOrWhiteSpace([string]$entry.reason)) {
        throw 'Every allowlist entry requires id, path, symbol, literal, category, and reason.'
    }
    $expectedId = "$($entry.path)::$($entry.symbol)::$($entry.literal)"
    if ($entry.id -cne $expectedId) { throw "Allowlist id '$($entry.id)' does not match '$expectedId'." }
    if ($allowedById.ContainsKey([string]$entry.id)) { throw "Duplicate allowlist id '$($entry.id)'." }
    $allowedById[[string]$entry.id] = $entry
}

$unclassified = @($candidateById.Keys | Where-Object { -not $allowedById.ContainsKey($_) })
$stale = @($allowedById.Keys | Where-Object { -not $candidateById.Contains($_) })

foreach ($id in $unclassified) {
    $candidate = $candidateById[$id]
    Write-Host "UNCLASSIFIED $($candidate.path):$($candidate.line) [$($candidate.symbol)] literal '$($candidate.literal)'"
}
foreach ($id in $stale) { Write-Host "STALE $id" }

Write-Host "$($unclassified.Count) unclassified OCR-dependent literal comparisons"
Write-Host "$($candidateById.Count - $unclassified.Count) intentional non-OCR literals"
if ($stale.Count -gt 0) { Write-Host "$($stale.Count) stale allowlist entries" }

if ($unclassified.Count -gt 0 -or $stale.Count -gt 0) { exit 1 }
