param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PlayFabTitleId,
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$QueueName
)

$projectRoot = Split-Path -Parent $PSScriptRoot
$clientPath = Join-Path $projectRoot 'Builds\Local\Client\ShooterClient.exe'
$logsPath = Join-Path $projectRoot 'Builds\Local\Logs'
$processStatePath = Join-Path $logsPath 'local-match-processes.json'
$shutdownSignalPath = Join-Path $logsPath 'local-match.shutdown'
$clientALog = Join-Path $logsPath 'playfab-client-a.log'
$clientBLog = Join-Path $logsPath 'playfab-client-b.log'

if (-not (Test-Path -LiteralPath $clientPath)) {
    throw "Development client build not found: $clientPath"
}

New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
Remove-Item -LiteralPath $shutdownSignalPath -Force -ErrorAction SilentlyContinue

function Start-PlayFabClient {
    param(
        [string]$PlayerId,
        [string]$LogPath
    )

    return Start-Process -FilePath $clientPath -ArgumentList @('--shooter-playfab-title-id', $PlayFabTitleId, '--shooter-player-id', $PlayerId, '--shooter-playfab-matchmaking-queue', $QueueName, '--shooter-shutdown-signal', $shutdownSignalPath, '-logFile', $LogPath) -PassThru
}

$clientAProcess = Start-PlayFabClient -PlayerId 'client-a' -LogPath $clientALog
$clientBProcess = Start-PlayFabClient -PlayerId 'client-b' -LogPath $clientBLog
$processState = @(
    [pscustomobject]@{ Role = 'PlayFab Client A'; PID = $clientAProcess.Id; ExecutablePath = $clientPath; StartTimeUtc = $clientAProcess.StartTime.ToUniversalTime().ToString('O'); Session = 'allocated by PlayFab'; Port = 'allocated by PlayFab'; PlayerId = 'client-a'; Log = $clientALog; ShutdownSignal = $shutdownSignalPath }
    [pscustomobject]@{ Role = 'PlayFab Client B'; PID = $clientBProcess.Id; ExecutablePath = $clientPath; StartTimeUtc = $clientBProcess.StartTime.ToUniversalTime().ToString('O'); Session = 'allocated by PlayFab'; Port = 'allocated by PlayFab'; PlayerId = 'client-b'; Log = $clientBLog; ShutdownSignal = $shutdownSignalPath }
)

$processState | ConvertTo-Json | Set-Content -LiteralPath $processStatePath -Encoding UTF8
$processState | Select-Object Role, PID, PlayerId, Log | Format-Table -AutoSize
Write-Host 'Select Find Game in both clients. Stop them with: .\StopLocalMatch.ps1'
