# Mikado_Online — Project Context for LLMs

> **One-line pitch:** A digital adaptation of the classic **Mikado (pick-up sticks)** board game — 3D physics-based, pick sticks without moving others.
> **Use:** Paste this file (or its relevant sections) into any LLM to give it full architectural context without needing to re-explore the repo.
> **Unity:** `6000.5.4f1` (Unity 6) • **Render Pipeline:** URP `17.5.0` • **Input:** Input System `1.19.0` • **Animation:** DOTween • **Code:** ~1010 LOC across 12 scripts • **State:** Local single-player (no networking yet — `Mikado_Online` is aspirational)

---

## 1. TL;DR

*   **What it is:** Player drops N sticks on a ground plane with physics; after they settle, player selects one stick at a time (outline highlight + camera tween), then attempts to pick it up by applying force. If any *other* stick moves more than `moveThreshold`, it's an illegal move (detected via position snapshot comparison). Sticks are removed via `SetActive(false)`. Scoring (win at `scoretoWin=25`) exists but is currently **dead code**.
*   **How it works core:** `CreateSticks` spawns → `StickCheck` snapshots positions after `gamestartTime` → `TouchManager` raycasts on `Stick` layer → `GameEventBus.TriggerTargetChange` → `StickCheck` toggles `ObjectPoints.SetSelection` → `ShaderControls` outlines via MaterialPropertyBlock + `CameraRotation` DOTweens → `PickUpController` applies force → `CollisionChecker` trigger detects illegal moves → `StickCheck.DetectStickMove` compares `Dictionary<Transform,Vector3>` distances.
*   **Architecture style:** MonoBehaviour-heavy, Manager-centric, **EventBus (Mediator)** via `static GameEventBus` with `Action<>` events. No DI, no ECS, no ScriptableObject data layer, no tests.
*   **Biggest gaps / blockers:** No networking; `EditorBuildSettings` has no real scenes registered; `ScoreSystem` not wired; `PickUpController` direction bug; `Presentation` depends on `Gameplay` (layer violation).

---

## 2. Tech Stack

| Layer | Detail |
| :--- | :--- |
| **Unity** | `6000.5.4f1 (d550df8bd089)` — `ProjectVersion.txt`, template `com.unity.template.urp-blank@17.0.11` |
| **Render Pipeline** | URP `17.5.0` (`com.unity.render-pipelines.universal`). Two quality assets: `Settings/PC_RPAsset` (2048 shadow, 50 dist, 4 cascades, HDR) + `Settings/Mobile_RPAsset` (1024 shadow, RenderScale 0.8, 1 cascade). Global settings `Settings/UniversalRenderPipelineGlobalSettings.asset`. |
| **Input** | Input System `1.19.0`, `activeInputHandler=1` (new only). Old `InputManager` axes unused. One `.inputactions` asset: `Assets/Mikado_Progress/Scripts/TouchControlls.inputactions` (map `Touch`). No `PlayerInput` component — manual `InputActionReference` polling. |
| **UI** | UGUI `2.5.0` + TextMeshPro (LiberationSans SDF, EmojiOne). Canvases in both scenes. |
| **Physics** | 3D Physics, Gravity `(0,-9.81,0)`, custom layer `Stick @ 6`, `QueriesHitTriggers=true`, `PhysMaterial/Bouncy.physicMaterial` on sticks. |
| **Animation / Tween** | DOTween (`Assets/Plugins/Demigiant/DOTween/*`, `DOTween.Modules.asmdef`, `Resources/DOTweenSettings.asset`). Symbols `DOTWEEN;DOTWEEN_NOPHYSICS;DOTWEEN_NOAUDIO` etc. |
| **Quality** | 2 levels: `Mobile` (excl. Standalone) + `PC` (excl. Android/iPhone). Mobile RenderScale 0.8, aniso 1, lodBias 1; PC aniso 2, lodBias 2. Current = PC. |
| **Packages (manifest)** | `2d.common@14.0.1`, `ide.visualstudio@2.0.28`, `inputsystem@1.19.0`, `urp@17.5.0`, `test-framework@1.7.0`, `ugui@2.5.0` + burst `1.8.29`, collections `6.5.0`, mathematics `1.4.0` (transitive). No custom registry. |
| **Platform** | `bundleIdentifier` still template `com.UnityTechnologies.com.unity.template.urpblank` (should be `com.designonsteroids.mikado`), `companyName DefaultCompany`, `bundleVersion 0.1.0`, `AndroidMinSdk 26`, `ARM64`, `IL2CPP`, `.NET Standard 2.1`. |
| **Networking** | **None.** No Netcode/Mirror/FishNet/Photon. `MultiplayerManager.asset` default. |

---

## 3. Directory Tree

