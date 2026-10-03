[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "Medium")]
param(
    [ValidateRange(0, 60)]
    [int]$GracefulTimeoutSeconds = 10
)

$stopScriptPath = Join-Path $PSScriptRoot "Tools\StopLocalMatch.ps1"
& $stopScriptPath `
    -GracefulTimeoutSeconds $GracefulTimeoutSeconds `
    -WhatIf:$WhatIfPreference
