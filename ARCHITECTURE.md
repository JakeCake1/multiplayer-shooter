# Multiplayer Shooter — Architecture

## Architectural Intent
Build a small multiplayer game with explicit boundaries rather than a monolithic Unity `GameManager`. Vendor SDKs are infrastructure details. Gameplay rules should be testable and should not require Photon Fusion or PlayFab wherever reasonably possible.

## Top-Level Source Layout

```text
Assets/
└── Shooter/
    ├── Core/
    ├── Features/
    │   ├── Player/
    │   ├── MatchRules/
    │   ├── Weapon/
    │   └── Health/
    ├── Application/
    ├── Presentation/
    ├── Infrastructure/
    │   ├── Fusion/
    │   └── PlayFab/
    ├── Bootstrap/
    └── Tests/
```

Folder layout must be backed by assembly-definition boundaries; folders alone are not architecture.

## Assemblies
Start with these assemblies (names may be refined only with a concrete reason):

```text
Shooter.Core
Shooter.Features.Player
Shooter.Features.MatchRules
Shooter.Features.Weapon
Shooter.Features.Health
Shooter.Application
Shooter.Presentation
Shooter.Infrastructure.Fusion
Shooter.Infrastructure.PlayFab
Shooter.Bootstrap
```

Add separate test assemblies as needed.

## Dependency Direction

```text
Presentation
     |
     v
Application
     |
     v
Feature assemblies
     |
     v
   Core

Infrastructure.Fusion  ---> Application / feature assemblies / Core as required
Infrastructure.PlayFab ---> Application / Core as required

Bootstrap ---> all concrete modules required to compose a client or server
```

The exact references should remain as narrow as practical. Do not add a reference merely for convenience.

### Hard constraints
- `Shooter.Core` must not reference Fusion, PlayFab, VContainer, Presentation, or feature assemblies.
- Feature assemblies must not reference Fusion, PlayFab, UI, or Bootstrap.
- `Shooter.Application` must not reference concrete PlayFab/Fusion implementations.
- Presentation must not call PlayFab APIs directly.
- Gameplay must not call Photon APIs directly.
- Bootstrap is the composition root and is allowed to know concrete implementations.

Where practical, keep Core and domain-style Gameplay code pure C#. Do not force purity when a Unity-facing adapter is the simpler correct design; instead keep the boundary explicit.

## Layer Responsibilities

### Core
Lowest-level reusable project primitives and abstractions. Keep it small; do not create a generic framework.

Possible contents only when actually needed:
- result/error types;
- small time abstraction;
- shared identifiers/value types;
- minimal events/contracts.

Avoid "utility dumping ground" behavior.

### Features
Each gameplay feature lives under `Assets/Shooter/Features/<FeatureName>`, owns its own assembly definition, and remains independent of the networking vendor. Current features are `Player`, `MatchRules`, `Weapon`, and `Health`. Future gameplay behavior belongs to a new or existing feature rather than a shared catch-all gameplay assembly:
- health/damage rules;
- weapon state/rules;
- player gameplay state;
- score rules;
- match rules/state;
- respawn rules.

Example direction:

```text
Health (gameplay rule/state)
        ^
        |
network/presentation adapters observe or synchronize it
```

Do not make `Health` inherit `NetworkBehaviour` merely because health is replicated.

### Application
Coordinates use cases and flows. This layer knows what the application needs, but not which vendor implements it.

Likely capability boundaries (names/API are NOT final):
- authentication;
- matchmaking;
- network session/connect/disconnect;
- game/application flow;
- match orchestration interfaces where appropriate.

Illustrative only:

```csharp
public interface IAuthenticationService { ... }
public interface IMatchmakingService { ... }
public interface INetworkSession { ... }
```

Do not copy these signatures blindly; design them when the use cases are specified.

### Presentation
Client-facing behavior:
- UI Toolkit views/screens;
- HUD;
- camera;
- animation;
- audio;
- VFX;
- local visual feedback.

Presentation invokes application capabilities. It does not invoke PlayFab SDK calls directly.

### Infrastructure.Fusion
Concrete Photon Fusion integration. Direct Fusion usage is expected here.

Likely responsibilities:
- runner lifecycle;
- client/server network session;
- Addressables-backed loading of Fusion network prefabs;
- Fusion input bridge;
- network player/avatar components;
- network spawning;
- replication adapters;
- Fusion-specific prediction/reconciliation;
- server/client callbacks.

Do not create project-owned equivalents for every Fusion primitive such as `INetworkVariable`, `INetworkObject`, `INetworkRPC`, etc. That would be a second networking framework and is explicitly not desired.

