# Local Development

## First Networking Slice

The current local slice proves:

```text
Dedicated Server + Client A + Client B
    -> same Fusion session
    -> server spawns one player object per PlayerRef
    -> clients send WASD input
    -> input-authority client predicts movement
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

In Rider, use `Run > Attach to Unity Process` and select the exact PID printed by the local launcher.

## Run

From PowerShell at the repository root:

```powershell
.\Tools\RunLocalMatch.ps1
```

Optional arguments:

```powershell
.\Tools\RunLocalMatch.ps1 -SessionName test-2 -Port 28000
```

The script launches the server first, waits briefly, then launches two clients. Use `W`, `A`, `S`, and `D` in the focused client window.

After launch, PowerShell prints a process table with role, PID, session, server port, and log path. Logs are separated into:

```text
Builds/Local/Logs/server.log
Builds/Local/Logs/client-a.log
Builds/Local/Logs/client-b.log
```

Development clients also show role, PID, session, port applicability, and current network state in the upper-left corner. The dedicated server writes the same information with a `[LocalProcess]` prefix to its log.

All three processes must use the same Photon Fusion AppId and region configuration. The local UDP port is bound by the dedicated server; Fusion clients join the named session through Photon Cloud.
