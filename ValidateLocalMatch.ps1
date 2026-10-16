[CmdletBinding()]
param(
    [switch]$RequireCompletedMatch,
    [switch]$RequireRepeatedMatch,
    [string]$LogsPath = ""
)

$validatorPath = Join-Path $PSScriptRoot 'Tools\ValidateLocalMatch.ps1'
$arguments = @{
    RequireCompletedMatch = $RequireCompletedMatch
    RequireRepeatedMatch = $RequireRepeatedMatch
}

if (-not [string]::IsNullOrWhiteSpace($LogsPath)) {
    $arguments.LogsPath = $LogsPath
}

& $validatorPath @arguments