```
Mikado_Online/                          # repo root
├── doc/
│   └── PROJECT_CONTEXT.md              # ← this file
├── Assets/
│   ├── Plugins/Demigiant/DOTween/      # DOTween.dll, DOTween.Modules.asmdef, Modules/*.cs
│   ├── Resources/DOTweenSettings.asset
│   ├── Settings/                       # URP + Volume
│   │   ├── PC_RPAsset.asset / PC_Renderer.asset
│   │   ├── Mobile_RPAsset.asset / Mobile_Renderer.asset
│   │   ├── UniversalRenderPipelineGlobalSettings.asset
│   │   ├── SampleSceneProfile.asset / DefaultVolumeProfile.asset
│   ├── Shader/OutlineBase.shader       # Custom/Outline (URP + outline pass cull front)
│   └── Mikado_Progress/                # ← main project content
│       ├── Materials/
│       │   ├── Borderline Shader/{Match_Outline.mat, Outline_mat.mat, Outline.mat}
│       │   ├── Grass/Grass.mat
│       │   ├── Phys Material/Bouncy.physicMaterial
│       │   └── Sky/Sky_LowPoly_01_Day_a.mat
│       ├── Models/Sticks/Match Stick/{Match.obj, Match.mtl, MatchAlbedo.png, MatchMetal.png, MatchNormal.png, MatchRough.png}
│       │   └── {grass_blades_1.fbx, ground_*.fbx, ground_wood_4.fbx, Non Metallic.mat}
│       ├── Prefabs/
│       │   ├── Hero Object/{Match.prefab, Match_Outline2.prefab}  # stick hero: Mesh+RB+ObjectPoints, layer Stick
│       │   └── {Grass.prefab, Grass Group.prefab}
│       ├── Scenes/
│       │   ├── Gameplay Scene.unity    # main (baked lighting)
│       │   ├── MainMenu.unity
│       │   └── Makido/{LightingData.asset, ReflectionProbe-0.exr}
│       ├── Scripts/
│       │   ├── Core/{GameEventBus.cs, ObjectPoints.cs, Mikado.Core.asmdef}
│       │   ├── Gameplay/{CollisionChecker.cs, CreateSticks.cs, PickUpController.cs, ScoreSystem.cs, StickCheck.cs, Mikado.Gameplay.asmdef}
│       │   ├── Presentation/{CameraRotation.cs, HoverMove.cs, ShaderControls.cs, TouchManager.cs, UIManager.cs, Mikado.Presentation.asmdef}
│       │   └── TouchControlls.inputactions
│       ├── Textures/{palette.png, Grass/Grass001_1K-JPG_*.jpg}
│       ├── TextMesh Pro/{Fonts, Resources, Shaders, Sprites}
│       └── UI/{Default.png, Default 1.png, Default 2.png}
│       └── Free Stylized Skybox/Panoramic/Textures/Sky_LowPoly_01_Day_a.png
├── Packages/{manifest.json, packages-lock.json}
├── ProjectSettings/
│   ├── ProjectVersion.txt
│   ├── ProjectSettings.asset           # productName Mikado_V2, company DefaultCompany
│   ├── GraphicsSettings.asset          # URP custom pipeline
│   ├── QualitySettings.asset           # Mobile / PC
│   ├── EditorBuildSettings.asset       # ⚠️ only disabled SampleScene — real scenes not registered
│   ├── TagManager.asset                # layer Stick @ 6
│   ├── DynamicsManager.asset / TimeManager.asset / etc.
│   └── URPProjectSettings.asset / EditorSettings.asset
├── .vscode/{extensions.json, launch.json, settings.json}
├── .gitignore / LICENSE / README.md (2 lines)
└── *.csproj / *.slnx                  # auto-generated, gitignored
```

**Counts:** 4 prefabs, 2 scenes, 12 custom C# (+8 DOTween modules), 1 custom shader, 7+ materials, 5 models.

---

## 4. Assembly Definitions & Dependency Graph

### 4.1 Asmdefs

| File | Name | References | Notes |
| :--- | :--- | :--- | :--- |
| `Assets/Mikado_Progress/Scripts/Core/Mikado.Core.asmdef` | `Core` | none | Base layer, `autoReferenced:true` |
| `Assets/Mikado_Progress/Scripts/Gameplay/Mikado.Gameplay.asmdef` | `Mikado.Gameplay` | `Core`, `TextMeshPro`, `InputSystem` | |
| `Assets/Mikado_Progress/Scripts/Presentation/Mikado.Presentation.asmdef` | `Mikado.Presentation` | `Core`, `Mikado.Gameplay`, `TextMeshPro`, `Mathematics`, `DOTween.Modules`, `InputSystem` | ⚠️ leaks — presentation should not depend on gameplay |
| `Assets/Plugins/Demigiant/DOTween/Modules/DOTween.Modules.asmdef` | `DOTween.Modules` | none | |

### 4.2 Dependency Graph

```
Core (no deps)
  ↑        ↑
  │        │
Gameplay ──┘
  ↑
  │
Presentation → DOTween.Modules, TextMeshPro, InputSystem, Mathematics
```

**Fix (if refactoring):** Move shared interfaces (e.g., `ISelectable`, `ITargetProvider`) into `Core`; make `Presentation` depend only on `Core`, have `Gameplay` publish via `GameEventBus`.

### 4.3 Key `.csproj` (auto-generated)

`Core.csproj`, `Mikado.Gameplay.csproj`, `Mikado.Presentation.csproj`, `DOTween.Modules.csproj`, `Assembly-CSharp.csproj`.

---

## 5. Architecture Overview

### 5.1 EventBus (Mediator) — the spine

`Assets/Mikado_Progress/Scripts/Core/GameEventBus.cs` — `static class GameEventBus` with `static event Action<>`:

```csharp
public static event Action<Transform>   OnTargetChange;              // TouchManager → StickCheck + CameraRotation
public static event Action<GameObject>  OnTargetCollisionDetected;   // CollisionChecker → StickCheck
public static event Action<int,bool>    OnCollision;                 // (would be) CollisionChecker → ScoreSystem  [currently dead]
public static event Action<Renderer,bool> OnStickSelected;            // ObjectPoints → ShaderControls
```

*   Sync, no queue, no WeakReference — callers must `OnEnable/OnDisable` subscribe correctly.
*   `RuntimeInitializeOnLoadMethod ResetAllListeners` is commented out — potential leak if scene reloads without unsubscribe (currently each manager unsubscribes).

### 5.2 State & Domain Model

*   `ObjectPoints.sticksState {Active, Selected, Disable}` — `UpdateState` toggles `Rigidbody.useGravity` (true=Active, false=Selected), sets `isTarget`/`isFlagged`, triggers `GameEventBus.TriggerSelection(renderer, bool)`.
*   `Disable` is unimplemented/no-op — reserved for collected sticks (currently they are `SetActive(false)` instead).

### 5.3 Snapshot Anti-Cheat (core rule)

`StickCheck.Dictionary<Transform,Vector3> position` stores settled positions after `gamestartTime`. On `OnTargetCollisionDetected` → `DetectStickMove` loops all *other* sticks, `Vector3.Distance(stored, current) > moveThreshold` → logs `Movement Detected!` and updates snapshot to new pos (so repeated micro-nudges can accumulate silently).

### 5.4 Managers

*   `CreateSticks` — bootstrap, owns spawn.
*   `StickCheck` — authoritative tracker + selection router + `GetStatus()` gate for input.
*   `TouchManager` — raycast input.
*   `CameraRotation` — orbit via polar coords + DOTween.
*   `ShaderControls` — MPB outline.
*   `CollisionChecker` — trigger/collision detection.
*   `PickUpController` — force pickup.
*   `ScoreSystem` — UI win/lose.
*   `UIManager` + `HoverMove` — menus.

