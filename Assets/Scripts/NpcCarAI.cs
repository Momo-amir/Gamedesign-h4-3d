using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCCarAI : MonoBehaviour
{
    public float moveAccel             = 25f;
    public float turnSpeed             = 100f;
    public float detectionRadius       = 10f;
    public float avoidanceStrength     = 2f;
    public float wanderInterval        = 3f;
    public float obstacleDetectDist    = 30f;

    Rigidbody rb;
    Transform player;
    Vector3 wanderDir;
    float   wanderTimer;

    void Awake()
    {
        rb     = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb.useGravity = true;

        PickWanderDir();
    }

    void FixedUpdate()
    {
        // 0) obstacle avoidance
        if (Physics.Raycast(transform.position, transform.forward,
                            out var hit, obstacleDetectDist)
            && hit.transform != player)
        {
            wanderDir = Vector3.Reflect(wanderDir, hit.normal);
            wanderDir.y = 0;
            wanderDir.Normalize();
            wanderTimer = wanderInterval;
            RotateTowards(wanderDir, turnSpeed * 2f);
            return;
        }

        // 1) decide idle vs evade
        Vector3 toP  = player ? player.position - transform.position : Vector3.zero;
        float   dist = player ? toP.magnitude : float.MaxValue;
        if (dist < detectionRadius)
            Evade(toP);
        else
            Idle();
    }

    void Evade(Vector3 toPlayer)
    {
        // steer away + wander
        Vector3 away = -toPlayer.normalized * avoidanceStrength * 2f;
        Vector3 dir  = (wanderDir + away).normalized;

        // drive
        rb.AddForce(transform.forward * moveAccel, ForceMode.Acceleration);
        RotateTowards(dir, turnSpeed * 2f);

        TimerTick();
    }

    void Idle()
    {
        // stop lateral XZ vel, keep Y
        var v = rb.linearVelocity; rb.linearVelocity = new Vector3(0, v.y, 0);
        // spin in place
        RotateTowards(transform.forward, turnSpeed * 0.2f);
    }

    void RotateTowards(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 0.01f) return;
        var target = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(
                            rb.rotation, target, speed * Time.fixedDeltaTime));
    }

    void TimerTick()
    {
        wanderTimer -= Time.fixedDeltaTime;
        if (wanderTimer <= 0f) PickWanderDir();
    }

    void PickWanderDir()
    {
        wanderDir = Random.insideUnitSphere;
        wanderDir.y = 0;
        wanderDir.Normalize();
        wanderTimer = wanderInterval;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform == player)
        {
            GameManager.Instance.OnNPCHit(this);
            return;
        }

        if (col.transform != player)
        {
            var n = col.contacts[0].normal;
            wanderDir = Vector3.Reflect(wanderDir, n).normalized;
            wanderTimer = wanderInterval;
        }
    }
}

// Note: This script assumes the player has a tag "Player".