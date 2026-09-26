# Mikado Online — 3D pick-up-sticks prototype (Unity)

Physics-based Mikado adaptation — sticks settle, you select one, lift it without disturbing the rest. Local single-player working prototype; online play is the future direction.

[Play in browser](https://play.unity.com/api/v1/games/game/83b9e6b1-d33b-41dc-8a3e-610ec8c7c381/build/latest/frame)

<!-- HERO PLACEHOLDER: add screenshot/GIF manually via github.com -->
<!-- Suggested: settled pile + one outlined selected stick. Path: docs/hero.png -->

## What this shows

- Event-driven game flow: select → highlight → camera focus → force pickup via a central event bus across assembly-defined layers
- Illegal-move detection: post-settle position snapshot with debounced per-stick drift checks during pickup attempts
- Rendering craft: outline highlight via MaterialPropertyBlock (no material instancing), URP, DOTween camera transitions, hover-responsive UI
- Data-driven tuning: stick counts, settings, and win-panel state in ScriptableObjects

## Controls

| Action | Input |
| :--- | :--- |
| Select stick | Click / tap |
| Orbit + height | WASD |
| Zoom | Q / E |
| Pick up | Space |

## Stack

Unity 6000.5.4f1 · URP 17.5 · Input System 1.19 · DOTween · C#

## Run locally

1. Unity Hub → Add → open this folder with Unity 6000.5.4f1
2. Open `Assets/Mikado_Progress/Scenes/Gameplay Scene.unity`
3. Press Play — wait for settle, then click a stick

## Status

Playable local prototype in active development.