### 5.5 Coroutine Initialization

`StickCheck.StartGame()`: `yield return WaitForSeconds(gamestartTime)` (physics settle) → snapshot `position` → `isposTake=true` → `InitScripts()` → unlocks `TouchManager.OnMouseMove`/`OnClick`. Fragile to `Time.timeScale` (paused during settle blocks it).

### 5.6 MaterialPropertyBlock Outline (good practice)

`ShaderControls` does `GetPropertyBlock → GetFloat/SetFloat("_OutlineWidth",1/0) → SetPropertyBlock` — zero GC, not `renderer.material` instancing (except commented damage glow does `renderer.material` — leak if re-enabled).

---

## 6. Scripts Reference

> 12 custom scripts, ~1010 LOC total. Relative paths, namespaces, key contracts.

### 6.1 Core

| Script | Namespace / Class | LOC | Purpose | Key Members / Events |
| :--- | :--- | :--- | :--- | :--- |
| `Assets/Mikado_Progress/Scripts/Core/GameEventBus.cs` | `Mikado.Core` / `static class GameEventBus` | 29 | Central pub/sub hub. | Events as §5.1. Methods: `TriggerTargetChange(Transform)`, `TriggerMovementDetected(GameObject)`, `TriggerCollision(int,bool)`, `TriggerSelection(Renderer,bool)` |
| `Assets/Mikado_Progress/Scripts/Core/ObjectPoints.cs` | `Mikado.Core` / `ObjectPoints : MonoBehaviour` | 89 | Per-stick data + state machine. Attached to `Prefabs/Hero Object/Match.prefab`. | `enum sticksState {Active,Selected,Disable}`; Fields: `[Serialize] int _points`, `string nameStick`, `bool isFlagged/isTarget`, `sticksState currentState`, `Rigidbody rb`, `Renderer ownRenderer`; Props `Points`; Methods `Init()`, `SetSelection(bool)` → `UpdateState(sticksState)` → `ToggleSelectionVisual(bool)` → `GameEventBus.TriggerSelection`. Gravity disabled when `Selected`. |

### 6.2 Gameplay (`Mikado.Gameplay`)

| Script | Class | LOC | Purpose | Key Members & Flow |
| :--- | :--- | :--- | :--- | :--- |
| `Assets/Mikado_Progress/Scripts/Gameplay/CreateSticks.cs` | `CreateSticks : MonoBehaviour` | 101 | Procedural spawner + jittered circular distributor. | Serialized: `int noOfSticks`, `GameObject prefabStick`, `float radius, radiusVariance`, `bool sortOnStart, randomRotation`, `float minRotation, maxRotation`, `float heightVariance, tiltVariance`; Internals: `Transform parentObject`, `List<Transform> childrens`, `StickCheck stickCheck`; Flow `Start() → CreateSticksOnCall()` instantiates N children under self → `ArrangeInCircle()` (angleStep `360/N` + `±radiusVariance` jitter, per-stick `radiusJitter/heightJitter`, `TransformPoint`, `Quaternion.Euler(tilt,yaw,tilt)`) → `stickCheck.Init(childrens)` |
| `Assets/Mikado_Progress/Scripts/Gameplay/StickCheck.cs` | `StickCheck : MonoBehaviour` | 155 | Game brain: validity + position tracker. Gates input. | Serialized: `TMP_Text noOfSticks, text`, `bool isLog`, `float gamestartTime, moveThreshold`; Fields: `bool isposTake`, `int stickCount`, `List<Transform> children`, `List<ObjectPoints> childrenScripts`, `Dictionary<Transform,Vector3> position`, `int _lastSelectedIndex=-1`; Subscribes `OnTargetCollisionDetected→MovementDetection`, `OnTargetChange→ChangeObjectState`; Methods: `Init(List<Transform>)` → `StartCoroutine(StartGame())` (wait + snapshot + `InitScripts()`), `ChangeObjectState(Transform)` (index via `children.IndexOf`, deselect prev via `_lastSelectedIndex`, select next, cache), `MovementDetection→DetectStickMove` (distance > threshold → collect update list → rewrite `position`), `OnStickCollected` (invalidate cache, `position.Remove`), `GetStatus()=>isposTake`; Guards for parallel-array desync. |
| `Assets/Mikado_Progress/Scripts/Gameplay/CollisionChecker.cs` | `CollisionChecker : MonoBehaviour` | 53 | Illegal-move / collection detector. Needs Trigger+Collider. | `OnTriggerEnter(Collider other)` if `other.GetComponent<ObjectPoints>().isTarget` → `TriggerTargetChange(null)` + `TriggerMovementDetected(other.gameObject)` + set `isFlagged`; `OnCollisionEnter(Collision)` if `isFlagged` → `TakeThis(obj)` (`SetActive(false)`) else set `isFlagged=true`. Scoring block (`TriggerCollision`) is **commented out** — dead path. Uses `isFlagged` double-flag hack. |
| `Assets/Mikado_Progress/Scripts/Gameplay/PickUpController.cs` | `PickUpController : MonoBehaviour` | 45 | Force-based pick-up (listens for target then applies force on action). | Serialized: `InputActionReference clickAction`, `float forceMagnitude`, `Transform forceDirection`; Field `Transform pickUpObject` (cached via `OnTargetChange`); On `clickAction.performed` → `rb.useGravity=false`, `rb.AddForce(forceDirection.position * forceMagnitude, ForceMode.Force)` — ⚠️ bug: `position` not direction (should be `forward`/normalized). `Force` maybe should be `Impulse`. |
| `Assets/Mikado_Progress/Scripts/Gameplay/ScoreSystem.cs` | `ScoreSystem : MonoBehaviour` | 53 | Win/lose UI. Currently dead — no `OnCollision` fired. | Serialized: `TMP_Text scoreUI`, `GameObject winUI/lostUI`, `int scoretoWin=25, totalavailableScore=31, currentScore`; Subscribes `OnCollision += Checker(int,bool)`; `Checker` adds/subtracts, `scoreUI.text=$"{currentScore}/{scoretoWin}"`, `SetActive(winUI/lostUI)` at thresholds. Needs wiring to `CollisionChecker` commented block. |

