[CmdletBinding()]
param(
    [switch]$RequireCompletedMatch,
    [switch]$RequireRepeatedMatch,
    [string]$LogsPath = ""
)

function Get-PatternCount {
    param(
        [string]$Content,
        [string]$Pattern
    )

    return [regex]::Matches($Content, $Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
}

function Read-LocalMatchLog {
    param(
        [string]$Role,
        [string]$Path,
        [string]$ReadyPattern
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Role log not found: $Path"
    }

    $content = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
    if ([string]::IsNullOrWhiteSpace($content)) {
        throw "$Role log is empty: $Path"
    }

    $criticalErrorPattern = 'NullReferenceException|InvalidOperationException|Unhandled Exception|UnexpectedFailure|GameNotFound|Failed to start local|Crash!!!|Assertion failed'
    $criticalErrors = Select-String -InputObject $content -Pattern $criticalErrorPattern -AllMatches | ForEach-Object { $_.Matches.Value } | Sort-Object -Unique
    return [pscustomobject]@{
        Role = $Role
        Path = $Path
        Ready = $content -match $ReadyPattern
        PlayingCount = Get-PatternCount -Content $content -Pattern '\[Match\].*phase:\s+Playing;'
        FinishedCount = Get-PatternCount -Content $content -Pattern '\[Match\].*phase:\s+Finished;'
        ResultCount = Get-PatternCount -Content $content -Pattern '\[MatchResult\]\[Server\]'
        ResetCount = Get-PatternCount -Content $content -Pattern '\[Match\]\[Server\] Prepared the local session for the next match\.'
        CriticalErrors = @($criticalErrors)
    }
}

function Add-ValidationFailure {
    param(
        [System.Collections.Generic.List[string]]$Failures,
        [string]$Message
    )

    $Failures.Add($Message)
}

$projectRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($LogsPath)) {
    $LogsPath = Join-Path $projectRoot 'Builds\Local\Logs'
}

$definitions = @(
    [pscustomobject]@{ Role = 'Server'; FileName = 'server.log'; ReadyPattern = '\[LocalProcess\]\s+Role:\s+Server.*Network:\s+Connected' }
    [pscustomobject]@{ Role = 'Client A'; FileName = 'client-a.log'; ReadyPattern = '\[LocalProcess\]\s+Role:\s+Client.*Network:\s+Connected' }
    [pscustomobject]@{ Role = 'Client B'; FileName = 'client-b.log'; ReadyPattern = '\[LocalProcess\]\s+Role:\s+Client.*Network:\s+Connected' }
)

$results = foreach ($definition in $definitions) {
    $logPath = Join-Path $LogsPath $definition.FileName
    Read-LocalMatchLog -Role $definition.Role -Path $logPath -ReadyPattern $definition.ReadyPattern
}

$requiredMatchCount = 0
if ($RequireRepeatedMatch) {
    $requiredMatchCount = 2
}
elseif ($RequireCompletedMatch) {
    $requiredMatchCount = 1
}

$failures = [System.Collections.Generic.List[string]]::new()
foreach ($result in $results) {
    if (-not $result.Ready) {
        Add-ValidationFailure -Failures $failures -Message "$($result.Role) never reached Network: Connected."
    }

    if ($result.CriticalErrors.Count -gt 0) {
        Add-ValidationFailure -Failures $failures -Message "$($result.Role) contains critical errors: $($result.CriticalErrors -join ', ')."
    }

    if ($requiredMatchCount -gt 0 -and ($result.PlayingCount -lt $requiredMatchCount -or $result.FinishedCount -lt $requiredMatchCount)) {
        Add-ValidationFailure -Failures $failures -Message "$($result.Role) contains fewer than $requiredMatchCount complete match lifecycle(s)."
    }
}

$serverResult = $results | Where-Object Role -eq 'Server'
if ($requiredMatchCount -gt 0 -and $serverResult.ResultCount -lt $requiredMatchCount) {
    Add-ValidationFailure -Failures $failures -Message "Server published fewer than $requiredMatchCount match result(s)."
}

if ($RequireRepeatedMatch -and $serverResult.ResetCount -lt 1) {
    Add-ValidationFailure -Failures $failures -Message 'Server did not prepare the local session for another match.'
}

$results | Select-Object Role, Ready, PlayingCount, FinishedCount, ResultCount, ResetCount, @{ Name = 'CriticalErrors'; Expression = { $_.CriticalErrors.Count } } | Format-Table -AutoSize

if ($failures.Count -gt 0) {
    Write-Host 'Local match validation failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host "- $failure" -ForegroundColor Red
    }

    throw 'Local match validation failed.'
}

Write-Host 'Local match validation passed.' -ForegroundColor Green
