#!/usr/bin/env pwsh
# Batch reply to PR #31 comments with commit references

$j = Get-Content "$env:TEMP\pr31_comments.json" | ConvertFrom-Json

# Map comments to their appropriate replies
$replies = @(
    # IdentityTypeMapper comments - addressed by 818aa55
    @{id=2572532321; reply="Addressed in commit ``818aa55`` - Implemented thread-safe singleton pattern with proper locking on dictionary operations."},
    @{id=2572664660; reply="Addressed in commit ``818aa55`` - Added comprehensive documentation explaining the rationale, thread-safety guarantees, and removal criteria."},
    @{id=2587364658; reply="Addressed in commit ``818aa55`` - Race condition fixed with proper locking around dictionary operations."},
    @{id=2587373638; reply="Addressed in commit ``818aa55`` - Fixed documentation to accurately reflect method behavior."},

    # Null checks before base constructor - addressed by 0854b25
    @{id=2588023858; reply="Addressed in commit ``0854b25`` - Added null checks before base constructor calls to throw ArgumentNullException with proper parameter name."},
    @{id=2587410106; reply="Addressed in commit ``0854b25`` - QueryDefinition now validates input before base constructor invocation."},
    @{id=2587454847; reply="Addressed in commit ``0854b25`` - SOAP FieldDefinition null check moved before base constructor call."},
    @{id=2587408610; reply="Addressed in commit ``0854b25`` - SOAP QueryDefinition null guard added."},

    # VssConnectionAdapter.CommonStructureService - addressed by 8da0ba8
    # Note: Need to find the specific comment ID

    # LevelOrderEnumerator.Reset() - addressed by 68a4cba
    @{id=2587351405; reply="Addressed in commit ``68a4cba`` - LevelOrderEnumerator.Reset() now properly resets the queue state."},

    # IRelatedLink.LinkTypeEnd null checks - addressed by f38a97d
    @{id=2587353813; reply="Addressed in commit ``f38a97d`` - Added null checks for IRelatedLink.LinkTypeEnd access."},
    @{id=2587360397; reply="Addressed in commit ``f38a97d`` - WorkItemLinkType coercion methods now throw InvalidOperationException when factory is null."},

    # IExceptionExploder documentation - addressed by 0576995
    @{id=2587388252; reply="Addressed in commit ``0576995`` - Added comprehensive XML documentation for IExceptionExploder.Explode method."},

    # SOAP Revision indexer null checks - addressed by 49e782a1
    @{id=2587398917; reply="Addressed in commit ``49e782a1`` - Added null checks to SOAP Revision indexer parameters."},

    # Directory.Build.props NoWarn comments - addressed by 4ad698b7
    @{id=2572530199; reply="Addressed in commit ``4ad698b7`` - Moved CA/IDE suppressions to .editorconfig. CS8xxx nullable warnings remain suppressed in Directory.Build.props for gradual migration."},
    @{id=2583135878; reply="Addressed in commit ``4ad698b7`` - Refactored NoWarn list: moved analyzer rules to .editorconfig with suggestion severity, keeping only essential nullable warnings suppressed for gradual migration."},
    @{id=2587341167; reply="Addressed in commit ``4ad698b7`` - Directory.Build.props NoWarn list has been significantly reduced. CA/IDE rules moved to .editorconfig."},

    # WorkItemLinkTypeCollection IList - addressed by c4780db8
    @{id=2589816159; reply="Addressed in commit ``c4780db8`` - Changed WorkItemLinkTypeCollection constructor parameter from List<T> to IList<T> for flexibility."},

    # REST IdentityDescriptor, ProjectCollection - addressed by d35bf0b6
    @{id=2587365809; reply="Addressed in commit ``d35bf0b6`` - Added null validation for IdentityDescriptor in REST implementation."},
    @{id=2587411858; reply="Addressed in commit ``d35bf0b6`` - ProjectCollection constructor now validates input parameters."}
)

Write-Host "Posting $($replies.Count) replies..."
$successCount = 0
$failCount = 0

foreach ($r in $replies) {
    try {
        $result = gh api "repos/rjmurillo/Qwiq/pulls/31/comments/$($r.id)/replies" -X POST -f body="$($r.reply)" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $successCount++
            Write-Host "✅ Replied to comment $($r.id)"
        } else {
            $failCount++
            Write-Host "❌ Failed to reply to $($r.id): $result"
        }
    }
    catch {
        $failCount++
        Write-Host "❌ Error replying to $($r.id): $_"
    }
    Start-Sleep -Milliseconds 500  # Rate limiting
}

Write-Host ""
Write-Host "=== COMPLETE ==="
Write-Host "Success: $successCount"
Write-Host "Failed: $failCount"
