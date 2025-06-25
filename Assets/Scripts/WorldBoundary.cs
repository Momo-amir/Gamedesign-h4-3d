using UnityEngine;

[ExecuteAlways]
public class WorldBoundary : MonoBehaviour
{
    [Header("Road Reference")]
    [Tooltip("The root GameObject that holds your road meshes/colliders. If null, uses this object's parent.")]
    public Transform roadParent;

    [Header("Wall Settings")]
    public float    wallHeight    = 5f;
    public float    wallThickness = 1f;
    public Material wallMaterial;

    void Start()
    {
        BuildWalls();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        // allow live‐update in Editor
        if (!Application.isPlaying)
            BuildWalls();
    }
    #endif

    void BuildWalls()
    {
        // clear any existing walls
        var existing = new System.Collections.Generic.List<Transform>();
        foreach (Transform c in transform)
            existing.Add(c);
        foreach (var c in existing)
            DestroyImmediate(c.gameObject);

        // pick road parent
        var rp = roadParent != null ? roadParent : transform.parent;
        if (rp == null)
        {
            Debug.LogWarning("WorldBoundary: no roadParent or parent to sample bounds from.");
            return;
        }

        // gather all Renderers (fallback to Colliders if none)
        var rends = rp.GetComponentsInChildren<Renderer>();
        Bounds b;
        if (rends.Length > 0)
        {
            b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
        }
        else
        {
            var cols = rp.GetComponentsInChildren<Collider>();
            if (cols.Length == 0)
            {
                Debug.LogWarning("WorldBoundary: no Renderer or Collider found under roadParent.");
                return;
            }
            b = cols[0].bounds;
            foreach (var c in cols) b.Encapsulate(c.bounds);
        }

        float minX = b.min.x, maxX = b.max.x;
        float minZ = b.min.z, maxZ = b.max.z;
        Vector3 center = b.center;

        // Build 4 walls
        CreateWall("Wall_North",
            // x, y, z
            new Vector3(center.x,
                        wallHeight * 0.5f,
                        maxZ + wallThickness * 0.5f),
            // x, y, z
            new Vector3((maxX - minX) + wallThickness * 2f,
                        wallHeight,
                        wallThickness));

        CreateWall("Wall_South",
            new Vector3(center.x,
                        wallHeight * 0.5f,
                        minZ - wallThickness * 0.5f),
            new Vector3((maxX - minX) + wallThickness * 2f,
                        wallHeight,
                        wallThickness));

        CreateWall("Wall_East",
            new Vector3(maxX + wallThickness * 0.5f,
                        wallHeight * 0.5f,
                        center.z),
            new Vector3(wallThickness,
                        wallHeight,
                        (maxZ - minZ) + wallThickness * 2f));

        CreateWall("Wall_West",
            new Vector3(minX - wallThickness * 0.5f,
                        wallHeight * 0.5f,
                        center.z),
            new Vector3(wallThickness,
                        wallHeight,
                        (maxZ - minZ) + wallThickness * 2f));
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(transform, worldPositionStays: true);
        wall.transform.position = pos;
        wall.transform.localScale = scale;

        if (wallMaterial != null)
            wall.GetComponent<Renderer>().sharedMaterial = wallMaterial;

        wall.GetComponent<Collider>().isTrigger = false;
    }
}
