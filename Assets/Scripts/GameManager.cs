using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("NPC Settings")]
    public GameObject       npcPrefab;            // assign your NPC prefab
    public Transform        spawnParent;          // optional parent for cleanliness

    [Header("Road Surface (must have Collider)")]
    public Collider  roadCollider;
    public LayerMask roadLayer;

    [Header("UI")]
    public TextMeshProUGUI hitCounterText; 
    public TextMeshProUGUI timerText;          



    [Header("NPC Stats…")] 
    public float baseSpeed, speedIncrement;
    public float baseAvoidance, avoidanceIncrement;
    public float baseDetectRadius, detectIncrement;
    public float spawnRadius = 20f;

    [Header("Game Timer")]
    public float totalTime    = 30f;   // start on 30 seconds
    public float hitTimeBonus = 2f;    // add 2 seconds per hit

    int hitCount = 0;

    float timeRemaining;
    bool  isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // initialize with new totalTime
        timeRemaining = totalTime;

                UpdateHitUI();
        UpdateTimerUI();

        SpawnNext();  
    }

    void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isGameOver   = true;
            OnGameOver();
        }
        UpdateTimerUI();
    }

    Vector3 GetRandomPointOnRoad()
    {
        Bounds b = roadCollider.bounds;
        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);
            Vector3 top = new Vector3(x, b.max.y + 5f, z);
            if (Physics.Raycast(top, Vector3.down, out var hit, 10f, roadLayer))
                return hit.point;
        }
        return b.center; // fallback
    }

    public void OnNPCHit(NPCCarAI npc)
    {
        if (isGameOver) return;

        hitCount++;
        UpdateHitUI();

        // add bonus time on each hit
        timeRemaining += hitTimeBonus;
        UpdateTimerUI();

        Destroy(npc.gameObject);
        SpawnNext();
    }

        void UpdateHitUI()
    {
        if (hitCounterText != null)
            hitCounterText.text = "Score: " + hitCount;
    }
    void SpawnNext()
    {
        Vector3 pos = GetRandomPointOnRoad();
        var parent = spawnParent != null ? spawnParent : transform;
        var go     = Instantiate(npcPrefab, pos, Quaternion.identity, parent);
        var ai     = go.GetComponent<NPCCarAI>();

        // bump up stats…
        ai.moveAccel         = baseSpeed        + speedIncrement     * hitCount;
        ai.avoidanceStrength = baseAvoidance    + avoidanceIncrement * hitCount;
        ai.detectionRadius   = baseDetectRadius + detectIncrement    * hitCount;
    
        // Orient NPC to face away from the player, with random yaw jitter
        var playerTf = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTf != null)
        {
            Vector3 awayDir = (go.transform.position - playerTf.position).normalized;
            // random yaw offset to vary initial direction
            float yawJ = Random.Range(-30f, 30f);
            awayDir = Quaternion.Euler(0f, yawJ, 0f) * awayDir;
            go.transform.rotation = Quaternion.LookRotation(awayDir, Vector3.up);
        }
    }

    void OnGameOver()
    {
        Debug.Log("Game Over! Final Score: " + hitCount);

        // STOP the game logic

    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining);
    
    }
}
