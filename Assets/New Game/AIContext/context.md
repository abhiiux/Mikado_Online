You are helping develop a 3D physics-based game where the player controls a character standing on top of a rolling ball.

Core vision:
- The ball is the primary physics object and uses a Rigidbody.
- The player can move, balance, jump, and intentionally or accidentally fall off the ball.
- Falling off the ball is a core gameplay mechanic, not a failure state to avoid entirely.
- The game should produce emergent and funny moments through physics interactions.

Architecture goals:
- Use a hybrid approach rather than a fully ragdoll-driven character.
- The ball should always be simulated with real physics (forces, torque, collisions, slopes, momentum).
- During normal gameplay, the character is animation-driven with IK-based foot placement and balance assistance.
- The character should react to the ball's velocity, rotation, and tilt.
- When balance is lost, strong impacts occur, or special gameplay events happen, the character transitions into a physical ragdoll state and can genuinely fall from the ball.
- Recovery from a fall should be possible through get-up animations or gameplay systems.

Technical constraints:
- Engine: Unity.
- Language: C#.
- Prefer maintainable, modular, component-based architecture.
- Use SOLID principles where appropriate.
- Favor deterministic gameplay over chaotic full-body physics unless the chaos is intentional.
- Avoid overengineering; solutions should be realistic for a small indie team.

Suggested high-level hierarchy:

PlayerBallCharacter
├── Ball (Rigidbody)
│   ├── SphereCollider
│   └── BallMotor
│
├── CharacterRoot
│   ├── Animator
│   ├── PlayerStateMachine
│   ├── BalanceController
│   ├── FootIKController
│   └── RagdollController
│
├── CameraTarget
└── InteractionSystems

Core gameplay pillars:
- Balance and movement.
- Physics-driven puzzles and traversal.
- Emergent failures and recoveries.
- Environmental interactions.
- Precise controls with believable physical behavior.

When proposing systems or code:
- Prefer hybrid animation + physics solutions.
- Treat falling as an intentional mechanic.
- Separate responsibilities into focused components.
- Explain tradeoffs before implementation.
- Prioritize fun, readability, and extensibility.