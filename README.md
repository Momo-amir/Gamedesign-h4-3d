# My project (2)

If scene missing or not loaded, please try and find the scene named Prototype in the project scene folder.

## NPC AI State Machine

The NPC driving logic is implemented as a simple finite‐state machine (FSM) using an enum and a `SetState(...)` method:

1. **States**

   - `Roam` – wander in a random direction, re‐choosing every `wanderInterval`. when wandering is done, switch to `Idle`.
   - `Idle` – player is far and no obstacle, stop moving and slowly spin in place.
   - `Evade` – player is within `detectionRadius`, steer and drive away.
   - `AvoidObstacle` – a short forward raycast detects a wall or obstacle, reflect direction. This does not work very well lol

2. **Transitions**

   - **Global overrides** (checked first each frame):
     - Obstacle ahead → `AvoidObstacle`
     - Player too close → `Evade`
   - **Evade exit**: when the player moves out of range → back to `Roam`.
   - **Roam ↔ Idle**: when the wander timer expires, switch between wandering and idling.

3. **Execution Loop**
   - In `FixedUpdate()` first evaluate transitions and call `SetState(newState)` if needed.
   - Then `switch(currentState)` to call the corresponding `UpdateRoam()`, `UpdateEvade()`, etc.
   - Each `UpdateXxx()` method contains only the behavior for that state (driving, turning, timing).

### Enhancements

- **Smooth Evade Steering**: Evade now drives purely away from the player and blends in obstacle reflections (via `Vector3.Slerp`) for curved, natural turns instead of reversing or jittering.
- **Spawn Orientation**: New NPCs are rotated on spawn to face away from the player (with a small random yaw offset) so they don’t immediately drive into walls.
- **Obstacle Jitter**: Wall‐avoidance reflections include a random jitter angle (`obstacleJitterAngle`) to prevent repetitive bounces off the same spot.

## Goals reached and Things to improve

- **Goals Reached**:
  - Basic NPC AI with roaming, idling, evading, and obstacle avoidance.
  - Smooth steering and driving behavior.
  - Randomized spawn orientation to avoid immediate collisions.
  - Simple timer and point system
- **Things to Improve**:
  - **Obstacle Avoidance**: The current wall reflection logic is not very effective.
  - **Game Over**: Implement a proper game over state when the timer runs out.
  - **Variety in NPC visuals and behaviors**: Add more NPC types with different driving styles.
