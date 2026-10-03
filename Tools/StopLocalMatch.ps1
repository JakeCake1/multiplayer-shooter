[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "Medium")]
param()

$projectRoot = Split-Path -Parent $PSScriptRoot
$processStatePath = Join-Path $projectRoot "Builds\Local\Logs\local-match-processes.json"
$targetPaths = @(
    Join-Path $projectRoot "Builds\Local\Server\ShooterServer.exe"
    Join-Path $projectRoot "Builds\Local\Client\ShooterClient.exe"
)

$targetNames = $targetPaths |
    ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_) } |
    Sort-Object -Unique

$trackedProcesses = @()
if (Test-Path -LiteralPath $processStatePath) {
    try {
        $processState = @(Get-Content -LiteralPath $processStatePath -Raw | ConvertFrom-Json)
        foreach ($entry in $processState) {
            $hasExpectedExecutablePath = $targetPaths | Where-Object {
                [string]::Equals(
                    [System.IO.Path]::GetFullPath($_),
                    [System.IO.Path]::GetFullPath($entry.ExecutablePath),
                    [System.StringComparison]::OrdinalIgnoreCase)
            }

            if (-not $hasExpectedExecutablePath) {
                Write-Warning "Saved PID $($entry.PID) does not belong to a local Shooter executable; it will not be stopped."
                continue
            }

            $process = Get-Process -Id $entry.PID -ErrorAction SilentlyContinue
            if ($null -eq $process) {
                continue
            }

            $expectedStartTime = [datetime]::Parse(
                $entry.StartTimeUtc,
                [System.Globalization.CultureInfo]::InvariantCulture,
                [System.Globalization.DateTimeStyles]::RoundtripKind)
            $actualStartTime = $process.StartTime.ToUniversalTime()

            if ($actualStartTime -eq $expectedStartTime.ToUniversalTime()) {
                $trackedProcesses += $process
            }
            else {
                Write-Warning "PID $($entry.PID) was reused by another process; it will not be stopped from saved state."
            }
        }
    }
    catch {
        Write-Warning "Could not read local match process state. Falling back to executable path discovery. $($_.Exception.Message)"
    }
}

$trackedProcessIds = @($trackedProcesses | ForEach-Object { $_.Id })
$discoveredProcesses = @(
    foreach ($targetName in $targetNames) {
        foreach ($process in Get-Process -Name $targetName -ErrorAction SilentlyContinue) {
            if ($process.Id -in $trackedProcessIds) {
                continue
            }

            try {
                $processPath = $process.Path
            }
            catch {
                Write-Warning "Could not inspect PID $($process.Id); it will not be stopped. $($_.Exception.Message)"
                continue
            }

            if ([string]::IsNullOrWhiteSpace($processPath)) {
                Write-Warning "Could not determine the executable path for PID $($process.Id); it will not be stopped."
                continue
            }

            $isLocalMatchProcess = $targetPaths | Where-Object {
                [string]::Equals(
                    [System.IO.Path]::GetFullPath($_),
                    [System.IO.Path]::GetFullPath($processPath),
                    [System.StringComparison]::OrdinalIgnoreCase)
            }

            if ($isLocalMatchProcess) {
                $process
            }
        }
    }
)

$matchingProcesses = @($trackedProcesses) + @($discoveredProcesses) |
    Sort-Object Id -Unique

if ($matchingProcesses.Count -eq 0) {
    Write-Host "No local Shooter server or client processes are running."

    if (Test-Path -LiteralPath $processStatePath) {
        if ($PSCmdlet.ShouldProcess($processStatePath, "Remove stale local match process state")) {
            Remove-Item -LiteralPath $processStatePath -Force
        }
    }

    return
}

$stoppedProcesses = @()
$stopFailures = 0
foreach ($process in $matchingProcesses) {
    $description = "$($process.ProcessName) (PID $($process.Id))"
    if ($PSCmdlet.ShouldProcess($description, "Stop local match process")) {
        try {
            Stop-Process -Id $process.Id -Force -ErrorAction Stop
            $stoppedProcesses += [pscustomobject]@{
                Process = $process.ProcessName
                PID = $process.Id
                Status = "Stopped"
            }
        }
        catch {
            $stopFailures++
            Write-Warning "Failed to stop $description. $($_.Exception.Message)"
        }
    }
}

if ($stoppedProcesses.Count -gt 0) {
    $stoppedProcesses | Format-Table -AutoSize
}

if ($stopFailures -eq 0 -and (Test-Path -LiteralPath $processStatePath)) {
    if ($PSCmdlet.ShouldProcess($processStatePath, "Remove local match process state")) {
        Remove-Item -LiteralPath $processStatePath -Force
    }
}
