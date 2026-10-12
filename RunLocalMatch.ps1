param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PlayFabTitleId,
    [string]$SessionName = "local-1v1",
    [ValidateRange(1, 65535)]
    [int]$Port = 27015,
    [ValidateRange(1, 300)]
    [int]$ServerReadyTimeoutSeconds = 90
)

$launcherPath = Join-Path $PSScriptRoot "Tools\RunLocalMatch.ps1"
& $launcherPath `
    -PlayFabTitleId $PlayFabTitleId `
    -SessionName $SessionName `
    -Port $Port `
    -ServerReadyTimeoutSeconds $ServerReadyTimeoutSeconds
