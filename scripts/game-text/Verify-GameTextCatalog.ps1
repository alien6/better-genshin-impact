[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $AnimeGameDataPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$pinnedCommit = "26df1dfbdf05a82bbb1d97506859f3e1c40718d8"
$cultureFiles = [ordered]@{
    "zh-Hans" = "TextMapCHS.json"
    "zh-Hant" = "TextMapCHT.json"
    "en" = "TextMapEN.json"
    "ja" = "TextMapJP.json"
    "fr" = "TextMapFR.json"
    "pt-BR" = "TextMapPT.json"
}

$checkoutPath = (Resolve-Path -LiteralPath $AnimeGameDataPath).Path
$head = (& git -C $checkoutPath rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Could not read AnimeGameData HEAD at '$checkoutPath'."
}

if ($head -cne $pinnedCommit) {
    throw "AnimeGameData HEAD '$head' does not match pinned commit '$pinnedCommit'."
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$catalogDirectory = Join-Path $repositoryRoot "BetterGenshinImpact\GameTask\Localization\Catalogs"
$manifestPath = Join-Path $catalogDirectory "source-manifest.json"
$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json -AsHashtable

if ($manifest.baseline.repository -cne "https://github.com/DimbreathBot/AnimeGameData.git") {
    throw "Source manifest repository does not match the canonical AnimeGameData repository."
}

if ($manifest.baseline.commit -cne $pinnedCommit) {
    throw "Source manifest commit '$($manifest.baseline.commit)' does not match '$pinnedCommit'."
}

$seenKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$verifiedAliasCount = 0

foreach ($culture in $cultureFiles.Keys) {
    $catalogPath = Join-Path $catalogDirectory "$culture.json"
    $relativeTextMapPath = "TextMap/$($cultureFiles[$culture])"
    $textMapPath = Join-Path $checkoutPath $relativeTextMapPath
    if (-not (Test-Path -LiteralPath $textMapPath -PathType Leaf)) {
        throw "Required TextMap '$textMapPath' was not found."
    }

    $commitBlob = (& git -C $checkoutPath rev-parse "${pinnedCommit}:$relativeTextMapPath").Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Could not resolve pinned TextMap '$relativeTextMapPath'."
    }

    $workingTreeBlob = (& git -C $checkoutPath hash-object "--path=$relativeTextMapPath" -- $relativeTextMapPath).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Could not hash working-tree TextMap '$textMapPath'."
    }

    if ($workingTreeBlob -cne $commitBlob) {
        throw "Working-tree TextMap '$textMapPath' differs from pinned commit '$pinnedCommit'."
    }

    $catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json -AsHashtable
    $textMap = Get-Content -LiteralPath $textMapPath -Raw -Encoding UTF8 | ConvertFrom-Json -AsHashtable

    if ($catalog.culture -cne $culture) {
        throw "Catalog '$catalogPath' declares culture '$($catalog.culture)' instead of '$culture'."
    }

    foreach ($entry in $catalog.entries.GetEnumerator()) {
        $key = [string] $entry.Key
        $null = $seenKeys.Add($key)
        if (-not $manifest.entries.ContainsKey($key)) {
            throw "Manifest is missing key '$key'."
        }

        $provenanceByCulture = $manifest.entries[$key]
        if (-not $provenanceByCulture.ContainsKey($culture)) {
            throw "Manifest key '$key' is missing culture '$culture'."
        }

        $aliases = @($entry.Value)
        $records = @($provenanceByCulture[$culture])
        if ($records.Count -ne $aliases.Count) {
            throw "Manifest key '$key' culture '$culture' has $($records.Count) record(s) for $($aliases.Count) alias(es)."
        }

        foreach ($alias in $aliases) {
            $matchingRecords = @($records | Where-Object { $_.value -ceq $alias })
            if ($matchingRecords.Count -ne 1) {
                throw "Manifest key '$key' culture '$culture' must contain exactly one record for alias '$alias'."
            }

            $record = $matchingRecords[0]
            if ($record.ContainsKey("textMapHash")) {
                $hash = [string] $record.textMapHash
                if (-not $textMap.ContainsKey($hash)) {
                    throw "TextMap '$($cultureFiles[$culture])' does not contain hash '$hash' for '$key'."
                }

                if ([string] $textMap[$hash] -cne [string] $alias) {
                    throw "TextMap hash '$hash' for '$key'/'$culture' does not equal alias '$alias'."
                }
            }
            elseif ($record.source -cne "curated" -or [string]::IsNullOrWhiteSpace([string] $record.reason)) {
                throw "Curated manifest record '$key'/'$culture'/'$alias' requires source='curated' and a reason."
            }

            $verifiedAliasCount++
        }
    }
}

foreach ($manifestEntry in $manifest.entries.GetEnumerator()) {
    if (-not $seenKeys.Contains([string] $manifestEntry.Key)) {
        throw "Manifest contains unknown key '$($manifestEntry.Key)'."
    }

    foreach ($culture in $manifestEntry.Value.Keys) {
        if (-not $cultureFiles.Contains($culture)) {
            throw "Manifest key '$($manifestEntry.Key)' contains unsupported culture '$culture'."
        }
    }
}

Write-Host "Verified $verifiedAliasCount catalog aliases against AnimeGameData $pinnedCommit."
