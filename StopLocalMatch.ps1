[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "Medium")]
param()

$stopScriptPath = Join-Path $PSScriptRoot "Tools\StopLocalMatch.ps1"
& $stopScriptPath -WhatIf:$WhatIfPreference
