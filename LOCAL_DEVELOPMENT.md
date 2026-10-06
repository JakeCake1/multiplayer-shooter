# Local Development

## First Networking Slice

The current local slice contains:

```text
Dedicated Server + Client A + Client B
    -> same Fusion session
    -> server spawns one player object per PlayerRef
    -> clients send movement and fire intent
    -> input-authority client predicts movement
    -> dedicated server validates automatic fire cadence
    -> server owns authoritative state
    -> NetworkTransform replicates and reconciles movement
```

## Build

In Unity, use:

```text
Shooter > Local Development > Build Development Client and Dedicated Server
```

This creates:

```text
Builds/Local/Client/ShooterClient.exe
Builds/Local/Server/ShooterServer.exe
```

Both outputs are Development builds with Script Debugging enabled, so Rider can attach to either `ShooterClient` process or the `ShooterServer` process. The server build uses Unity's `StandaloneBuildSubtarget.Server`; it is not a headless client build.

The build also rebuilds Addressables content. `Player` and `MatchState` are loaded by address at runtime and are no longer serialized into the bootstrap scenes.

In Rider, use `Run > Attach to Unity Process` and select the exact PID printed by the local launcher.

## Run

From PowerShell at the repository root:

```powershell
.\RunLocalMatch.ps1
```

Optional arguments:

```powershell
.\RunLocalMatch.ps1 -SessionName test-2 -Port 28000
```

The script launches the server first and waits until its log reports `Network: Connected` before launching either client. The readiness timeout defaults to 90 seconds and can be increased for a particularly slow Development build:

```powershell
.\RunLocalMatch.ps1 -SessionName test-2 -Port 28000 -ServerReadyTimeoutSeconds 120
```

Use `W`, `A`, `S`, and `D` to move in the focused client window. Hold the left mouse button to send automatic-fire intent; the dedicated server confirms shots only during the `Playing` phase and writes them with a `[Weapon][Server]` prefix.

After launch, PowerShell prints a process table with role, PID, session, server port, and log path. Logs are separated into:

```text
Builds/Local/Logs/server.log
Builds/Local/Logs/client-a.log
Builds/Local/Logs/client-b.log
```

Development clients also show role, PID, session, port applicability, current network state, replicated match phase, connected player count, phase time remaining, and recently active authoritative shot counters in the upper-left corner. A player's shot counter appears when its replicated total increases and disappears after five seconds without another confirmed shot. The dedicated server writes process information with a `[LocalProcess]` prefix to its log.

The server also owns and replicates the local match phases:

```text
WaitingForPlayers -> Starting -> Playing -> Finishing -> Finished
```

Phase transitions are written to server and client logs with a `[Match]` prefix. The current local tuning is two required players, a three-second countdown, a 60-second match, and a one-second finishing phase.

All three processes must use the same Photon Fusion AppId and region configuration. The local UDP port is bound by the dedicated server; Fusion clients join the named session through Photon Cloud.

## Stop

To stop all dedicated server and client processes launched for the local match, run from PowerShell at the repository root:

```powershell
.\StopLocalMatch.ps1
```

To preview which processes would be stopped without terminating them:

```powershell
.\StopLocalMatch.ps1 -WhatIf
```

The stop script checks saved process IDs, start times, and executable paths. It first asks the local builds to shut down their Fusion sessions cleanly, waits up to ten seconds, and force-stops only processes that do not exit. This prevents a Photon room from lingering and colliding with an immediate restart that uses the same session name. The graceful timeout can be overridden, or set to zero when testing the fallback:

```powershell
.\StopLocalMatch.ps1 -GracefulTimeoutSeconds 15
```

The script does not stop the Unity Editor or players from other projects.