### 6.3 Presentation (`Mikado.Presentation`)

| Script | Class | LOC | Purpose | Key Members & Flow |
| :--- | :--- | :--- | :--- | :--- |
| `Assets/Mikado_Progress/Scripts/Presentation/CameraRotation.cs` | `CameraRotation : MonoBehaviour` | 171 | Orbit camera with zoom/height + event-driven `LookAt` DOTween. | Serialized: `Transform defaultTarget`, `float rotationSpeed=90, zoomSpeed=5, heightSpeed=5, orbitRadius=17, min/max OrbitRadius 2/17, heightOffset=5, targetChangeDuration=0.5`, `InputActionReference camRotationControls/camZoomControls`; Fields: `float currentAngle`, `Transform currentTarget`, `Vector3 lookTarget`, `Tween targetTween`, `Vector2 inputDirection`, `float zoomDirection`, `bool isMoving`; `OnEnable` enables actions + `OnTargetChange→HandleTargetChange(null→default)` → `DOTween.To(()=>lookTarget,…).SetEase(InOutQuad)`; `Update` reads `inputDirection.x→currentAngle`, `y→heightOffset`, `zoom→orbitRadius`, calls `UpdateCameraPosition()` (polar: `x=sin(rad)*radius,z=cos(rad)*radius,pos=lookTarget+(x,height,z),LookAt`) |
| `Assets/Mikado_Progress/Scripts/Presentation/ShaderControls.cs` | `ShaderControls : MonoBehaviour` | 60 | Outline / MPB controller (non-GC). | `string _OutlineWidth="_OutlineWidth"`; `MaterialPropertyBlock propertyBlock`; Subscribes `OnStickSelected→ToggleSelectionState(Renderer,bool)` (`GetPropertyBlock→SetFloat(1/0)→SetPropertyBlock`). Commented `DamageGlow` uses `renderer.material` (instanced leak). Matches `Shader/OutlineBase.shader` gate `_OutlineWidth>0.5 ? _OutlineWidth01 : 0`. |
| `Assets/Mikado_Progress/Scripts/Presentation/TouchManager.cs` | `TouchManager : MonoBehaviour` | 137 | Primary raycaster (click → select). WIP drag remnants. | Serialized: `ShaderControls shaderControls`, `StickCheck stickCheck`, `TMP_Text debugUI`, `InputActionReference clickAction, mousePosAction`; Fields: `LayerMask layerMask = Stick`, `Camera mainCamera`, `Vector2 mousePos`, `Renderer hoveredRenderer`, `GameObject selectedCube`, `bool isDragging, Vector3 dragOffset` (dead); Imports `Mikado.Gameplay`+`Mathematics`; `Awake` gets `Sticker` mask + `Camera.main`; `OnMouseMove→mousePos` only if `stickCheck.GetStatus()`; `OnClick` raycasts `ScreenPointToRay(mousePos)` vs `Stick` → `debugUI.text + TriggerTargetChange(hit.transform)`; `CheckHoverOnLayerMask` commented, `isDragging` logic disabled. |
| `Assets/Mikado_Progress/Scripts/Presentation/UIManager.cs` | `UIManager : MonoBehaviour` | 47 | Menu/pause. | Serialized: `TMP_Text debugUI`, `bool paused`; `PauseButton()` toggles `Time.timeScale 1↔0`; `RestartButton()` → `SceneManager.LoadScene(active)` or `debugUI="Un Pause Scene"` if paused; `NextScene()` → `LoadScene(buildIndex+1)` — ⚠️ breaks due to missing build settings. |
| `Assets/Mikado_Progress/Scripts/Presentation/HoverMove.cs` | `HoverMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler` | 71 | Reusable UI hover animation. | `enum MoveAxis{X,Y,Z}`; Serialized `MoveAxis axis=Y, float distance=20, smoothTime=0.15`; Fields `Vector3 originalPosition, targetPosition, velocity, bool isHovered`; `Awake` caches `localPosition`; `Update` `SmoothDamp(localPosition→targetPosition (offset axis*distance if hovered))`. Used on `MainMenu Quit_Button`. Requires EventSystem. |

**Total:** 12 scripts, ~1010 LOC + 203L `.inputactions` JSON.

---

## 7. Gameplay Flows (Sequence)

### 7.1 Startup

```
CreateSticks.Start()
  → GetComponents(Transform, StickCheck)
  → for i in 0..noOfSticks-1: Instantiate(prefabStick, parent)
  → ArrangeInCircle()  # jittered radius/height/tilt
  → StickCheck.Init(childrens)
       → Clear() + StartCoroutine(StartGame())
            → yield WaitForSeconds(gamestartTime)  # physics settle
            → foreach item in children: position.Add(item, item.position)
            → isposTake = true
            → noOfSticks.text = children.Count
            → InitScripts() # GetComponent<ObjectPoints> + Init()
            → Log("Goo!")
```

`TouchManager` gates `OnMouseMove`/`OnClick` on `GetStatus()==true` — clicks ignored until settled.

### 7.2 Selection

```
TouchManager.OnClick (InputAction Press)
  → Physics.Raycast(ScreenPointToRay(mousePos), Stick layer)
  → debugUI.text = hit info
  → GameEventBus.TriggerTargetChange(hit.transform)
       ├─→ StickCheck.ChangeObjectState(Transform)
       │     → IndexOf(children) → if same as _lastSelectedIndex return
       │     → prev = childrenScripts[_lastSelectedIndex]; prev.SetSelection(false)
       │     → next = childrenScripts[index]; next.SetSelection(true) → ObjectPoints.UpdateState(Selected)
       │     │     → rb.useGravity=false, isTarget=true, TriggerSelection(renderer,true)
       │     │          → ShaderControls.ToggleSelectionState (MPB _OutlineWidth=1)
       │     → _lastSelectedIndex = index
       └─→ CameraRotation.HandleTargetChange(Transform)
             → if null → defaultTarget else newTarget
             → DOTween.To(()=>lookTarget, newPos, 0.5).SetEase(InOutQuad) + UpdateCameraPosition() each frame
```

Selecting same stick twice is no-op; deselect old before selecting new. `null` from `CollisionChecker` is **intentionally ignored** (`if(selectedTransform==null) return` — comment D1).

