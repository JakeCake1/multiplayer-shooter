param(
    [string]$SessionName = "local-1v1",
    [ValidateRange(1, 65535)]
    [int]$Port = 27015,
    [ValidateRange(1, 300)]
    [int]$ServerReadyTimeoutSeconds = 90
)

function Wait-LocalServerReady {
    param(
        [System.Diagnostics.Process]$Process,
        [string]$LogPath,
        [int]$TimeoutSeconds
    )

    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    $connectedPattern = '\[LocalProcess\]\s+Role:\s+Server.*Network:\s+Connected'

    while ([DateTime]::UtcNow -lt $deadline) {
        $Process.Refresh()
        if ($Process.HasExited) {
            throw "Dedicated server exited before becoming ready (exit code $($Process.ExitCode)). See: $LogPath"
        }

        if (Test-Path -LiteralPath $LogPath) {
            try {
                $logContent = Get-Content -LiteralPath $LogPath -Raw -ErrorAction Stop
                if ($logContent -match $connectedPattern) {
                    return
                }
            }
            catch [System.IO.IOException] {
                # Unity can briefly lock the log while flushing it. Retry until timeout.
            }
        }

        Start-Sleep -Milliseconds 250
    }

    throw "Dedicated server did not become ready within $TimeoutSeconds seconds. See: $LogPath"
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$serverPath = Join-Path $projectRoot "Builds\Local\Server\ShooterServer.exe"
$clientPath = Join-Path $projectRoot "Builds\Local\Client\ShooterClient.exe"
$logsPath = Join-Path $projectRoot "Builds\Local\Logs"

if (-not (Test-Path -LiteralPath $serverPath)) {
    throw "Dedicated server build not found: $serverPath"
}

if (-not (Test-Path -LiteralPath $clientPath)) {
    throw "Client build not found: $clientPath"
}

New-Item -ItemType Directory -Path $logsPath -Force | Out-Null

$serverLog = Join-Path $logsPath "server.log"
$clientALog = Join-Path $logsPath "client-a.log"
$clientBLog = Join-Path $logsPath "client-b.log"
$processStatePath = Join-Path $logsPath "local-match-processes.json"
$shutdownSignalPath = Join-Path $logsPath "local-match.shutdown"

# Avoid treating a successful line from a previous run as current readiness.
Set-Content -LiteralPath $serverLog -Value "" -Encoding UTF8
Remove-Item -LiteralPath $shutdownSignalPath -Force -ErrorAction SilentlyContinue

$serverProcess = Start-Process `
    -FilePath $serverPath `
    -ArgumentList @(
        "--shooter-session", $SessionName,
        "--shooter-port", $Port,
        "--shooter-shutdown-signal", $shutdownSignalPath,
        "-logFile", $serverLog) `
    -WindowStyle Hidden `
    -PassThru

Write-Host "Waiting up to $ServerReadyTimeoutSeconds seconds for the dedicated server to become ready..."

try {
    Wait-LocalServerReady `
        -Process $serverProcess `
        -LogPath $serverLog `
        -TimeoutSeconds $ServerReadyTimeoutSeconds
}
catch {
    if (-not $serverProcess.HasExited) {
        Stop-Process -Id $serverProcess.Id -Force
    }

    throw
}

Write-Host "Dedicated server is connected. Starting clients..."
Start-Sleep -Milliseconds 500

$clientAProcess = Start-Process `
    -FilePath $clientPath `
    -ArgumentList @(
        "--shooter-session", $SessionName,
        "--shooter-shutdown-signal", $shutdownSignalPath,
        "-logFile", $clientALog) `
    -PassThru

$clientBProcess = Start-Process `
    -FilePath $clientPath `
    -ArgumentList @(
        "--shooter-session", $SessionName,
        "--shooter-shutdown-signal", $shutdownSignalPath,
        "-logFile", $clientBLog) `
    -PassThru

$processState = @(
    [pscustomobject]@{
        Role = "Server"
        PID = $serverProcess.Id
        ExecutablePath = $serverPath
        StartTimeUtc = $serverProcess.StartTime.ToUniversalTime().ToString("O")
        Session = $SessionName
        Port = $Port
        Log = $serverLog
        ShutdownSignal = $shutdownSignalPath
    }
    [pscustomobject]@{
        Role = "Client A"
        PID = $clientAProcess.Id
        ExecutablePath = $clientPath
        StartTimeUtc = $clientAProcess.StartTime.ToUniversalTime().ToString("O")
        Session = $SessionName
        Port = "n/a"
        Log = $clientALog
        ShutdownSignal = $shutdownSignalPath
    }
    [pscustomobject]@{
        Role = "Client B"
        PID = $clientBProcess.Id
        ExecutablePath = $clientPath
        StartTimeUtc = $clientBProcess.StartTime.ToUniversalTime().ToString("O")
        Session = $SessionName
        Port = "n/a"
        Log = $clientBLog
        ShutdownSignal = $shutdownSignalPath
    }
)

$processState | ConvertTo-Json | Set-Content -LiteralPath $processStatePath -Encoding UTF8
$processState |
    Select-Object Role, PID, Session, Port, Log |
    Format-Table -AutoSize

Write-Host "Stop all local match processes with: .\StopLocalMatch.ps1"
