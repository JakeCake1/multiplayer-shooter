param(
    [string]$SessionName = "local-1v1",
    [ValidateRange(1, 65535)]
    [int]$Port = 27015
)

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

$serverProcess = Start-Process `
    -FilePath $serverPath `
    -ArgumentList @(
        "--shooter-session", $SessionName,
        "--shooter-port", $Port,
        "-logFile", $serverLog) `
    -WindowStyle Hidden `
    -PassThru

Start-Sleep -Seconds 2

$clientAProcess = Start-Process `
    -FilePath $clientPath `
    -ArgumentList @("--shooter-session", $SessionName, "-logFile", $clientALog) `
    -PassThru

$clientBProcess = Start-Process `
    -FilePath $clientPath `
    -ArgumentList @("--shooter-session", $SessionName, "-logFile", $clientBLog) `
    -PassThru

@(
    [pscustomobject]@{
        Role = "Server"
        PID = $serverProcess.Id
        Session = $SessionName
        Port = $Port
        Log = $serverLog
    }
    [pscustomobject]@{
        Role = "Client A"
        PID = $clientAProcess.Id
        Session = $SessionName
        Port = "n/a"
        Log = $clientALog
    }
    [pscustomobject]@{
        Role = "Client B"
        PID = $clientBProcess.Id
        Session = $SessionName
        Port = "n/a"
        Log = $clientBLog
    }
) | Format-Table -AutoSize