### 7.3 Pickup (Force)

```
PickUpController: cached pickUpObject via OnTargetChange
User presses PickUp (Space / TouchControlls.Touch.PickUp)
  → clickAction.performed → HandlePickUp
       → rb.useGravity=false
       → rb.AddForce(forceDirection.position * forceMagnitude)  # bug: position not direction
```

`forceDirection` is a `Transform` in scene (`PickUp ForceDirection`); using `.position` as vector gives world-position-as-direction. Fix: `forceDirection.forward.normalized * magnitude` or `(forceDirection.position - pickUpObject.position).normalized`.

### 7.4 Removal / Illegal Move Detection

```
CollisionChecker.OnTriggerEnter(Collider other)  # trigger volume (ground?)
  → if other.ObjectPoints.isTarget:
       → GameEventBus.TriggerTargetChange(null)       # deselect
       → GameEventBus.TriggerMovementDetected(other.gameObject)  # → StickCheck.MovementDetection
            → DetectStickMove(selected)  # for all other sticks, distance > moveThreshold?
            │     → collect sticksToUpdate, Log Movement Detected
            │     → rewrite position[transform] = newPos  # masks future micro-moves
            → OnStickCollected(selected) # _lastSelectedIndex=-1 if was selected; position.Remove(selected)
       → isFlagged = true  (on self)
Otherwise OnCollisionEnter(Collision) flag logic → TakeThis(obj) -> obj.SetActive(false)
TakeThis: obj.SetActive(false) # stick disappears; still in children/childrenScripts lists but not in position dict
```

**Scoring path (currently commented):** `CollisionChecker` would `TriggerCollision(points,bool)` → `ScoreSystem.Checker` → win if `currentScore >= scoretoWin` else decrement `totalavailableScore` → lose if `< scoretoWin`. Not active — win/lose canvases never fire.

---

## 8. Input System

### 8.1 Asset

`Assets/Mikado_Progress/Scripts/TouchControlls.inputactions` — single map `Touch`:

