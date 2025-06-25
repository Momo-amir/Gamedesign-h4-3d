using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCCarAI : MonoBehaviour
{
    enum State { Roam, Evade, AvoidObstacle, Idle }
    
    [Header("AI Settings")]
    public float moveAccel          = 25f;
    public float wanderAccel        = 10f;    // acceleration when wandering
    public float turnSpeed          = 100f;
    public float detectionRadius    = 10f;
    public float avoidanceStrength  = 2f;
    public float wanderInterval     = 3f;
    public float obstacleDetectDist = 3f;
    // how much random angle to apply on wall reflection
    public float obstacleJitterAngle = 30f;
    bool hasBeenHit = false;


    Rigidbody rb;
    Transform player;
    Vector3 wanderDir;
    float   wanderTimer;
    State   currentState;

    void Awake()
    {
        rb     = GetComponent<Rigidbody>();
        player = GameObject.FindWithTag("Player")?.transform;
        rb.useGravity = true;
        SetState(State.Roam);
    }

    void FixedUpdate()
    {
        // 1) obstacle override only in Roam/Idle
        if (IsObstacleAhead() && currentState != State.Evade)
            SetState(State.AvoidObstacle);
        else if (player && Vector3.Distance(transform.position, player.position) < detectionRadius)
            SetState(State.Evade);
        else if (currentState == State.Evade)
            SetState(State.Roam);

        switch (currentState)
        {
            case State.Roam:         UpdateRoam();         break;
            case State.Evade:        UpdateEvade();        break;
            case State.AvoidObstacle:UpdateAvoidObstacle();break;
            case State.Idle:         UpdateIdle();         break;
        }
    }

    void SetState(State newState)
    {
        if (newState == currentState) return;
        currentState = newState;
        PickWanderDirection();
    }

    // choose a random horizontal direction that isn't blocked immediately
    void PickWanderDirection()
    {
        const int maxTries = 8;
        Vector3 dir = Vector3.zero;
        for (int i = 0; i < maxTries; i++)
        {
            dir = Random.onUnitSphere;
            dir.y = 0f;
            dir.Normalize();
            // if this direction doesn't immediately hit a wall, use it - not really a "wander" if it does
            // but we don't want to get stuck in a corner
            
            if (!Physics.Raycast(transform.position, dir, obstacleDetectDist))
                break;
        }
        wanderDir = dir;
        wanderTimer = wanderInterval;
    }

    bool IsObstacleAhead()
    {
        return Physics.Raycast(transform.position, transform.forward,
                               out var hit, obstacleDetectDist)
               && hit.transform != player;
    }

    void UpdateRoam()
    {
        // roam in the wanderDir at a slower, constant wander speed
        rb.AddForce(wanderDir * wanderAccel, ForceMode.Acceleration);
        TurnToward(wanderDir, turnSpeed * 0.5f);
        TickWander();
    }

    void UpdateEvade()
    {
        // compute pure away direction from player
        Vector3 awayDir = (transform.position - player.position).normalized;
        
        // if obstacle ahead, blend in a gentle reflection
        if (Physics.Raycast(transform.position, transform.forward, out var hit, obstacleDetectDist)
            && hit.transform != player)
        {
            Vector3 reflect = Vector3.Reflect(awayDir, hit.normal).normalized;
            // blend 50% toward reflection to steer around
            awayDir = Vector3.Slerp(awayDir, reflect, 0.5f).normalized;
        }
        
        // always drive forward
        DriveForward();
        // steer toward blended awayDir
        TurnToward(awayDir, turnSpeed * avoidanceStrength);
    }

    void UpdateAvoidObstacle()
    {
        // reflect off the wall normal
        var hit = Physics.RaycastAll(transform.position,
                                     transform.forward, obstacleDetectDist)[0];
        Vector3 refl = Vector3.Reflect(wanderDir, hit.normal).normalized;
        float jitterAv = Random.Range(-obstacleJitterAngle, obstacleJitterAngle);
        refl = Quaternion.Euler(0f, jitterAv, 0f) * refl;
        DriveForward();
        TurnToward(refl, turnSpeed * 2f);
    }

    void UpdateIdle()
    {
        // optional: spin in place or stand still
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        TurnToward(transform.forward, turnSpeed * 0.1f);
    }

    void DriveForward()
    {
        rb.AddForce(transform.forward * moveAccel,
                    ForceMode.Acceleration);
    }

    void TurnToward(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 0.01f) return;
        var target = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation, target, speed * Time.fixedDeltaTime));
    }

    void TickWander()
    {
        wanderTimer -= Time.fixedDeltaTime;
        if (wanderTimer <= 0f)
            SetState(player && Vector3.Distance(transform.position, player.position) < detectionRadius
                     ? State.Evade
                     : State.Roam);
    }

    void OnCollisionEnter(Collision col)
    {
        // only react to the player and only once
         if (hasBeenHit) return;

        if (col.gameObject.CompareTag("Player"))
        {
            hasBeenHit = true;
            GameManager.Instance.OnNPCHit(this);
        }
    }
}