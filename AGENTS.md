# Mikado_Online

## Scope

Only `Assets/New Game/` is active; `Assets/Mikado_Progress/` is legacy — ignore it.
Unity 6000.3.8f1, URP 17.3.0, Input System 1.18.0, Cinemachine 3.1.7, Animation Rigging 1.4.1.
No CI, no tests, no lint/format scripts.

## Ball game (`Assets/New Game/`)

- Ball controlled via `Rigidbody.AddTorque` on ball's `Rigidbody`
- Input via `InputAction` + `performed` delegate (XZ plane); `OnCancelled` clears input to prevent idle torque persistence
- Torque mapping: `(input.z, 0, -input.x)`
- Two scenes: `Test.unity` (simple test scene), `New Game.unity` (main gameplay scene)
- **Scene physics checks**: ensure ground has no unintended tilt; ensure ball/ground have non-zero friction to prevent perpetual rolling

## Key scripts

| File | Role |
|---|---|
| `Scripts/PlayerController.cs` | Ball input, torque, gizmo (COM + direction arrow), alien follow |
| `Scripts/FollowCamera.cs` | SmoothDamp follow on LateUpdate (uses `1f / positionDamping` as smooth time) |
| `Scripts/AlienLegController.cs` | Procedural foot IK animation for alien legs (sine wave step cycle) |
| `Scripts/BalanceController.cs` | Balance meter: drains on acceleration, triggers stagger → ragdoll → recovery |
| `Scripts/Teste/Respawner.cs` | Resets ball on input; zeros `linearVelocity` + `angularVelocity` (Unity 6 API) |

## Conventions

- `[SerializeField] private` for serialized fields; `public` for editor-tweakable values
- Input: `OnEnable`/`OnDisable` lifecycle for action enable/disable and callback attach/detach
- `DrawMovementGizmo()`: green line + sphere from COM, offset by `gizmoOffset`
- Alien character uses Animation Rigging (IK chain in `AIContext/context.md`)
- Respawner disables/re-enables the object to reset state