| Action | Type | Expected Control | Bindings | Consumer |
| :--- | :--- | :--- | :--- | :--- |
| `Press` / alias `clickAction` | Button | `Mouse/leftButton`, `Touchscreen/press` | `TouchManager.clickAction` (`performed/canceled`) | Raycast select |
| `Move` / `mousePosAction` | Value Vector2 | `Mouse/position`, `Touchscreen/touch*/position` | `TouchManager.mousePosAction` → `OnMouseMove(ctx.ReadValue<Vector2>)` | Stores `mousePos` |
| `Rotate` / `camRotationControls` | Value Vector2 (2DVector composite) | `Keyboard/WASD` (Up/Down/Left/Right) | `CameraRotation.camRotationControls` | `inputDirection` → `currentAngle`/`heightOffset` |
| `Zoom` / `camZoomControls` | Value Axis (1D) | `Keyboard/Q` negative, `Keyboard/E` positive` | `CameraRotation.camZoomControls` | `zoomDirection` → `orbitRadius` |
| `PickUp` / `clickAction` (in PickUpController) | Button | `Keyboard/space` (TouchControlls.Touch.PickUp) | `PickUpController.clickAction` | `AddForce` |

*   ControlSchemes empty. No `PlayerInput` component — actions wired as `InputActionReference` serialized fields.
*   Confusing naming: `clickAction` in both `TouchManager` (means Press) and `PickUpController` (means PickUp/Space) — distinct assets with same field name.

### 8.2 Camera Input Handling

`CameraRotation.Update()`:
```csharp
inputDirection = camRotationControls.action.ReadValue<Vector2>();
zoomDirection  = camZoomControls.action.ReadValue<float>();
currentAngle  += inputDirection.x * rotationSpeed * Time.deltaTime;
heightOffset  += inputDirection.y * heightSpeed  * Time.deltaTime;
orbitRadius   -= zoomDirection    * zoomSpeed    * Time.deltaTime; // clamped min/max 2/17
UpdateCameraPosition(); // polar
```

`TouchManager` polls `mousePosAction` in `OnMouseMove` callback; raycast happens on `clickAction.performed`.

---

## 9. Scenes, Prefabs, Materials, Shaders, Textures

### 9.1 Scenes

| Scene | Build Setting | Key Contents (from YAML) |
| :--- | :--- | :--- |
| `Assets/Mikado_Progress/Scenes/Gameplay Scene.unity` | **Not in `EditorBuildSettings`** (only disabled `Assets/Scenes/SampleScene.unity` entry) — builds will fail; `SceneManager.LoadScene(buildIndex+1)` broken | ~48 MonoBehaviours. GameObjects: `CreateSticks` (with `CreateSticks`+`StickCheck`), `Camera` (`CameraRotation`), `TouchManager`, `ShaderControls`, `PickUpController`, `ScoreSystem`, `UIManager`, Directional Light, Volume, `Grass`/`Ground` meshes, `PickUp ForceDirection`, UGUI `Canvas` (TMP). `RenderSettings.skybox=Sky_LowPoly_01_Day_a.mat`, `LightingData.asset` in `Makido/` |
| `Assets/Mikado_Progress/Scenes/MainMenu.unity` | Also **not in build settings** | `Quit_Button` (`RectTransform`+`HoverMove` axis X distance -100) + `Text (TMP)`, likely Play/Settings buttons, Canvas, EventSystem. Lighter. |
| `Assets/Mikado_Progress/Scenes/Makido/LightingData.asset` + `ReflectionProbe-0.exr` | — | Baked GI for Gameplay Scene. |

**Fix:** In `File > Build Profiles`, add `MainMenu.unity` (index 0) then `Gameplay Scene.unity` (index 1), disable stale `SampleScene`.

### 9.2 Prefabs (4)

*   `Assets/Mikado_Progress/Prefabs/Hero Object/Match.prefab` — **canonical stick**: `MeshFilter` (`Models/Sticks/Match Stick/Match.obj`), `MeshRenderer` (`Materials/Borderline Shader/Match_Outline.mat` → `OutlineBase.shader`), `Rigidbody` (gravity on, Bouncy physMat), `MeshCollider`, `ObjectPoints` (`_points`, `nameStick`, `isFlagged/isTarget`, `currentState`). Layer `Stick`.
*   `Assets/Mikado_Progress/Prefabs/Hero Object/Match_Outline2.prefab` — variant (outline test without MPB? inspect if outlining differs).
*   `Assets/Mikado_Progress/Prefabs/Grass.prefab` + `Grass Group.prefab` — env cluster (`grass_blades_1.fbx`, `Grass.mat`).

Spawn: `CreateSticks` does `Instantiate(prefabStick, parentObject)` N times.

### 9.3 Custom Shader

`Assets/Shader/OutlineBase.shader` — `Shader "Custom/Outline"` (URP):

*   Pass 1 `UniversalForward`: URP Lit, `BaseMap*BaseColor` + MainLight shadow + SH ambient + fog. Tags `LightMode UniversalForward`.
*   Pass 2 `Outline`: `Cull Front`, vertex extrusion `pos + normal * _OutlineWidth01` gated by `_OutlineWidth > 0.5 ? width : 0`, outputs `_OutlineColor`. Controlled via `MaterialPropertyBlock` in `ShaderControls`.

Props: `_BaseMap`, `_BaseColor`, `_OutlineWidth` (toggle 0/1), `_OutlineWidth01` (thickness), `_OutlineColor`.

### 9.4 Materials

*   `Materials/Borderline Shader/{Match_Outline.mat, Outline_mat.mat, Outline.mat}` — URP Lit / custom outline variants.
*   `Materials/Grass/Grass.mat`
*   `Materials/Phys Material/Bouncy.physicMaterial` — low friction/bounce for settling.
*   `Materials/Sky/Sky_LowPoly_01_Day_a.mat`
*   `Models/Non Metallic.mat`

### 9.5 Textures & Models

*   Textures: `Textures/palette.png`, `Textures/Grass/Grass001_1K-JPG_{Color,NormalDX,NormalGL,Roughness,AmbientOcclusion,Displacement}.jpg` (high-res), `Default*.png` (UI), `Free Stylized Skybox/.../Sky_LowPoly_01_Day_a.png`, `Models/Sticks/Match Stick/Match{Albedo,Metal,Normal,Rough}.png`.
*   Models: `Models/{grass_blades_1.fbx, ground_cloud_4.fbx, ground_dirt_8.fbx, ground_grass_2.fbx, ground_wood_4.fbx}`, `Models/Sticks/Match Stick/Match.obj (+.mtl)` — hero stick low-poly cylinder with caps.

### 9.6 ScriptableObjects / Data

*   None custom — no `CreateAssetMenu` ScriptableObjects. All config is serialized fields. Gap for data-driven sticks (points, color, rarity) — `ObjectPoints.Points` must be set per prefab instance.
*   TMP: `TextMesh Pro/Resources/{TMP Settings.asset, LiberationSans SDF.asset, EmojiOne.asset, Default Style Sheet.asset}`.

### 9.7 URP Settings Detail

*   `Settings/PC_RPAsset.asset` + `PC_Renderer.asset` — 2048 shadow resolution, 50 shadow distance, 4 cascades, HDR, MSAA 1.
*   `Settings/Mobile_RPAsset.asset` + `Mobile_Renderer.asset` — 1024 shadow, RenderScale 0.8, no opaque texture, 1 cascade.
*   `Settings/UniversalRenderPipelineGlobalSettings.asset` — shader variant map.
*   `Settings/DefaultVolumeProfile.asset` + `SampleSceneProfile.asset` — bloom/tonemapping (26KB/3.7KB).

---

## 10. Project Settings (Key Files)

*   `ProjectSettings/ProjectVersion.txt` → `6000.5.4f1`
*   `ProjectSettings/ProjectSettings.asset` → `productName Mikado_V2`, `companyName DefaultCompany`, `productGUID 6898817f...`, `bundleVersion 0.1.0`, template `clonedFromGUID` still URP blank.
*   `ProjectSettings/GraphicsSettings.asset` → `m_CustomRenderPipeline → URP` (guid `18dc0cd2...`), TierSettings empty.
*   `ProjectSettings/QualitySettings.asset` → Mobile/PC, `m_CurrentQuality 1 (PC)`, per-platform `Android:0 Mobile, Standalone:1 PC`, `RenderScale Mobile 0.8 / PC 1`, pixelLightCount Mobile2 / PC4-ish, shadows etc.
*   `ProjectSettings/TagManager.asset` → `layers[6]=Stick`, sorting `Default`, tags `[]`.
*   `ProjectSettings/DynamicsManager.asset` → gravity `-9.81`, `DefaultContactOffset 0.01`, `LayerCollisionMatrix all true`.
*   `ProjectSettings/TimeManager.asset` → `Fixed Timestep 0.02` (50Hz), `MaximumAllowed 0.3333`, `TimeScale 1`.
*   `ProjectSettings/EditorBuildSettings.asset` → stale `Assets/Scenes/SampleScene.unity enabled:0` + `com.unity.input.settings.actions` config — see §9.1 blocker.
*   `ProjectSettings/EditorSettings.asset` → `EnterPlayModeOptionsEnabled 1`, `AsyncShaderCompilation 1`, `SerializationMode 2`.
*   `Packages/manifest.json` + `packages-lock.json` → see §2.

---

## 11. Known Issues, WIPs & Gotchas

| Area | Issue | Severity | Fix Sketch |
| :--- | :--- | :--- | :--- |
| **Build** | `EditorBuildSettings` missing both real scenes; stale `SampleScene` disabled. `UIManager.NextScene()` `LoadScene(buildIndex+1)` will fail. | **Blocker** | Add `MainMenu.unity` (0) + `Gameplay Scene.unity` (1) to Build Profiles; remove `SampleScene` entry. |
| **Config** | `bundleIdentifier` still `com.UnityTechnologies.com.unity.template.urpblank`, `companyName DefaultCompany` | Ship blocker | Set to `com.designonsteroids.mikado` / `DesignOnSteroids` etc. |
| **Scoring** | `ScoreSystem.OnCollision` never fired — `CollisionChecker` scoring block commented out (`TriggerCollision`). Win/lose canvases dead. `scoretoWin 25 / totalavailableScore 31` hardcoded. | Gameplay | Uncomment/wire `TriggerCollision` on `TakeThis`; or move win condition to `StickCheck` count of `SetActive(false)` sticks. |
| **Pickup** | `PickUpController.AddForce(forceDirection.position * magnitude, ForceMode.Force)` uses world position not direction; `Force` vs `Impulse` unclear. | Bug | `forceDirection.forward.normalized * magnitude` or `(pickUpObject.position - origin).normalized`; `ForceMode.Impulse` for instant. |
| **Physics/State** | `ObjectPoints` mixes physics state with game state (`isTarget` + `rb.useGravity` toggle). `StickCheck.position` dict + parallel arrays `children`/`childrenScripts` + `_lastSelectedIndex` fragility; `SetActive(false)` objects remain in lists (desync guard exists but fragile). `DetectStickMove` rewrites snapshot after detection → micro-nudges accumulate silently. | Design | Introduce `IStickView` interface, separate `StickState`; central `List<Stick>` single source; don't rewrite snapshot on illegal move; use `OverlapBox`/`Sleep` checks. |
| **Layers** | `Presentation` → `Gameplay` asmdef dependency (leak). `TouchManager` imports `Mikado.Gameplay` for `StickCheck`. | Arch | Move `StickCheck.GetStatus()` / selection contracts into `Core`, or interface. |
| **Input** | `clickAction` naming collision (TouchManager Press vs PickUpController PickUp). `isDragging`/`selectedCube`/`dragOffset` + `CheckHoverOnLayerMask` are dead/half-implemented drag-to-move WIP. | WIP | Rename fields (`selectAction`, `pickupAction`); remove dead drag code or finish drag feature. |
| **Timing** | `StickCheck.StartGame WaitForSeconds(gamestartTime)` affected by `Time.timeScale` (pause breaks it). `isposTake` gating via `GetStatus()` polls each mouse move. | Fragile | Use `WaitForSecondsRealtime`; event `OnReady` instead of poll. |
| **Memory** | `ShaderControls` commented `DamageGlow` used `renderer.material` (leaks instances) vs MPB path which is correct. | If re-enabled | Use MPB for all, or `MaterialPropertyBlock` + shared material. |
| **Docs** | Only `README.md` (2 lines) + DOTween readme. No `ARCHITECTURE.md` etc. | Gap | This file fills gap. |
| **Tests** | `test-framework@1.7.0` installed but zero EditMode/PlayMode tests. | Gap | Add at least `StickCheck` threshold test, `ObjectPoints` state test. |
| **Commented Code** | Evidence across files: `GameEventBus.ResetAllListeners`, `CollisionChecker` scoring, `CreateSticks` old `ArrangeInCircle`, `TouchManager` hover/drag, `UIManager` Button refs, `OutlineBase.shader` `shader_feature` — indicates iteration not cleaned. | Noise | Purge or `// TODO` tag commented blocks before sharing. |

