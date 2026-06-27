# Mikado_Online

## Scope

Only `Assets/New Game/` is active. Everything under `Assets/Mikado_Progress/` is legacy — ignore it.

## Ball game (`Assets/New Game/`)

- Ball controlled via `Rigidbody.AddTorque` on the player's `Rigidbody`
- Input via Unity Input System (`InputAction` + `performed` delegate)
- Torque mapping for XZ-plane rolling: `torque = (input.z, 0, -input.x)`

## Key scripts

- `PlayerController.cs` — ball input, torque application, gizmo debug (COM + movement direction arrow)
- `Respawner.cs` (in `Teste/`) — resets ball position on input; **must also zero** `Rigidbody.velocity` and `angularVelocity` on respawn

## Conventions

- `[SerializeField] private` for serialized fields; `public` for editor-tweakable values
- Input: `OnEnable`/`OnDisable` lifecycle for action enable/disable and callback attach/detach
- `OnDrawGizmos` split into `DrawCenterOfMass()` + `DrawMovementGizmo()`
- Movement direction gizmo: green line + sphere from ball center, offset by `gizmoOffset`
