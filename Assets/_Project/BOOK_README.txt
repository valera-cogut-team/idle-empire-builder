IdleEmpireBuilder — book companion sample (Unity HyperCasual / Mid-Core Architecture)

Unity version
  See ProjectSettings/ProjectVersion.txt (expected: 6000.x LTS / project pinned version).

Open this project
  1. Clone or unzip the source bundle.
  2. Open the project folder in Unity Hub (same major/minor as ProjectVersion.txt).
  3. Open scene: Assets/_Project/Scenes/BootstrapScene.unity
  4. Ensure bootstrap runs AppEntryPoint (menu: IdleEmpireBuilder → Project → Bootstrap Content if Addressables / prefabs are missing).

First run checklist
  - IdleEmpireBuilder → Project → Bootstrap Content (creates tuning asset if missing, UI screen prefabs, registers Addressables).
  - Press Play: Splash → Game; tap empty space — bonus gold; tap a building — upgrade if affordable; passive income ticks every frame via a single IUpdateHandler.
  - Gold and building levels persist via Storage module keys IdleEmpireBuilder.* in PlayerPrefs (default storage).

Controls
  - Keyboard: Space / Fire1 — same as tap (bonus gold or UI-independent interact).
  - Mouse: left button — tap / upgrade (raycast to building).
  - Touch: first finger — same as mouse when not over UI.

Layout
  - Gameplay: Assets/_Project/Source/Modules/Game/IdleEmpireBuilder/
  - Bootstrap registers IdleEmpireEconomyModule + IdleEmpireWorldModule in AppBootstrap.cs

Further reading (repo)
  - Assets/_Project/Docs/GameTemplate-README.md
  - Assets/_Project/Docs/Book-Mobile-Smoke-Test.md
  - Assets/_Project/Docs/IdleEmpireBuilder-Complete-Setup-RU.md
