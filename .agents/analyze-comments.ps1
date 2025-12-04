#!/usr/bin/env pwsh
# Analyze PR #31 comments and match to commits

$j = Get-Content "$env:TEMP\pr31_comments.json" | ConvertFrom-Json

# Commits that address issues:
$commitMap = @{
    "818aa55" = @("IdentityTypeMapper", "thread safety", "GetHashCode", "race condition")
    "0854b25" = @("null check", "base constructor", "constructor call", "ArgumentNullException")
    "8da0ba8" = @("VssConnectionAdapter", "CommonStructureService", "WorkItemCore", "indexer")
    "68a4cba" = @("LevelOrderEnumerator", "Reset", "FieldDefinitionCollection")
    "f38a97d" = @("IRelatedLink", "LinkTypeEnd", "WorkItemLinkType", "coercion", "Coerce")
    "0576995" = @("IExceptionExploder", "typo", "Link.Comment", "documentation")
    "d35bf0b6" = @("IdentityDescriptor", "ProjectCollection", "validation")
    "49e782a1" = @("Revision", "SOAP", "indexer")
    "4ad698b7" = @("NoWarn", "editorconfig", "CA", "IDE", "suppressions", "Directory.Build.props")
    "c4780db8" = @("WorkItemLinkTypeCollection", "IList")
}

# Categorize comments
$results = @()
foreach ($comment in $j) {
    $matched = $null
    $body = $comment.body.ToLower()
    $path = $comment.path.ToLower()

    foreach ($commit in $commitMap.Keys) {
        $keywords = $commitMap[$commit]
        foreach ($kw in $keywords) {
            if ($path -like "*$($kw.ToLower())*" -or $body -like "*$($kw.ToLower())*") {
                $matched = $commit
                break
            }
        }
        if ($matched) { break }
    }

    $results += [PSCustomObject]@{
        id = $comment.id
        path = $comment.path
        matched_commit = $matched
        body_preview = if ($comment.body.Length -gt 80) { $comment.body.Substring(0, 80) } else { $comment.body }
    }
}

# Summary
$matchedComments = $results | Where-Object { $null -ne $_.matched_commit }
$unmatchedComments = $results | Where-Object { $null -eq $_.matched_commit }

Write-Host "=== SUMMARY ==="
Write-Host "Total comments: $($results.Count)"
Write-Host "Matched to commits: $($matchedComments.Count)"
Write-Host "Unmatched: $($unmatchedComments.Count)"
Write-Host ""
Write-Host "=== MATCHED BY COMMIT ==="
foreach ($commit in $commitMap.Keys) {
    $cnt = ($matchedComments | Where-Object { $_.matched_commit -eq $commit }).Count
    Write-Host "${commit}: $cnt comments"
}
Write-Host ""
Write-Host "=== UNMATCHED COMMENT PATHS ==="
$unmatchedComments | ForEach-Object { Write-Host $_.path }
