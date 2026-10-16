# PlayFab Deployment

## 1. Build the server package

In Unity, select:

```text
Shooter > PlayFab Deployment > Build Production Dedicated Server Package
```

This creates a clean non-Development Unity Dedicated Server build and packages it as:

```text
Builds/PlayFab/ShooterServer.zip
```

The executable is at the root of the ZIP. `Package Existing Dedicated Server` recreates only the ZIP, while `Clean Build and Package` removes both PlayFab outputs.

## 2. Create the Multiplayer Servers build

In PlayFab Game Manager, open the title and go to `Multiplayer > Servers > New Build`.

Use the following initial test configuration:

| Setting | Value |
|---|---|
| Server type | Managed Windows / Windows Server Core |
| Asset | `ShooterServer.zip` |
| Start command | `C:\Assets\ShooterServer.exe -batchmode -nographics -logFile -` |
| Servers per machine | `1` |
| Port name | `game_port` |
| Protocol | `UDP` |
| Container port | `27015` |
| Region | One enabled region covered by the title quota |
| Standby servers | `1` while testing |
| Maximum servers | `1` initially |

Choose a VM family and region that are covered by the approved quota. Deployment normally takes several minutes. Continue only after the build is healthy and the region has a server in `StandingBy`.

Set the region's standby target back to `0` whenever hosted testing is paused so unused VMs do not continue consuming core hours.

## 3. Connect the matchmaking queue

Open `Multiplayer > Matchmaking`, edit the 1v1 queue, and configure:

| Setting | Value |
|---|---|
| Minimum match size | `2` |
| Maximum match size | `2` |
| Server allocation | Enabled |
| Build | The deployed Build ID; use a Build Alias after the first successful deployment |
| Region selection path | `Latencies` |
| Maximum latency | `1000 ms` for the first infrastructure test; tighten after validation |

The queue region rule must be compatible with a region enabled on the server build. The clients already send measured PlayFab QoS latency values under `Latencies`.

## 4. Run two clients

Build the Development client if necessary, then run from the repository root:

```powershell
.\RunPlayFabMatch.ps1 -PlayFabTitleId ABCDE -QueueName quick-1v1
```

Select `Find Game` in both windows. The expected flow is:

```text
Ticket created
-> match found
-> PlayFab server allocated
-> clients receive public IP, game_port, and MatchId
-> GSDK activates the server
-> Fusion connects both clients using MatchId as the session name
-> match reaches Playing
```

Client logs are written to:

```text
Builds/Local/Logs/playfab-client-a.log
Builds/Local/Logs/playfab-client-b.log
```

Stop the clients with:

```powershell
.\StopLocalMatch.ps1
```

## 5. Inspect the hosted server

The server log should contain these milestones:

```text
[PlayFab][Server] GSDK is standing by for a server allocation.
[PlayFab][Server] Allocation active.
[Match][Server] Authoritative phase: Playing
[Match][Server] Authoritative phase: Finished
[Server] Match finished. Process shutdown scheduled
```

After the ten-second results grace period, the Unity process stops and PlayFab recycles the game server. If allocation fails, inspect the queue event and the server instance logs before changing client or Fusion code.

## References

- [Deploying PlayFab multiplayer server builds](https://learn.microsoft.com/en-us/gaming/playfab/multiplayer/servers/deploying-playfab-multiplayer-server-builds)
- [Game Manager deployment walkthrough](https://learn.microsoft.com/en-us/xbox/playfab/multiplayer/servers/quickstart-for-multiplayer-servers-game-manager)
- [Integrating Matchmaking with Multiplayer Servers](https://learn.microsoft.com/en-us/xbox/playfab/multiplayer/matchmaking/multiplayer-servers)
- [PlayFab Multiplayer Servers best practices](https://learn.microsoft.com/en-us/gaming/playfab/multiplayer/servers/best-practices)