No `TODO/FIXME` grep hits — WIPs are via commented code only.

---

## 12. How to Run & Controls

### 12.1 Open

1.  Unity Hub → Add → `Mikado_Online/` → Open with **Unity `6000.5.4f1`** (install via Hub if missing; IL2CPP, Android SDK 26+ if building for mobile).
2.  Allow burst/collections/mathematics import. DOTween may prompt to re-setup — allow.
3.  **First fix:** `File > Build Profiles` → add `Assets/Mikado_Progress/Scenes/MainMenu.unity` then `Assets/Mikado_Progress/Scenes/Gameplay Scene.unity` (order matters for `NextScene`). Remove/disable `Assets/Scenes/SampleScene.unity`.
4.  Open `Gameplay Scene.unity` → Press Play.

### 12.2 Play

*   Wait `gamestartTime` seconds for sticks to settle (log: "please wait until sticks are settle" → "Goo!").
*   **Select:** Left-click / tap on a stick (raycast against `Stick` layer) → outline appears, camera DOTweens to it.
*   **Camera:** `WASD` rotate/orbit+height (`Touch.Rotate`), `Q/E` zoom (`Touch.Zoom`).
*   **Pickup:** `Space` (Touch.PickUp) → applies force to selected stick (currently buggy direction).
*   **Illegal detection:** If non-selected stick moves `> moveThreshold` after pickup, console logs `Movement Detected! + distance`; stick is collected (`SetActive(false)`).
*   **Pause/Restart:** `UIManager.PauseButton` toggles `Time.timeScale`; `RestartButton` reloads scene (or shows "Un Pause Scene" if paused); `NextScene` loads next build index.
*   **Menu:** `MainMenu.unity` → Play/Quit buttons with `HoverMove` hover animations.

### 12.3 Scenes to Inspect

*   Hierarchy of `Gameplay Scene.unity`: look for `CreateSticks` parent (holds `CreateSticks`+`StickCheck`), `PickUp ForceDirection` transform (pickup direction), `UI Canvas` (score `TMP_Text`, win/lose `GameObject`s), `Grass`/`Ground`.

---

## 13. Extension Notes — Making it Online

Title is `Mikado_Online` but current code is **strictly local single-player**. To make it online:

1.  **Pick a stack:** For Unity 6, **Netcode for GameObjects (NGO)** + Relay/Lobby is idiomatic (FishNet alternative for physics-heavy).
2.  **Authoritative physics:** Move `Dictionary<Transform,Vector3>` snapshot and `DetectStickMove` to **server**; clients just send `TriggerTargetChange` as `ServerRpc`. Use `NetworkRigidbody` or server-only `AddForce` then sync transforms (NGO `NetworkTransform` or `NetworkRigidbody`).
3.  **Replace EventBus:** `GameEventBus` must become `NetworkEvents` with `ServerRpc`/`ClientRpc` — e.g., `RequestSelectionServerRpc(index)` → server validates → `ClientRpc` to highlight (MPB) + camera target for all.
4.  **State sync:** `ObjectPoints` becomes `NetworkBehaviour` with `NetworkVariable<sticksState>` / `isTarget`, `Points`, `nameStick`. Don't sync via `SetActive` — use `NetworkObject.Despawn`.
5.  **Input ownership:** Gate `TouchManager` raycast by `IsOwner` / `IsClient` checks; validate raycast on server to prevent cheat.
6.  **Turns:** Add turn-order state machine (not present) — currently any player could pick any time.
7.  **Determinism:** Unity 3D physics is non-deterministic cross-device — for competitive, either server-authoritative physics + client interpolation, or deterministic spawn (`ArrangeInCircle` with synced seed).
8.  **Build:** Fix `EditorBuildSettings` + add Netcode bootstrap scene.

