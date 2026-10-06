# AGENTS.md — Multiplayer Shooter

## Project Mission
Build a portfolio-quality minimal 1v1 PC multiplayer shooter in Unity 6. Prioritize correct multiplayer architecture, dedicated-server authority, understandable code, testability, and a complete end-to-end session lifecycle over feature count or visual content.

Read `PROJECT_SPEC.md` and `ARCHITECTURE.md` before proposing or implementing architecture.

## Fixed Decisions
Treat the following as accepted unless the user explicitly asks to revisit them:
- Unity 6 / C#.
- Photon Fusion 2.
- Fusion Dedicated Server / Server Mode; NOT Host Mode.
- Unity dedicated/headless server build.
- PlayFab Authentication + Matchmaking + Multiplayer Servers.
- Automatic 1v1 matchmaking; NO lobby browser.
- VContainer for composition/DI.
- UI Toolkit.
- Unity Input System.
- Assembly-definition boundaries.
- Separate `Shooter.Infrastructure.Fusion` and `Shooter.Infrastructure.PlayFab` assemblies.
- Architecture rule: Authority -> Prediction -> Presentation.
- Future NGO migration should be possible without rewriting gameplay rules, but do not create a custom networking framework to achieve it.

## Engineering Rules
1. Server is authoritative for shared competitive state.
2. Clients send intent/input, not authoritative outcomes. Never accept a client command equivalent to "deal N damage to player X" as truth.
3. Keep gameplay rules independent of Fusion and PlayFab wherever practical.
4. Keep PlayFab and Fusion SDK calls inside their infrastructure boundaries or explicitly justified network-facing adapters.
5. UI/presentation must not call PlayFab directly.
6. Do not create a global `GameManager` responsible for boot, matchmaking, networking, match rules, UI, and results.
7. Use VContainer at composition roots; do not turn every object into a service solely to use DI.
8. Prefer small explicit interfaces around application capabilities, not wrappers for every third-party type.
9. Do not introduce `INetworkObject`, `INetworkVariable`, `INetworkRPC`, etc. merely to mirror Fusion APIs.
10. Dedicated server must not initialize client-only presentation/input systems.
11. Use `.asmdef` references to enforce dependency direction rather than relying on conventions alone.
12. Avoid premature systems outside MVP: bots, progression, inventory, multiple weapons/maps, MMR, customization, abilities.
13. Keep code and naming straightforward enough to explain in a portfolio interview.
14. Prefer role-specific client and server implementations selected by their composition roots. Do not combine both execution paths in one class when separate classes can express the behavior directly.
15. Extract a named method when three or more related lines implement one responsibility.
16. Keep each project-owned C# statement on one physical line unless separate statements are delimited with semicolons. Do not apply this rule to vendored or generated code.
17. Use Addressables for runtime game scenes and content prefabs. Keep only the minimal client and server bootstrap scenes in Build Settings.
18. Keep no more than one project-owned C# class in a source file. Name the file after that class. Vendored and generated code are excluded.
19. Organize gameplay by feature under `Assets/Shooter/Features/<FeatureName>`. Every feature owns its own `.asmdef`; new gameplay logic starts in a new or existing feature directory rather than a shared catch-all gameplay assembly.
20. Until a feature has normal client-facing visual presentation, provide a client-only Development-build `DebugOverlay` that makes its behavior observable. When normal presentation makes that overlay redundant, ask the user for approval before removing it; never delete a feature `DebugOverlay` silently.

## Working Style for Codex
- Before making a substantial architectural change, state the proposed change and why it respects the documented boundaries.
- Do not silently replace an accepted technology or topology.
- If a requirement is not decided in the docs, mark it as a design choice and ask or propose options rather than presenting it as already agreed.
- Prefer incremental vertical slices that compile/run over creating a large speculative class hierarchy.
- When adding a dependency between assemblies, explain why the dependency is necessary.
- When Fusion or PlayFab APIs force a compromise in the ideal layering, prefer a clear adapter/boundary and document the compromise instead of adding abstraction for abstraction's sake.
- Keep client and server execution paths explicit.
- Add tests first/alongside code for pure gameplay rules when practical.

## Immediate Next Design Task
Do NOT start implementing the entire game yet.

The next task is to design the **Application/Game Flow** without a monolithic manager. Work through:

`Boot -> Menu -> Authentication readiness -> Searching -> Match allocation -> Connecting -> Waiting for players -> Playing -> Results -> Find Game again / Exit`

The design should determine:
- state representation;
- who owns transitions;
- how UI observes/requests transitions;
- how PlayFab matchmaking results feed connection setup;
- how Fusion connection/disconnection feeds flow state;
- how the server's authoritative match state is reflected on clients;
- cancellation and basic failure paths;
- which interfaces actually need to exist.

Only after this flow is agreed should concrete service/controller APIs and implementation classes be finalized.

## First Implementation Milestone After Flow Design
Local networking vertical slice:
1. Launch one dedicated server locally.
2. Launch two clients locally.
3. Connect both clients to the server using Fusion Server Mode.
4. Spawn two minimal player representations.
5. Send player input.
6. Demonstrate responsive/synchronized movement with server authority/prediction appropriate to Fusion.

Do not block this milestone on production PlayFab hosting or polished UI.

## Definition of MVP Done
A user can launch the PC client, choose Find Game, be automatically paired with another player, connect to an allocated dedicated server, play a timed 1v1 shooter match with movement/jump/automatic weapon/damage/health/death/respawn/score, see final results, and either search again or exit. Competitive outcomes are server-authoritative and the codebase maintains the documented vendor boundaries.
