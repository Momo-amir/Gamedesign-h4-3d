using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCCarAI : MonoBehaviour
{
    enum State { Roam, Evade, AvoidObstacle, Idle }
    
    [Header("AI Settings")]
    public float moveAccel          = 25f;
    public float turnSpeed          = 100f;
    public float detectionRadius    = 10f;
    public float avoidanceStrength  = 2f;
    public float wanderInterval     = 3f;
    public float obstacleDetectDist = 3f;
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
        // 1) global transitions have priority
        if (IsObstacleAhead())
            SetState(State.AvoidObstacle);
        else if (player && Vector3.Distance(transform.position, player.position) < detectionRadius)
            SetState(State.Evade);
        else if (currentState == State.Evade)
            SetState(State.Roam);

        // 2) execute current state logic
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
        wanderTimer  = wanderInterval;       // reset wander in any state
        wanderDir    = Random.onUnitSphere;  wanderDir.y = 0; wanderDir.Normalize();
    }

    bool IsObstacleAhead()
    {
        return Physics.Raycast(transform.position, transform.forward,
                               out var hit, obstacleDetectDist)
               && hit.transform != player;
    }

    void UpdateRoam()
    {
        DriveForward();
        TurnToward(wanderDir, turnSpeed * 0.5f);
        TickWander();
    }

    void UpdateEvade()
    {
        Vector3 away = (transform.position - player.position).normalized;
        DriveForward();
        TurnToward( (wanderDir + away*avoidanceStrength).normalized,
                    turnSpeed );
        TickWander();
    }

    void UpdateAvoidObstacle()
    {
        // reflect off the wall normal
        var hit = Physics.RaycastAll(transform.position,
                                     transform.forward, obstacleDetectDist)[0];
        Vector3 refl = Vector3.Reflect(wanderDir, hit.normal).normalized;
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