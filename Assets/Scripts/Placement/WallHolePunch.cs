using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class WallHolePunch : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private LayerMask wallMask;            // Layers containing walls
    [SerializeField] private float maxDistance = 100f;     // Max distance to check walls
    
    [Header("Hole Punch Settings")]
    [SerializeField] private float holeRadius = 2.0f;       // Radius of each hole
    [SerializeField] private float holeFalloff = 1.0f;     // Smooth falloff distance
    [SerializeField] private int maxHoles = 5;              // Maximum number of holes
    
    [Header("Objects to Reveal")]
    [SerializeField] private Transform[] revealObjects;     // Objects that should be visible through walls
    [SerializeField] private string revealObjectTag = "Player"; // Tag to find reveal objects
    
    [Header("Shader Settings")]
    [SerializeField] private Shader holePunchShader;       // The WallHolePunch shader
    [SerializeField] private bool autoFindShader = true;    // Automatically find shader
    
    [Header("Enable/Disable")]
    [SerializeField] private bool holePunchEnabled = true;

    private Camera cam;
    private readonly List<Renderer> wallRenderers = new();
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
    private readonly Dictionary<Renderer, Material[]> holePunchMaterials = new();
    
    // Shader property IDs (cached for performance)
    private static readonly int HolePositionsID = Shader.PropertyToID("_HolePositions");
    private static readonly int HoleCountID = Shader.PropertyToID("_HoleCount");
    private static readonly int HoleRadiusID = Shader.PropertyToID("_HoleRadius");
    private static readonly int HoleFalloffID = Shader.PropertyToID("_HoleFalloff");
    private static readonly int MaxHolesID = Shader.PropertyToID("_MaxHoles");

    void Awake()
    {
        cam = Camera.main;
        
        // Find shader if not assigned
        if (holePunchShader == null && autoFindShader)
        {
            holePunchShader = Shader.Find("Custom/WallHolePunch");
            if (holePunchShader == null)
            {
                Debug.LogError("WallHolePunch: Could not find 'Custom/WallHolePunch' shader. Please assign it manually.");
            }
        }
        
        // Find all wall renderers
        FindWallRenderers();
        
        // Find reveal objects if not assigned
        if (revealObjects == null || revealObjects.Length == 0)
        {
            FindRevealObjects();
        }
    }

    void FindWallRenderers()
    {
        wallRenderers.Clear();
        
#if UNITY_2023_1_OR_NEWER
        var rends = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        var rends = Object.FindObjectsOfType<Renderer>();
#endif
        
        foreach (var r in rends)
        {
            if (((1 << r.gameObject.layer) & wallMask.value) != 0)
            {
                wallRenderers.Add(r);
            }
        }
        
        Debug.Log($"WallHolePunch: Found {wallRenderers.Count} wall renderers.");
    }

    void FindRevealObjects()
    {
        List<Transform> found = new List<Transform>();
        
        // Find by tag
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(revealObjectTag);
        foreach (var obj in tagged)
        {
            found.Add(obj.transform);
        }
        
        revealObjects = found.ToArray();
        Debug.Log($"WallHolePunch: Found {revealObjects.Length} reveal objects by tag '{revealObjectTag}'.");
    }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;
        
        if (!holePunchEnabled || holePunchShader == null)
        {
            RestoreAllWalls();
            return;
        }
        
        // Get active reveal object positions
        List<Vector4> holePositions = new List<Vector4>();
        foreach (var obj in revealObjects)
        {
            if (obj != null && obj.gameObject.activeInHierarchy)
            {
                holePositions.Add(new Vector4(obj.position.x, obj.position.y, obj.position.z, 1));
            }
        }
        
        // Limit to max holes
        if (holePositions.Count > maxHoles)
        {
            holePositions = holePositions.GetRange(0, maxHoles);
        }
        
        // Update all wall materials
        UpdateWallMaterials(holePositions);
    }

    void UpdateWallMaterials(List<Vector4> holePositions)
    {
        // Prepare hole positions array (pad to fixed size for shader)
        Vector4[] holeArray = new Vector4[10]; // Max 10 holes
        for (int i = 0; i < holeArray.Length; i++)
        {
            if (i < holePositions.Count)
            {
                holeArray[i] = holePositions[i];
            }
            else
            {
                holeArray[i] = Vector4.zero;
            }
        }
        
        foreach (var renderer in wallRenderers)
        {
            if (renderer == null || !renderer.enabled) continue;
            
            // Check if wall is within range
            float dist = Vector3.Distance(cam.transform.position, renderer.bounds.center);
            if (dist > maxDistance) continue;
            
            // Ensure we have hole punch materials for this renderer
            if (!holePunchMaterials.ContainsKey(renderer))
            {
                CreateHolePunchMaterials(renderer);
            }
            
            // Update shader properties
            Material[] mats = holePunchMaterials[renderer];
            foreach (var mat in mats)
            {
                if (mat != null)
                {
                    mat.SetVectorArray(HolePositionsID, holeArray);
                    mat.SetFloat(HoleCountID, holePositions.Count);
                    mat.SetFloat(HoleRadiusID, holeRadius);
                    mat.SetFloat(HoleFalloffID, holeFalloff);
                    mat.SetFloat(MaxHolesID, maxHoles);
                }
            }
            
            // Apply hole punch materials
            renderer.sharedMaterials = mats;
        }
    }

    void CreateHolePunchMaterials(Renderer renderer)
    {
        // Store original materials
        if (!originalMaterials.ContainsKey(renderer))
        {
            Material[] originals = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                originals[i] = renderer.sharedMaterials[i];
            }
            originalMaterials[renderer] = originals;
        }
        
        // Create hole punch materials
        Material[] originals = originalMaterials[renderer];
        Material[] holeMats = new Material[originals.Length];
        
        for (int i = 0; i < originals.Length; i++)
        {
            Material original = originals[i];
            Material holeMat = new Material(holePunchShader);
            
            // Copy properties from original material if possible
            if (original != null)
            {
                // Try to copy common properties
                if (original.HasProperty("_BaseMap") && holeMat.HasProperty("_BaseMap"))
                    holeMat.SetTexture("_BaseMap", original.GetTexture("_BaseMap"));
                else if (original.HasProperty("_MainTex") && holeMat.HasProperty("_BaseMap"))
                    holeMat.SetTexture("_BaseMap", original.GetTexture("_MainTex"));
                
                if (original.HasProperty("_BaseColor") && holeMat.HasProperty("_BaseColor"))
                    holeMat.SetColor("_BaseColor", original.GetColor("_BaseColor"));
                else if (original.HasProperty("_Color") && holeMat.HasProperty("_BaseColor"))
                    holeMat.SetColor("_BaseColor", original.GetColor("_Color"));
                
                if (original.HasProperty("_Metallic") && holeMat.HasProperty("_Metallic"))
                    holeMat.SetFloat("_Metallic", original.GetFloat("_Metallic"));
                
                if (original.HasProperty("_Smoothness") && holeMat.HasProperty("_Smoothness"))
                    holeMat.SetFloat("_Smoothness", original.GetFloat("_Smoothness"));
            }
            
            holeMats[i] = holeMat;
        }
        
        holePunchMaterials[renderer] = holeMats;
    }

    void RestoreAllWalls()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sharedMaterials = kvp.Value;
            }
        }
    }

    void OnDestroy()
    {
        RestoreAllWalls();
        
        // Clean up created materials
        foreach (var mats in holePunchMaterials.Values)
        {
            foreach (var mat in mats)
            {
                if (mat != null)
                {
                    DestroyImmediate(mat);
                }
            }
        }
        
        holePunchMaterials.Clear();
        originalMaterials.Clear();
    }

    void OnDisable()
    {
        RestoreAllWalls();
    }

    // ----------------- Public API -----------------
    public bool HolePunchEnabled => holePunchEnabled;

    public void SetHolePunch(bool enabled)
    {
        if (holePunchEnabled == enabled) return;
        holePunchEnabled = enabled;
        if (!holePunchEnabled) RestoreAllWalls();
    }

    public void ToggleHolePunch() => SetHolePunch(!holePunchEnabled);
    public void EnableHolePunch() => SetHolePunch(true);
    public void DisableHolePunch() => SetHolePunch(false);

    public void SetRevealObjects(Transform[] objects)
    {
        revealObjects = objects;
    }

    public void AddRevealObject(Transform obj)
    {
        List<Transform> list = new List<Transform>(revealObjects ?? new Transform[0]);
        if (!list.Contains(obj))
        {
            list.Add(obj);
            revealObjects = list.ToArray();
        }
    }

    public void RemoveRevealObject(Transform obj)
    {
        List<Transform> list = new List<Transform>(revealObjects ?? new Transform[0]);
        if (list.Remove(obj))
        {
            revealObjects = list.ToArray();
        }
    }

    public void RefreshWallRenderers()
    {
        FindWallRenderers();
    }
}

