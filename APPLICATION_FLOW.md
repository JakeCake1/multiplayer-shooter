# Multiplayer Shooter — Application and Game Flow

## Status

This document defines the first-pass client application flow and its relationship to PlayFab matchmaking, Fusion connectivity, and the authoritative server match lifecycle.

It is a design baseline for the first implementation milestone. Names and method signatures remain provisional until the local networking vertical slice validates them.

## Goals

- Represent the complete user journey from boot to playing again or exiting.
- Keep client application flow, backend matchmaking, and server match state separate.
- Give one application-layer owner responsibility for client transitions without creating a global `GameManager`.
- Keep UI passive and independent of PlayFab and Fusion SDKs.
- Make cancellation, failure, cleanup, and stale asynchronous results explicit.
- Allow the local networking slice to use direct connection data before PlayFab is integrated.

## Non-Goals

- Defining final C# interfaces or every concrete class.
- Abstracting individual Fusion primitives.
- Reconnect or session recovery after a mid-match disconnect.
- Party, lobby browser, rematch voting, MMR, or other post-MVP flows.
- Defining combat, movement, or lag-compensation mechanics.

## Separate State Machines

The runtime contains three cooperating state machines with different owners and sources of truth.

```text
Client Application Flow
Booting -> Menu -> Searching -> Connecting -> WaitingForPlayers -> Playing -> Results
             ^              \-> Menu          \-----------------------> Menu
             |                                                        |
             +---------------- Find Game Again / recoverable failure -+

Matchmaking / Allocation
Idle -> CreatingTicket -> WaitingForMatch -> Allocating -> Ready
  ^            \--------------- Cancelled / Failed -------/
  +--------------------------------------------------------+

Authoritative Server Match
WaitingForPlayers -> Starting -> Playing -> Finishing -> Finished
```

They communicate through explicit results and observations. They do not directly mutate one another's state.

## Client Flow State

The client flow is represented as a closed set of state variants rather than a single enum plus a bag of nullable fields. Each state carries only the data valid for that stage.

Conceptual variants:

- `Booting`: client composition and initial authentication are starting.
- `Menu`: authentication readiness and an optional user-facing problem are available to the view.
- `Searching`: one matchmaking operation is active; allocation may be an internal substage.
- `Connecting`: a server endpoint has been resolved and a Fusion connection attempt is active.
- `WaitingForPlayers`: the network session is connected and the server match is not yet playing.
- `Playing`: the server reports that authoritative play has started.
- `Results`: immutable final results reported by the server are available.

`MatchFound` and `Allocating` are not separate top-level screens. They are progress details inside `Searching`, because the MVP presents one continuous "Searching for game" experience.

Failure is not a permanent catch-all state. A failed operation first performs stage-specific cleanup, then returns to `Menu` with a structured problem that Presentation can display. Fatal boot/configuration failures may use a dedicated blocking error presentation because no usable menu can be entered.

### Authentication Readiness

Authentication starts during `Booting`. The client may show the menu shell once presentation is ready, but `Find Game` is enabled only when authentication is ready.

This separates the user-facing `Menu` state from the asynchronous readiness of authentication and avoids a dedicated authentication screen for the MVP. A recoverable authentication failure is shown in the menu with a retry action.

## Transition Ownership

A client-only application coordinator owns client flow transitions. The provisional name is `ClientFlowCoordinator`.

Its responsibilities are limited to:

- accepting user intents such as find game, cancel, retry, find game again, and exit;
- invoking application capabilities for authentication, matchmaking, and network sessions;
- serializing transitions and rejecting invalid intents for the current state;
- translating capability results into the next client flow state;
- observing authoritative match-state snapshots exposed by the network boundary;
- cancelling active work and cleaning up resources when leaving a stage;
- publishing the current client flow state for Presentation.

It must not:

- call PlayFab or Fusion SDK APIs directly;
- implement matchmaking ticket polling;
- own a `NetworkRunner`;
- decide damage, score, respawn, timer, or final results;
- manipulate UI Toolkit elements;
- own camera, input, audio, VFX, or scene-specific presentation;
- coordinate the dedicated server lifecycle.

This makes it an application use-case coordinator rather than a global game manager.

## Transition Rules

### Boot and Menu

1. Client Bootstrap builds the client composition.
2. The flow coordinator starts authentication readiness.
3. Presentation observes `Booting`, then `Menu`.
4. `Find Game` remains disabled until authentication is ready.
5. Authentication failure returns or remains in `Menu` with retry/exit options.

### Find Game

1. UI submits a `FindGame` intent.
2. The coordinator accepts it only from an authenticated `Menu` or from `Results` after session cleanup.
3. The coordinator enters `Searching` and starts one matchmaking operation.
4. PlayFab infrastructure owns ticket creation, polling, cancellation, and allocation translation.
5. A successful result contains project-owned connection data rather than PlayFab SDK models.
6. The coordinator enters `Connecting` and passes that data to the network-session capability.

