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
    -> NetworkCharacterController predicts, collides, and reconciles movement
```

## Build

In Unity, use:

```text
Shooter > Local Development > Build Development Client and Dedicated Server
```

To delete the existing client and dedicated-server outputs without starting a build, use:

```text
Shooter > Local Development > Clean Builds
```

To clean both outputs and then rebuild both Development players, use:

```text
Shooter > Local Development > Build Development Client and Dedicated Server (Clean)
```

Cleaning removes `Builds/Local/Client` and `Builds/Local/Server`, including executables, debug symbols, player data, and copied Addressables content. `Builds/Local/Logs` is preserved. The clean rebuild command also invalidates Unity's incremental player-build cache for each target. Stop a running local match before cleaning because Windows may lock its executables.

This creates:

```text
Builds/Local/Client/ShooterClient.exe
Builds/Local/Server/ShooterServer.exe
```

Both outputs are Development builds with Script Debugging enabled, so Rider can attach to either `ShooterClient` process or the `ShooterServer` process. The server build uses Unity's `StandaloneBuildSubtarget.Server`; it is not a headless client build.

The build also rebuilds Addressables content. `Player`, `MatchState`, and the shared collision arena are loaded by address at runtime and are no longer serialized into the bootstrap scenes.

In Rider, use `Run > Attach to Unity Process` and select the exact PID printed by the local launcher.

## Run

From PowerShell at the repository root:

```powershell
.\RunLocalMatch.ps1 -PlayFabTitleId ABCDE
```

`PlayFabTitleId` is the public Title ID shown for the selected title in PlayFab Game Manager. It is required by the Unified SDK and is not a developer secret.

Optional arguments:

```powershell
.\RunLocalMatch.ps1 -PlayFabTitleId ABCDE -SessionName test-2 -Port 28000
```

The script launches the server first and waits until its log reports `Network: Connected` before launching either client. The readiness timeout defaults to 90 seconds and can be increased for a particularly slow Development build:

```powershell
.\RunLocalMatch.ps1 -PlayFabTitleId ABCDE -SessionName test-2 -Port 28000 -ServerReadyTimeoutSeconds 120
```

Each client authenticates through the PlayFab Unified SDK before opening the local main menu. The launcher supplies distinct development-only Custom IDs, `client-a` and `client-b`, so PlayFab creates or resolves two different players. Select `Find Game` in both client windows to connect them to the session started by the script. Matchmaking is still the local adapter in this slice; the screen then follows the authoritative lifecycle through connecting, waiting/countdown, play, and results, while `Find Game Again` disconnects the completed Fusion session before reconnecting.

The local launcher intentionally does not pass `--shooter-playfab-matchmaking-queue`, so it keeps using the local matchmaking adapter. To exercise the PlayFab ticket lifecycle, launch a Development client with an existing queue name:

```powershell
.\Builds\Local\Client\ShooterClient.exe --shooter-playfab-title-id ABCDE --shooter-player-id client-a --shooter-playfab-matchmaking-queue quick-1v1
```

That queue must have a minimum and maximum match size of two and PlayFab Multiplayer Server allocation enabled. Add a Region Selection Rule whose path is `Latencies`; before creating a ticket, the client requests the title's deployed QoS regions, measures their UDP round-trip time on port 3075, and sends the results in the ticket's `Latencies` attribute. Measurements are written with a `[PlayFab][QoS]` prefix. The client processes ticket state changes every 100 milliseconds only while the ticket is active and uses the resulting `MatchId` as the Fusion session name. Ticket status, allocation endpoint, region, and selected Fusion session are written with a `[PlayFab]` prefix. During `Searching`, Cancel invalidates the current flow attempt, deletes the active PlayFab ticket through cancellation, and returns the client to `Menu` without an error. The existing Searching and failure screens provide client-visible confirmation, so this backend slice does not add a separate debug overlay.

## PlayFab Multiplayer Server build

The official Unity GSDK source is vendored under `Assets/ThirdParty/PlayFabGSDK`. A dedicated server automatically selects PlayFab mode when the Multiplayer Server Agent provides `GSDK_CONFIG_FILE`; without that environment variable it keeps the existing local command-line flow.

Configure the PlayFab Multiplayer Server build with one UDP port named `game_port`. The process startup command should run the dedicated-server executable without local session arguments:

```text
ShooterServer.exe -batchmode -nographics -logFile -
```

The server starts the GSDK heartbeat, enters `StandingBy`, and waits for PlayFab allocation. When the Agent activates it, the server reads the GSDK `SessionId` and assigned `serverListeningPort`, then starts the Fusion Server session using those values. A GSDK termination callback gracefully stops Fusion before quitting the process. After the authoritative match reaches `Finished`, a PlayFab server keeps the results session alive for ten seconds, gracefully stops Fusion, and exits so the allocated server can be recycled. Local dedicated servers intentionally remain alive for `Find Game Again`. Development clients launched with a PlayFab matchmaking queue show the approximate remaining shutdown grace period in the debug overlay. The build pipeline temporarily allows the GSDK's local HTTP Agent transport only while producing a dedicated-server build and restores the project setting afterward; client builds retain the project's normal HTTP policy.

The match uses first-person controls. Use `W`, `A`, `S`, and `D` to move relative to the camera, `Space` to jump, the mouse to look, and the left mouse button to fire the automatic weapon. Mouse sensitivity is configured in the main menu from `0.25x` to `5.00x`, defaults to `2.50x`, and is persisted locally. The cursor locks when `Playing` begins; press `Escape` to release it and click the game view to capture it again. Fusion's `NetworkCharacterController` predicts movement, jumping, and collision for the owning client and reconciles them against the dedicated server. Both roles load the same Addressable arena before starting the network runner, so the ground, perimeter walls, and cover participate in identical simulation geometry. The dedicated server sanitizes yaw/pitch, derives the three-dimensional shot direction, confirms shots only during `Playing`, resolves the raycast, applies damage, and writes the result with a `[Weapon][Server]` prefix.

After launch, PowerShell prints a process table with role, PID, session, server port, and log path. Logs are separated into:

```text
Builds/Local/Logs/server.log
Builds/Local/Logs/client-a.log
Builds/Local/Logs/client-b.log
```

During `Playing`, the client loads the production HUD through Addressables. It shows the authoritative time remaining plus health and kills for both players; the local row is marked `You`, and the two rows use distinct blue and orange accents. A gold hit marker flashes over the crosshair for 150 milliseconds whenever the server increases the local player's replicated confirmed-hit counter. While the local player is dead, a centered production status shows the authoritative respawn countdown rounded up to whole seconds. Development clients additionally show role, PID, session, port applicability, current network state, replicated match phase, connected player count, phase time remaining, recently active authoritative shot counters, and the final match result in the upper-left corner. A player's shot counter appears when its replicated total increases, is marked `[LC]` when it came through the lag-compensated server path, and disappears after five seconds without another confirmed shot. Its world-space ray disappears one second after the last confirmed fire input. A miss draws the full configured range; a collision ends at the lag-compensated server hit point. Each player uses the same distinct color for its ray, shot counter, and result row. Health remains visible in the production HUD while the replicated player object exists. At zero health, movement and firing stop, and remote clients hide the player's renderers without despawning the network object; during `Playing`, the server restores the player at its original spawn point after three seconds and the renderers become visible again. When `Playing` ends, the server snapshots both scores and publishes the winner or draw independently of the player objects. The dedicated server writes process information with a `[LocalProcess]` prefix to its log and the final result with a `[MatchResult][Server]` prefix.

If either client disconnects during `Playing`, the server records that player as having forfeited, preserves both current scores, and moves through `Finishing` to `Finished`. The remaining client receives the standard Results screen with the connected opponent as the winner. A disconnect during the starting countdown instead returns the server to `WaitingForPlayers`.

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
