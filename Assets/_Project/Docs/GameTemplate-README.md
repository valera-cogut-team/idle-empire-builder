# GameTemplate layout (`Assets/_Project/`)

This tree is the **minimal modular stack** used in the book: Zenject `DiContainer`, `IModule` / `ModuleManager`, and feature modules (Core, UI, game).

## Bootstrap

| Path | Role |
|------|------|
| `Source/Bootstrap/AppEntryPoint.cs` | Unity entry: creates `DiContainer`, runs `AppBootstrap.InitializeAsync`. |
| `Source/Bootstrap/AppBootstrap.cs` | Registers Core modules, then **IdleEmpireBuilder** (`IdleEmpireEconomyModule` + `IdleEmpireWorldModule`), then UI / AppFlow. |

## Core (shared)

Under `Source/Modules/Core/`: Logger, LifeCycle (`IUpdateHandler` / `ILateUpdateHandler`), Input, Addressables, Storage, Timer, StateMachine, Pool, Audio, Effects, Shaker, etc.

**Performance note (book Ch. 8 / 14):** Prefer **one** gameplay tick via `ILifeCycleFacade` instead of many `MonoBehaviour.Update` scripts for simulation. Keep HUD subscriptions on reactive streams (UniRx) rather than polling every frame with throwaway strings.

## UI flow

Splash → Game is driven by **AppFlow** and **ScreenRouter**; it does not hard-code IdleEmpireBuilder. The game module spawns its world under **`IdleEmpireBuilderWorld`** when enabled.

## Chapter 11 sample (idle economy)

| Path | Role |
|------|------|
| `Source/Modules/Game/IdleEmpireBuilder/` | Idle sample split by responsibility: `IdleEmpireEconomyModule` (state/facade/content) + `IdleEmpireWorldModule` (runtime world/tick/presentation). |

## Editor tooling

**IdleEmpireBuilder → Project → Bootstrap Content** — creates Addressables entries, `IdleEmpireBuilderTuning` asset (if missing), UI screen prefabs, validates `AppEntryPoint` in the bootstrap scene.

Batch (CI): `-executeMethod EditorTools.IdleEmpireBuilderProjectTools.BatchBootstrap`
