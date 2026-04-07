# Mobile / desktop smoke test — IdleEmpireBuilder sample

Use before tagging a book release build. Devices: at least **one** mid-tier phone (Android or iOS) plus **Editor** or standalone.

## Functional

1. Cold start: Splash visible briefly, then Game screen; no exceptions in Console.
2. **Passive income:** Gold increases over time without input (rate scales with building levels).
3. **Tap gold:** Tap / Fire1 on empty ground (not over UI, not on a building) — gold jumps by the configured tap amount.
4. **Upgrade:** Tap a building cube when you have enough gold — level increases, cost deducted, HUD updates.
5. **Persistence:** Stop Play mode and Play again — gold and building levels match the saved session.

## Feedback (polish)

6. Tap and upgrade produce short procedural blips; upgrade optionally nudges the camera via **Shaker** (tunable on `IdleEmpireBuilderTuning` asset).

## Performance (Profiler)

7. **CPU:** Gameplay tick should show **`IdleEmpireGameTickService.OnUpdate`** as the main game logic hotspot (not dozens of `Update` scripts).
8. **GC:** During a 60s run, avoid sustained per-frame allocations in the tick path; HUD updates should fire on reactive changes, not every frame with new string garbage (Profiler *GC Alloc* column).

## Safe area

9. On a notched device (or simulator), HUD top text stays inside **safe area** (no clipping under status bar).

## Addressables

10. Delete `Library` (optional clean), open project, run **IdleEmpireBuilder → Project → Bootstrap Content**, then **Play** — tuning loads from **`Config_IdleEmpireBuilderTuning`** or falls back to a runtime default ScriptableObject.

## Optional

11. **Airplane / background:** Note any `Application.runInBackground` or audio focus behaviour for your publisher QA checklist.
