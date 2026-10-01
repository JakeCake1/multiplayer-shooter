# Multiplayer Shooter — Project Specification

## Purpose
Portfolio project: a minimal PC 1v1 multiplayer shooter that demonstrates production-minded multiplayer architecture, dedicated-server authority, networking fundamentals, backend matchmaking, and clean separation of gameplay from vendor-specific networking code.

The priority is engineering quality and a complete multiplayer lifecycle, not content volume.

## Product Scope (MVP)

### User flow
1. Launch game.
2. Main Menu: `Find Game` / `Exit`.
3. `Find Game` starts automatic 1v1 matchmaking. There is no lobby browser.
4. When a match is formed, a dedicated server is allocated and both clients connect.
5. Server waits until both players are connected, then starts a short countdown.
6. Players enter one small test arena with obstacles.
7. Each player can move, look, jump, and fire an automatic weapon.
8. Shots register hits and apply damage; players have health.
9. On death, the killer receives a score increment and the dead player respawns after a delay.
10. Death does not end the match. The match ends when the server-authoritative timer expires.
11. Results screen shows both players and their final scores.
12. From Results the player can `Find Game` again (disconnect and re-enter matchmaking) or `Exit`.
13. Finished dedicated-server instances receive a short grace period and then shut down.

### Explicit non-goals for MVP
- Lobby browser / room selection.
- Bots.
- Multiple weapons.
- Inventory/equipment systems.
- Character customization.
- Progression/ranks/MMR.
- Multiple maps.
- Complex abilities.
- Skill-based matchmaking.
- Peer-hosted/Host Mode gameplay.

## Technology Stack
- Engine: Unity 6.
- Language: C#.
- Networking: Photon Fusion 2.
- Network topology: Dedicated Server / Fusion Server Mode.
- Server build: Unity Dedicated Server/headless build.
- Backend: PlayFab.
  - Authentication.
  - Matchmaking (simple 1v1 queue).
  - Multiplayer Servers / server allocation.
- Dependency injection / composition: VContainer.
- UI: Unity UI Toolkit.
- Input: Unity Input System.
- Tests: Unity Test Framework + NUnit.
- Initial logging: thin project-owned logging abstraction/categories over Unity logging.
- Initial development deployment: local dedicated server + two local clients; hosted deployment comes after the full local vertical slice works.

## Responsibility Boundaries

### PlayFab
Answers **where and with whom to play**:
- player authentication/identity;
- 1v1 matchmaking queue;
- match formation;
- dedicated-server allocation/lifecycle integration.

PlayFab must not leak directly into UI or gameplay code.

### Photon Fusion 2
Answers **how the realtime networked match runs**:
- client/server connection;
- simulation ticks;
- input transport;
- replication;
- RPCs where appropriate;
- network object spawning;
- prediction/reconciliation mechanisms used by the chosen movement/combat implementation.

Fusion-specific code should be isolated so that a later NGO implementation is feasible.

### Dedicated Server
The server is the authoritative source of truth for competitive game state, including:
- match state and timer;
- authoritative movement/simulation decisions;
- shot validation / authoritative hit result;
- fire-rate/cooldown validation;
- authoritative ammo if ammo is included in MVP;
- damage;
- health;
- death;
- respawn;
- kills/score;
- final match results.

### Client
The client is responsible for:
- reading local input;
- responsive local prediction where appropriate;
- camera;
- UI/HUD;
- animation;
- audio;
- VFX;
- local presentation feedback such as recoil/muzzle flash.

A client must never be able to authoritatively tell the server that another player took damage or that a kill occurred.

## Core Networking Rule
For every mechanic, reason in three categories:

1. **Authority** — who decides the true game state? Competitive/shared truth belongs on the server.
2. **Prediction** — what may the client temporarily predict for responsiveness?
3. **Presentation** — what is purely visual/audio/UI and can remain client-side?

Short version: **the server decides what happened; the client decides how to present it.**

Prediction is not authority.

## Match State Machine
Authoritative server match state should conceptually follow:

`WaitingForPlayers -> Starting -> Playing -> Finishing -> Finished`

MVP end condition: authoritative match timer reaches zero.

## Client/Application Flow
Conceptual user/application flow:

`Boot -> Menu -> Searching -> MatchFound/Allocating -> Connecting -> Waiting -> Playing -> Results`

Technical substates such as allocation do not need separate user-facing screens; a single "Searching for game..." experience can cover them.

## Backend / Server Flow
Conceptually:

`Client -> PlayFab Authentication -> PlayFab Matchmaking -> Match Found -> PlayFab Multiplayer Server Allocation -> Unity Dedicated Server + Fusion -> Clients Connect -> Match -> Results -> Clients Disconnect -> Server Grace Period -> Server Shutdown`

## Portability Goal: Future NGO Migration
The project starts with Photon Fusion 2, but should avoid making gameplay rules dependent on Fusion APIs.

Target migration story:
- PlayFab may remain unchanged.
- Gameplay/domain logic remains mostly unchanged.
- Fusion infrastructure is replaced with an NGO infrastructure implementation where practical.

Do **not** build a custom networking framework or abstract every primitive. It is acceptable for infrastructure/network-facing components to directly use Fusion types. Abstract meaningful application capabilities and protect gameplay rules, not vendor APIs one-for-one.

## First Vertical Slice
Before building menus/backend/cloud polish, prove the realtime architecture locally:

`Dedicated Server + Client A + Client B -> connect -> spawn two simple player representations -> synchronized/predicted movement`

Then add combat, match rules, PlayFab lifecycle, and presentation incrementally.

## Current Design Status
Decided:
- product loop and MVP scope;
- automatic matchmaking rather than lobby browser;
- Unity 6 / C#;
- Photon Fusion 2 Server Mode;
- dedicated server authority;
- PlayFab Authentication + Matchmaking + Multiplayer Servers;
- VContainer, UI Toolkit, Input System;
- Authority / Prediction / Presentation rule;
- layered architecture and separate Fusion/PlayFab infrastructure assemblies;
- future NGO migration is a design consideration, not an immediate implementation target.

Not yet designed in detail:
- Application/Game Flow implementation;
- concrete interfaces/classes and their APIs;
- exact Fusion movement/prediction approach;
- exact shooting/hit-registration/lag-compensation implementation;
- PlayFab ticket/allocation integration details;
- scene strategy;
- server process configuration and hosted deployment;
- disconnect/reconnect/error handling;
- exact match duration, damage values, respawn delay, fire rate, health values;
- test plan beyond architectural intent.