The authoritative match adapter is split by responsibility: `FusionServerMatchController` advances pure `MatchStateMachine` rules, `FusionMatchState` contains only replicated state, and `FusionMatchStateObserver` handles client observation. `MatchPhaseRules` defines when gameplay input is accepted, while `FusionPlayerAvatar` applies that rule to the replicated phase on both the authoritative and predicted simulation paths. Network prefabs are resolved through `FusionNetworkAssetLoader`; bootstrap scenes do not serialize prefab references.

The initial combat adapter sends fire and planar aim intent through Fusion input. `FusionServerWeapon` validates aim and fire cadence, performs the authoritative `Physics.Raycast`, applies damage through `FusionPlayerHealthState`, and replicates the most recent confirmed trace endpoints for client debug presentation; clients only observe these server results. This local baseline tests hits against the server's current simulation state and intentionally does not include lag compensation yet.

Fusion `NetworkBehaviour` implementations live under `Infrastructure/Fusion/NetworkBehaviours/<FeatureName>` while remaining inside `Shooter.Infrastructure.Fusion`. This keeps network-facing components discoverable without creating an assembly per adapter.

### Infrastructure.PlayFab
Concrete PlayFab integration:
- authentication adapter;
- matchmaking ticket lifecycle;
- match result/allocation data translation;
- Multiplayer Servers integration needed by client/server lifecycle.

Expose project/application models across the boundary rather than PlayFab SDK models where useful.

### Bootstrap
Composition root using VContainer.

Maintain distinct client and dedicated-server compositions, conceptually:

```text
Bootstrap/
├── Client/
│   ├── ClientLifetimeScope
│   ├── ClientLocalProcessDiagnostics
│   └── LocalProcessDebugOverlay
└── Server/
    ├── ServerLifetimeScope
    └── ServerLocalProcessDiagnostics
```

Client composition may include:
- PlayFab authentication/matchmaking;
- Fusion client session;
- input;
- UI;
- camera/audio/VFX.

Server composition may include:
- Fusion server runner;
- authoritative match orchestration;
- combat authority;
- spawn/respawn;
- score/timer.

Dedicated server must not initialize client-only systems such as camera, HUD, audio, VFX, or local input readers.

The client and server bootstrap scenes are the only scenes kept in Build Settings. Runtime scenes and content prefabs use Addressables. Local builds rebuild Addressables content with the player build so client and dedicated-server outputs receive the same network prefab catalog.

## Runtime Authority Model

### Server Authority
Server owns shared competitive truth:
- match state;
- timer;
- health/damage/death;
- score/kills;
- respawn;
- validation of firing and hit outcome;
- authoritative simulation decisions.

### Client Prediction
Use prediction only where it improves responsiveness and can be corrected by authority. Primary candidates are local movement and immediate firing feedback; exact implementation is still to be designed.

### Client Presentation
Camera, HUD, animation, sound, VFX, crosshair, menus, and non-authoritative feedback remain client-side.

## State Machines
Do not collapse all runtime flow into one manager.

The detailed ownership, transitions, cancellation, and failure policy are defined in `APPLICATION_FLOW.md`.

### Client/application state
Conceptually:
`Boot -> Menu -> Searching -> Connecting -> Waiting -> Playing -> Results`

### Matchmaking/backend state
Conceptually:
`Searching -> MatchFound -> Allocating -> Ready/Failed`

### Server match state
Conceptually:
`WaitingForPlayers -> Starting -> Playing -> Finishing -> Finished`

These state machines may communicate through explicit application events/results, but they are different responsibilities.

## Session Lifecycle
1. Client boots and composes client services.
2. Client authenticates with PlayFab.
3. Main Menu appears.
4. Find Game creates/joins the 1v1 matchmaking process.
5. PlayFab forms a pair and allocates a Multiplayer Server.
6. Clients receive the data required to reach the assigned match server.
7. Unity dedicated server runs Fusion in Server Mode.
8. Both clients connect.
9. Server waits for 2/2 players, starts countdown, then starts authoritative match.
10. During play, clients send input; server owns competitive results; state is replicated back.
11. Death awards score and triggers delayed respawn; match continues.
12. Timer expires; server freezes/finalizes results and enters Finished.
13. Clients display Results.
14. Find Game disconnects and re-enters PlayFab matchmaking; Exit closes the client.
15. Finished server shuts down after a grace period.

## NGO Migration Constraint
The desired migration is architectural, not magical source compatibility.

Expected to change:
- network behaviours/components;
- runner/network-manager lifecycle;
- spawning APIs;
- RPC/replication code;
- prediction/network transform details.

Expected to survive largely unchanged:
- gameplay rules/domain state where isolated;
- application flow contracts where appropriately designed;
- PlayFab integration if backend architecture remains the same;
- presentation not tightly coupled to Fusion components.

The project must not over-engineer today merely to make every Fusion line replaceable tomorrow.
