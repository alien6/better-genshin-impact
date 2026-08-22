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

$candidatePattern = '(?i)ContainsText|TryClickText|GetBy(?:Any)?Text|Find(?:Rect)?ByText|Bv\s*\.\s*FindF?|Text|Ocr|Region|Title|ClassName'
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
$sourceCache = @{}
$candidateById = [ordered]@{}
$candidatePaths = @($rgOutput | ForEach-Object {
    $record = $_ | ConvertFrom-Json
    if ($record.type -eq 'match') { ([string]$record.data.path.text).Replace('\', '/') }
} | Sort-Object -Unique)

function Add-Candidate([string]$path, [int]$literalIndex, [string]$literal) {
    if ([string]::IsNullOrWhiteSpace($literal)) { return }
    $source = $sourceCache[$path]
    $lineNumber = 1 + ([regex]::Matches($source.Substring(0, $literalIndex), "`n")).Count
    $sourceLine = $fileCache[$path][$lineNumber - 1]
    if ($sourceLine.TrimStart().StartsWith('//', [StringComparison]::Ordinal)) { return }
    $symbol = Get-SurroundingSymbol $fileCache[$path] ($lineNumber - 1)
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

function Get-InvocationEnd([string]$source, [int]$openParenthesisIndex) {
    $depth = 0
    $state = 'code'
    for ($index = $openParenthesisIndex; $index -lt $source.Length; $index++) {
        $character = $source[$index]
        $next = if ($index + 1 -lt $source.Length) { $source[$index + 1] } else { [char]0 }
        switch ($state) {
            'line-comment' {
                if ($character -eq "`n") { $state = 'code' }
                continue
            }
            'block-comment' {
                if ($character -eq '*' -and $next -eq '/') { $state = 'code'; $index++ }
                continue
            }
            'string' {
                if ($character -eq '\') { $index++; continue }
                if ($character -eq '"') { $state = 'code' }
                continue
            }
            'verbatim-string' {
                if ($character -eq '"' -and $next -eq '"') { $index++; continue }
                if ($character -eq '"') { $state = 'code' }
                continue
            }
            'character' {
                if ($character -eq '\') { $index++; continue }
                if ($character -eq "'") { $state = 'code' }
                continue
            }
        }

        if ($character -eq '/' -and $next -eq '/') { $state = 'line-comment'; $index++; continue }
        if ($character -eq '/' -and $next -eq '*') { $state = 'block-comment'; $index++; continue }
        if ($character -eq '@' -and $next -eq '"') { $state = 'verbatim-string'; $index++; continue }
        if ($character -eq '"') { $state = 'string'; continue }
        if ($character -eq "'") { $state = 'character'; continue }
        if ($character -eq '(') { $depth++; continue }
        if ($character -eq ')') {
            $depth--
            if ($depth -eq 0) { return $index }
        }
    }
    return -1
}

$helperInvocationPattern = [regex]::new(
    '(?im)\b(?:(?:Bv)\s*\.\s*(?:FindF?|FindByText)|ContainsText|TryClickText|GetByAnyText|GetByText|FindRectByText|OcrMatch)\s*\(')
$directMethodPattern = [regex]::new(
    '(?is)\b(?<receiver>[A-Za-z_]\w*(?:\s*\.\s*[A-Za-z_]\w*)*)\s*\.\s*(?:Contains|StartsWith|EndsWith|Equals)\s*\(\s*"(?<literal>(?:[^"\\]|\\.)+)"')
$directEqualityPattern = [regex]::new(
    '(?is)(?:\b(?<receiver>[A-Za-z_]\w*(?:\s*\.\s*[A-Za-z_]\w*)*)\s*(?:==|!=)\s*"(?<literal>(?:[^"\\]|\\.)+)"|"(?<reverseLiteral>(?:[^"\\]|\\.)+)"\s*(?:==|!=)\s*\b(?<reverseReceiver>[A-Za-z_]\w*(?:\s*\.\s*[A-Za-z_]\w*)*))')
$collectionDeclarationPattern = [regex]::new(
    '(?is)\b(?<name>[A-Za-z_]\w*)\s*=\s*new\s*\[\s*\]\s*\{(?<items>.{0,1500}?)\}\s*;')

foreach ($path in $candidatePaths) {
    $absolutePath = Join-Path $repositoryRoot $path
    $source = Get-Content -LiteralPath $absolutePath -Raw -Encoding UTF8
    $sourceCache[$path] = $source
    $fileCache[$path] = @(Get-Content -LiteralPath $absolutePath -Encoding UTF8)

    foreach ($helperMatch in $helperInvocationPattern.Matches($source)) {
        $openParenthesisIndex = $source.IndexOf('(', $helperMatch.Index)
        $endIndex = Get-InvocationEnd $source $openParenthesisIndex
        if ($endIndex -lt 0) { continue }
        $invocation = $source.Substring($helperMatch.Index, $endIndex - $helperMatch.Index + 1)
        foreach ($literalMatch in [regex]::Matches($invocation, '"(?<literal>(?:[^"\\]|\\.)+)"')) {
            Add-Candidate $path ($helperMatch.Index + $literalMatch.Index) $literalMatch.Groups['literal'].Value
        }
    }

    foreach ($comparisonMatch in $directMethodPattern.Matches($source)) {
        $receiver = $comparisonMatch.Groups['receiver'].Value
        if ($receiver -notmatch '(?i)text|ocr|region|title|classname') { continue }
        Add-Candidate $path $comparisonMatch.Groups['literal'].Index $comparisonMatch.Groups['literal'].Value
    }

    foreach ($comparisonMatch in $directEqualityPattern.Matches($source)) {
        $receiver = if ($comparisonMatch.Groups['receiver'].Success) { $comparisonMatch.Groups['receiver'].Value } else { $comparisonMatch.Groups['reverseReceiver'].Value }
        if ($receiver -notmatch '(?i)text|ocr|region|title|classname') { continue }
        $literalGroup = if ($comparisonMatch.Groups['literal'].Success) { $comparisonMatch.Groups['literal'] } else { $comparisonMatch.Groups['reverseLiteral'] }
        Add-Candidate $path $literalGroup.Index $literalGroup.Value
    }

    foreach ($collectionMatch in $collectionDeclarationPattern.Matches($source)) {
        $tailLength = [Math]::Min(3000, $source.Length - ($collectionMatch.Index + $collectionMatch.Length))
        $tail = $source.Substring($collectionMatch.Index + $collectionMatch.Length, $tailLength)
        $collectionName = [regex]::Escape($collectionMatch.Groups['name'].Value)
        if ($tail -notmatch "(?is)\b$collectionName\s*\.\s*Any\s*\(\s*\w*(?:text|ocr|region)\w*\s*\.\s*Contains") { continue }
        $itemsGroup = $collectionMatch.Groups['items']
        foreach ($literalMatch in [regex]::Matches($itemsGroup.Value, '"(?<literal>(?:[^"\\]|\\.)+)"')) {
            Add-Candidate $path ($itemsGroup.Index + $literalMatch.Index) $literalMatch.Groups['literal'].Value
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