---

## 14. File References (Relative Paths)

**Scripts (12):**
```
Assets/Mikado_Progress/Scripts/Core/GameEventBus.cs
Assets/Mikado_Progress/Scripts/Core/ObjectPoints.cs
Assets/Mikado_Progress/Scripts/Gameplay/CollisionChecker.cs
Assets/Mikado_Progress/Scripts/Gameplay/CreateSticks.cs
Assets/Mikado_Progress/Scripts/Gameplay/PickUpController.cs
Assets/Mikado_Progress/Scripts/Gameplay/ScoreSystem.cs
Assets/Mikado_Progress/Scripts/Gameplay/StickCheck.cs
Assets/Mikado_Progress/Scripts/Presentation/CameraRotation.cs
Assets/Mikado_Progress/Scripts/Presentation/HoverMove.cs
Assets/Mikado_Progress/Scripts/Presentation/ShaderControls.cs
Assets/Mikado_Progress/Scripts/Presentation/TouchManager.cs
Assets/Mikado_Progress/Scripts/Presentation/UIManager.cs
```
Input: `Assets/Mikado_Progress/Scripts/TouchControlls.inputactions` (203L JSON, map `Touch`)
Shader: `Assets/Shader/OutlineBase.shader` (181L, `Custom/Outline`, URP + outline pass)
Scenes: `Assets/Mikado_Progress/Scenes/Gameplay Scene.unity`, `Assets/Mikado_Progress/Scenes/MainMenu.unity`
Prefabs: `Assets/Mikado_Progress/Prefabs/Hero Object/Match.prefab`, `Match_Outline2.prefab`, `Grass.prefab`, `Grass Group.prefab`
Asmdefs: `Assets/Mikado_Progress/Scripts/Core/Mikado.Core.asmdef`, `Gameplay/Mikado.Gameplay.asmdef`, `Presentation/Mikado.Presentation.asmdef`, `Assets/Plugins/Demigiant/DOTween/Modules/DOTween.Modules.asmdef`
Materials: `Assets/Mikado_Progress/Materials/Borderline Shader/*.mat`, `Grass/Grass.mat`, `Phys Material/Bouncy.physicMaterial`, `Sky/Sky_LowPoly_01_Day_a.mat`
Models/Textures: `Assets/Mikado_Progress/Models/Sticks/Match Stick/Match.obj`, `Textures/palette.png`, `Textures/Grass/Grass001_1K-JPG_*.jpg`
Settings: `Assets/Settings/{PC,Mobile}_RPAsset.asset`, `{PC,Mobile}_Renderer.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `SampleSceneProfile.asset`, `DefaultVolumeProfile.asset`
ProjectSettings: `ProjectSettings/{ProjectVersion.txt, ProjectSettings.asset, GraphicsSettings.asset, QualitySettings.asset, EditorBuildSettings.asset, TagManager.asset, DynamicsManager.asset, TimeManager.asset, URPProjectSettings.asset, EditorSettings.asset}`
Packages: `Packages/manifest.json`, `Packages/packages-lock.json` — key deps: `2d.common@14.0.1, inputsystem@1.19.0, urp@17.5.0, ugui@2.5.0, test-framework@1.7.0`, transitive `burst@1.8.29, collections@6.5.0, mathematics@1.4.0`

**Packages/manifest.json snippet:**
```json
{
  "dependencies": {
    "com.unity.2d.common": "14.0.1",
    "com.unity.ide.visualstudio": "2.0.28",
    "com.unity.inputsystem": "1.19.0",
    "com.unity.render-pipelines.universal": "17.5.0",
    "com.unity.test-framework": "1.7.0",
    "com.unity.ugui": "2.5.0"
  }
}
```

---

## 15. Prompt Snippet — Paste to Any LLM

> Use this as a system prompt opener after attaching the file or pasting its sections.

```
You are an expert Unity 6 (6000.5.4f1) + URP 17.5 + Input System 1.19 developer working on Mikado_Online — a 3D Mikado (pick-up sticks) game.

Context: Local single-player prototype. Core stack: C# (~1010 LOC, 12 scripts), 3 asmdefs (Core -> Gameplay -> Presentation via static GameEventBus), URP forward + custom OutlineBase.shader (MPB _OutlineWidth), 3D physics layer Stick=6 with Bouncy physMat. Main scene Gameplay Scene.unity (not yet in Build Settings). Prefab Match.prefab with ObjectPoints (points/name/isTarget/isFlagged, state Active/Selected/Disable, gravity toggle, MPB outline).

Key systems:
- CreateSticks spawns N sticks in jittered circle → StickCheck snapshots Dictionary<Transform,Vector3> after gamestartTime → isposTake gates TouchManager.
- TouchManager raycasts Stick layer on click → GameEventBus.TriggerTargetChange → StickCheck.ChangeObjectState (parallel arrays + _lastSelectedIndex) → ObjectPoints.SetSelection → ShaderControls MPB + CameraRotation DOTween.
- PickUpController AddForce on Space (bug: uses position not direction).
- CollisionChecker trigger → TriggerMovementDetected → StickCheck.DetectStickMove (distance > moveThreshold) + OnStickCollected → SetActive(false). Scoring via ScoreSystem is dead (OnCollision never fired).

Gaps: No networking (would need NGO + NetworkRigidbody + server-authoritative snapshot), bundleId still template, Build Settings missing scenes, Presentation->Gameplay asmdef leak, drag code dead.

When answering, respect relative paths Assets/Mikado_Progress/Scripts/... and asmdef boundaries. Use line refs like CreateSticks.cs: arranging. Be concise, cite file:line when proposing edits. Don't hallucinate files — only those listed in §14 exist. Ask before adding Netcode packages.
```

---

*Generated for LLM handoff — covers architecture, flows, gotchas, and wiring so any LLM can reason about the codebase without extra exploration.*
