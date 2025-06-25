# My project (2)

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
