param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PlayFabTitleId,
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$QueueName
)

$launcherPath = Join-Path $PSScriptRoot 'Tools\RunPlayFabMatch.ps1'
& $launcherPath -PlayFabTitleId $PlayFabTitleId -QueueName $QueueName
