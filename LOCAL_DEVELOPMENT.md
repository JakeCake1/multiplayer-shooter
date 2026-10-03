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
Shooter > Local Development > Build Client and Dedicated Server
```

This creates:

```text
Builds/Local/Client/ShooterClient.exe
Builds/Local/Server/ShooterServer.exe
```

The server build uses Unity's `StandaloneBuildSubtarget.Server`; it is not a headless client build.

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

All three processes must use the same Photon Fusion AppId and region configuration. The local UDP port is bound by the dedicated server; Fusion clients join the named session through Photon Cloud.