For the local vertical slice, a local connection-data provider may supply the same application model without PlayFab. This is a composition choice, not a second flow.

The first local implementation uses two explicit bootstrap scenes:

- `LocalServer` contains only `ServerLifetimeScope` and starts Fusion in Server Mode.
- `LocalClient` contains only `ClientLifetimeScope` and starts Fusion in Client Mode.

Both roles use the same session identifier. Local launch arguments are intentionally project-specific so they do not collide with Unity arguments:

```text
--shooter-session <name>   default: local-1v1
--shooter-port <port>      server only; default: 27015
```

The local client uses `W`, `A`, `S`, and `D` for the first movement proof. Input is sampled only by the client runner, transported as Fusion input, and consumed by both the predicting input-authority client and the authoritative server in `FixedUpdateNetwork`.

The role is selected by composition, not by a runtime flag. This keeps the dedicated-server path from constructing client presentation or input services. The local launcher is temporary vertical-slice composition; production clients will receive equivalent neutral connection data from the matchmaking/allocation capability.

### Connect and Wait

1. Fusion infrastructure owns runner creation and the concrete connection attempt.
2. Successful transport/session connection moves the client to `WaitingForPlayers`.
3. A transport connection does not mean the match is playing.
4. The client observes the authoritative server match state through the network-session boundary.
5. Server state `Starting` remains represented by `WaitingForPlayers` in the top-level client flow; Presentation may show countdown data from the authoritative match snapshot.
6. Server state `Playing` moves the client to `Playing`.

### Match Completion

1. The server timer reaches zero and the server enters `Finishing`.
2. The server freezes or finalizes competitive outcomes.
3. The client does not calculate its own final score.
4. Once the server publishes `Finished` with final results, the client enters `Results`.
5. The network session may remain connected during a short results/grace period, but the results displayed are immutable.

### Find Game Again

1. UI submits `FindGameAgain` from `Results`.
2. The coordinator disconnects and disposes the completed network session.
3. Only after cleanup completes does it start a new matchmaking operation.
4. Failure to disconnect cleanly is logged but must not leave the flow indefinitely stuck; bounded cleanup is followed by returning to `Menu` with a problem.

### Exit

1. Exit is accepted from `Menu` and `Results`.
2. If exit is later exposed from active stages, the coordinator first cancels matchmaking or disconnects the session.
3. Application termination is requested through a small platform-facing capability so it can be replaced in tests.

## UI Boundary

Presentation observes a read-only client flow state or a presentation-specific projection of it. It sends user intents and does not perform transitions itself.

```text
UI Toolkit View
    | user intent
    v
Application Flow Coordinator
    | published state
    v
Presenter / View Model -> UI Toolkit View
```

Presentation is responsible for:

- selecting the visible screen;
- enabling and disabling actions based on published state;
- displaying progress, countdown, errors, HUD, and results;
- client-only feedback.

Presentation does not:

- inspect PlayFab ticket/allocation objects;
- inspect Fusion callbacks to decide application flow;
- infer match completion from a local timer;
- publish authoritative gameplay outcomes.

## PlayFab Boundary

PlayFab answers where and with whom to play. Its infrastructure adapter owns SDK-specific operations and translates them into application models.

The application needs capabilities equivalent to:

- ensure authentication readiness;
- begin one matchmaking/allocation operation;
- report progress if useful to Presentation;
- complete with neutral connection data;
- cancel the active operation;
- return structured, application-level failure information.

The neutral connection result should contain only data actually required to establish the Fusion client session, such as an endpoint, port, session identifier, and short-lived credentials if required. Its final shape will be defined when the local and PlayFab connection paths are implemented.

The application must not expose PlayFab SDK response types beyond `Shooter.Infrastructure.PlayFab`.

## Fusion Boundary

Fusion answers how the realtime session runs. Its infrastructure adapter owns:

- `NetworkRunner` lifecycle;
- connect and disconnect operations;
- Fusion callbacks;
- input transport;
- network spawning and despawning;
- replicated authoritative match-state observation;
- prediction and reconciliation details.

The application needs capabilities equivalent to:

- connect using neutral connection data;
- observe connection lifecycle changes;
- observe authoritative match state and final results;
- disconnect and dispose the active session.

Fusion callbacks are translated into application-level connection events. Presentation does not subscribe directly to Fusion to decide which application screen is active.

## Server Match Boundary

The dedicated server owns:

- admission and connected-player count;
- authoritative transition from waiting to countdown and play;
- match timer;
- combat outcomes, death, respawn, and score;
- finishing and immutable final results;
- grace period and shutdown request.

Clients receive a replicated match snapshot sufficient for Presentation and client flow. The snapshot conceptually includes:

- match phase;
- authoritative timing/countdown information;
- player identities needed for the match UI;
- current score during play;
- final results after finishing.

The precise replicated Fusion representation belongs in `Shooter.Infrastructure.Fusion`. Gameplay rules and application-facing snapshots should not inherit from Fusion types.

## Cancellation and Concurrency

Every asynchronous flow operation belongs to one flow attempt.

The coordinator assigns an attempt/version identifier and cancellation scope when starting authentication retry, matchmaking, or connection. A completion is applied only if it belongs to the current attempt and the expected current state.

This prevents scenarios such as:

- a cancelled matchmaking poll later starting a connection;
- a late connection callback replacing a newer menu state;
- a disconnect callback from an old runner affecting a new session;
- repeated button presses starting parallel tickets or runners.

User actions that start work are disabled or ignored while the corresponding transition is already in progress. Infrastructure cancellation must be idempotent.

## Failure Policy for MVP

| Failure | Required cleanup | Client outcome |
|---|---|---|
| Authentication failure | Dispose/release the failed attempt | `Menu`, Find Game disabled, retry/exit available |
| Matchmaking failure or timeout | Cancel/delete ticket | `Menu` with recoverable problem |
| User cancels search | Cancel/delete ticket | `Menu` without an error |
| Allocation failure | Cancel/delete remaining ticket state | `Menu` with recoverable problem |
| Connection failure | Stop and dispose runner | `Menu` with recoverable problem |
| Disconnect before play | Dispose session | `Menu` with recoverable problem |
| Disconnect during play | Dispose session; do not invent results | `Menu` with disconnect problem |
| Invalid/missing final results | Dispose session | `Menu` with protocol/session problem |
| Find Game Again cleanup failure | Bounded cleanup and logging | `Menu` with recoverable problem |
| Fatal configuration failure | Dispose any partially created resources | Blocking error with exit option |

Reconnect is outside the MVP. Opponent disconnect/forfeit policy is a server-match design decision and must be defined before combat completion; until then, the client only reflects the authoritative server outcome or a session failure.

## Capability Boundaries to Validate

The following boundaries are expected, but their final APIs must emerge from the first use cases:

- authentication readiness;
- matchmaking/allocation operation;
- network session lifecycle;
- authoritative match-state observation;
- client flow state observation and user intents;
- application exit;
- structured logging.

Avoid introducing interfaces for:

- every individual state class;
- every Fusion callback or replicated property;
- generic network objects, RPCs, or network variables;
- UI controls and views that do not need substitution;
- pure data-only gameplay objects solely for DI.

## Composition

Client Bootstrap composes:

- the client flow coordinator;
- PlayFab authentication and matchmaking adapters, or local development substitutes;
- Fusion client session infrastructure;
- input and client Presentation;
- logging and platform exit capabilities.

Server Bootstrap composes:

- Fusion server infrastructure;
- authoritative match orchestration and gameplay rules;
- server spawning/respawn/combat services;
- server logging and shutdown capability.

Server composition must not instantiate the client flow coordinator, UI, camera, audio, VFX, or local input readers.

## Implementation Order

1. Create assembly definitions and client/server composition roots.
2. Define the smallest client-flow state representation required by Boot and Menu.
3. Define neutral local connection data and the network-session capability needed by the local slice.
4. Implement local direct connection composition without PlayFab.
5. Prove dedicated server plus two clients, spawning, input, and predicted/synchronized movement.
6. Add authoritative server match phases and replicate a match snapshot.
7. Add combat, score, respawn, timer, and final results.
8. Complete Menu, Searching, HUD, and Results presentation.
9. Implement PlayFab authentication, matchmaking, and allocation behind the existing application boundary.
10. Add the failure and cancellation paths described above.

## Decisions Deferred Until Their Vertical Slice

- Exact C# names and signatures for capabilities and state variants.
- Fusion movement controller and prediction approach.
- Shooting, hit registration, and lag compensation.
- Scene loading and runner scene-management strategy.
- Opponent disconnect and forfeit semantics.
- Hosted server process arguments and PlayFab build configuration.
- Gameplay tuning values.

Each deferred decision should be documented when its implementing vertical slice begins, using the simplest design consistent with server authority and the existing assembly boundaries.

## Acceptance Checklist for This Design

- Client, matchmaking, and server match state are distinct.
- One client application coordinator owns client transitions.
- UI observes state and sends intent only.
- PlayFab allocation is translated before reaching Application.
- Fusion callbacks are translated before driving client flow.
- Server match state, timer, score, and results remain authoritative.
- Search cancellation and basic failure cleanup are explicit.
- Late asynchronous completions cannot advance an obsolete flow attempt.
- Play Again disconnects before starting a new matchmaking attempt.
- Local direct connection can exercise the same application flow without waiting for PlayFab.
