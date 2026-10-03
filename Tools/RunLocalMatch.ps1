param(
    [string]$SessionName = "local-1v1",
    [ValidateRange(1, 65535)]
    [int]$Port = 27015
)

$projectRoot = Split-Path -Parent $PSScriptRoot
$serverPath = Join-Path $projectRoot "Builds\Local\Server\ShooterServer.exe"
$clientPath = Join-Path $projectRoot "Builds\Local\Client\ShooterClient.exe"

if (-not (Test-Path -LiteralPath $serverPath)) {
    throw "Dedicated server build not found: $serverPath"
}

if (-not (Test-Path -LiteralPath $clientPath)) {
    throw "Client build not found: $clientPath"
}

Start-Process `
    -FilePath $serverPath `
    -ArgumentList @("--shooter-session", $SessionName, "--shooter-port", $Port) `
    -WindowStyle Hidden

Start-Sleep -Seconds 2

Start-Process -FilePath $clientPath -ArgumentList @("--shooter-session", $SessionName)
Start-Process -FilePath $clientPath -ArgumentList @("--shooter-session", $SessionName)
