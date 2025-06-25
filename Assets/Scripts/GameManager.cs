using UnityEngine;
using TMPro;
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


    [Header("NPC Stats…")] 
    public float baseSpeed, speedIncrement;
    public float baseAvoidance, avoidanceIncrement;
    public float baseDetectRadius, detectIncrement;
    public float spawnRadius = 20f;

    int hitCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
                UpdateHitUI();

        SpawnNext();  
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
        hitCount++;
        UpdateHitUI();        Destroy(npc.gameObject);
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
    }
}
