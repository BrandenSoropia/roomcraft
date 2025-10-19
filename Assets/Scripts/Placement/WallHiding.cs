using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class WallAutoHider_ScreenTriangle : MonoBehaviour
{
    [Header("What to hide")]
    [SerializeField] private LayerMask wallMask;            // put your Walls layer(s) here
    [SerializeField] private float maxDistance = 100f;      // skip very far objects
    [SerializeField] private float screenEdgeMargin = 0.002f; // small tolerance in viewport space

    [Header("Triangle region (Viewport 0..1)")]
    [Tooltip("Bottom screen triangle A(0,0), B(0.5,0.5), C(1,0) by default")]
    [SerializeField] private Vector2 A = new Vector2(0f, 0f);
    [SerializeField] private Vector2 B = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 C = new Vector2(1f, 0f);

    [Header("Hide mode")]
    [Tooltip("Assign a transparent material to swap while hidden (recommended). Leave null for in-place alpha fade.")]
    [SerializeField] private Material fadeMaterial;
    [Range(0f, 1f)] [SerializeField] private float fadeAlpha = 0.2f;
    [SerializeField] private float fadeLerpSpeed = 20f;
    [SerializeField] private bool useShadowsOnly = false;

    [Header("Enable/Disable")]
    [SerializeField] private bool hidingEnabled = true;

    Camera cam;

    // Cache all wall renderers we might hide (optional; you can also populate this at runtime)
    readonly List<Renderer> wallRenderers = new();

    class HiddenState
    {
        public Renderer rend;
        public Material[] originalMats;
        public ShadowCastingMode originalShadowMode;

        // In-place alpha
        public Material[] instancedMats;
        public Color[] originalColors;
        public float currentAlpha = 1f;
    }

    readonly Dictionary<Renderer, HiddenState> activeHidden = new();
    readonly HashSet<Renderer> thisFrameHits = new();

    void Awake()
    {
        cam = Camera.main;

        // find all Renderers on wall layers
#if UNITY_2023_1_OR_NEWER
        var rends = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        var rends = Object.FindObjectsOfType<Renderer>();
#endif
        foreach (var r in rends)
        {
            if (((1 << r.gameObject.layer) & wallMask.value) != 0)
                wallRenderers.Add(r);
        }
    }

    void OnDestroy()
    {
        // restore anything left hidden
        RestoreAllHidden();
    }

    void OnDisable()
    {
        // ensure scene is clean when component is disabled
        RestoreAllHidden();
    }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // Early out when hiding disabled; also ensure anything previously hidden is restored
        if (!hidingEnabled)
        {
            if (activeHidden.Count > 0) RestoreAllHidden();
            return;
        }

        // Precompute camera frustum for quick reject
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        thisFrameHits.Clear();

        for (int i = 0; i < wallRenderers.Count; i++)
        {
            var r = wallRenderers[i];
            if (!r || !r.enabled) continue;
            var bounds = r.bounds;

            // quick reject: behind camera or too far
            if (Vector3.Distance(cam.transform.position, bounds.center) > maxDistance) continue;
            if (!GeometryUtility.TestPlanesAABB(planes, bounds)) continue;

            // Project the 8 bounds corners to viewport space
            Vector3[] corners = GetBoundsCorners(bounds);
            bool anyInFront = false;
            Vector2[] screen = new Vector2[corners.Length];
            for (int k = 0; k < corners.Length; k++)
            {
                var sp = cam.WorldToViewportPoint(corners[k]);
                if (sp.z > 0f) anyInFront = true;
                screen[k] = new Vector2(sp.x, sp.y);
            }
            if (!anyInFront) continue; // all behind

            // If ANY projected corner is inside the triangle → mark
            bool inside = false;
            for (int k = 0; k < screen.Length && !inside; k++)
                inside = PointInTriangle(screen[k], A, B, C, screenEdgeMargin);

            // Robustness: also test if triangle’s vertices are inside renderer’s screen AABB
            if (!inside)
            {
                var aabb = ScreenAABB(screen);
                if (PointInAABB(A, aabb) || PointInAABB(B, aabb) || PointInAABB(C, aabb))
                    inside = true;
            }

            if (inside) thisFrameHits.Add(r);
        }

        // Hide new hits / drive fade
        foreach (var r in thisFrameHits)
        {
            if (!activeHidden.ContainsKey(r))
            {
                var st = HideRenderer(r);
                if (st != null) activeHidden[r] = st;
            }
            else
            {
                if (fadeMaterial == null && !useShadowsOnly)
                    DriveFadeAlpha(activeHidden[r], fadeAlpha, Time.deltaTime);
            }
        }

        // Restore non-hits this frame
        if (activeHidden.Count > 0)
        {
            var toRestore = ListPool<Renderer>.Get();
            foreach (var kv in activeHidden)
                if (!thisFrameHits.Contains(kv.Key)) toRestore.Add(kv.Key);

            for (int i = 0; i < toRestore.Count; i++)
            {
                var r = toRestore[i];
                RestoreRenderer(activeHidden[r]);
                activeHidden.Remove(r);
            }
            ListPool<Renderer>.Release(toRestore);
        }
    }

    // ----------------- Hiding / Restoring -----------------
    HiddenState HideRenderer(Renderer r)
    {
        var st = new HiddenState { rend = r, originalShadowMode = r.shadowCastingMode };

        if (useShadowsOnly)
        {
            r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            return st;
        }

        if (fadeMaterial != null)
        {
            st.originalMats = r.sharedMaterials;
            int n = st.originalMats.Length;
            var swaps = new Material[n];
            for (int i = 0; i < n; i++) swaps[i] = fadeMaterial;
            r.sharedMaterials = swaps;
            return st;
        }

        // In-place fade
        var shared = r.sharedMaterials;
        st.instancedMats = new Material[shared.Length];
        st.originalColors = new Color[shared.Length];
        for (int i = 0; i < shared.Length; i++)
        {
            var inst = r.materials[i]; // creates/returns an instance
            st.instancedMats[i] = inst;

            if (inst.HasProperty("_Color"))
            {
                st.originalColors[i] = inst.color;
                SetMaterialTransparent(inst);
            }
        }
        st.currentAlpha = 1f;
        DriveFadeAlpha(st, fadeAlpha, Time.deltaTime);
        return st;
    }

    void RestoreRenderer(HiddenState st)
    {
        if (st == null || st.rend == null) return;

        if (useShadowsOnly)
        {
            st.rend.shadowCastingMode = st.originalShadowMode;
            return;
        }

        if (fadeMaterial != null && st.originalMats != null)
        {
            st.rend.sharedMaterials = st.originalMats;
            return;
        }

        if (st.instancedMats != null && st.originalColors != null)
        {
            for (int i = 0; i < st.instancedMats.Length; i++)
            {
                var m = st.instancedMats[i];
                if (!m) continue;
                if (m.HasProperty("_Color"))
                {
                    m.color = st.originalColors[i];
                    SetMaterialOpaque(m);
                }
            }
        }
    }

    void DriveFadeAlpha(HiddenState st, float targetAlpha, float dt)
    {
        if (st.instancedMats == null) return;
        st.currentAlpha = Mathf.Lerp(st.currentAlpha, targetAlpha, Mathf.Clamp01(fadeLerpSpeed * dt));
        for (int i = 0; i < st.instancedMats.Length; i++)
        {
            var m = st.instancedMats[i];
            if (!m) continue;
            if (m.HasProperty("_Color"))
            {
                var c = m.color;
                c.a = st.currentAlpha;
                m.color = c;
            }
        }
    }

    // New: restore all currently hidden renderers
    void RestoreAllHidden()
    {
        foreach (var kv in activeHidden) RestoreRenderer(kv.Value);
        activeHidden.Clear();
        thisFrameHits.Clear();
    }

    // ----------------- Toggle API -----------------
    public bool HidingEnabled => hidingEnabled;

    public void SetHiding(bool enabled)
    {
        if (hidingEnabled == enabled) return;
        hidingEnabled = enabled;
        if (!hidingEnabled) RestoreAllHidden();
    }

    public void ToggleHiding() => SetHiding(!hidingEnabled);
    public void EnableHiding() => SetHiding(true);
    public void DisableHiding() => SetHiding(false);

    // ----------------- Math helpers -----------------
    static Vector3[] GetBoundsCorners(Bounds b)
    {
        Vector3 c = b.center;
        Vector3 e = b.extents;
        return new Vector3[]
        {
            new Vector3(c.x - e.x, c.y - e.y, c.z - e.z),
            new Vector3(c.x - e.x, c.y - e.y, c.z + e.z),
            new Vector3(c.x - e.x, c.y + e.y, c.z - e.z),
            new Vector3(c.x - e.x, c.y + e.y, c.z + e.z),
            new Vector3(c.x + e.x, c.y - e.y, c.z - e.z),
            new Vector3(c.x + e.x, c.y - e.y, c.z + e.z),
            new Vector3(c.x + e.x, c.y + e.y, c.z - e.z),
            new Vector3(c.x + e.x, c.y + e.y, c.z + e.z),
        };
    }

    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c, float epsilon = 0f)
    {
        // Barycentric point-in-triangle with small tolerance
        float s = a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y;
        float t = a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y;

        if ((s < -epsilon) != (t < -epsilon)) return false;

        float A = -b.y * c.x + a.y * (c.x - b.x) + a.x * (b.y - c.y) + b.x * c.y;
        if (A < 0f) { s = -s; t = -t; A = -A; }
        float invA = 1f / A;
        float w = 1f - (s + t) * invA;
        return (s >= -epsilon) && (t >= -epsilon) && (w >= -epsilon);
    }

    struct AABB2 { public Vector2 min, max; }
    static AABB2 ScreenAABB(Vector2[] pts)
    {
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (int i = 0; i < pts.Length; i++)
        {
            var p = pts[i];
            if (p.x < min.x) min.x = p.x; if (p.y < min.y) min.y = p.y;
            if (p.x > max.x) max.x = p.x; if (p.y > max.y) max.y = p.y;
        }
        return new AABB2 { min = min, max = max };
    }
    static bool PointInAABB(Vector2 p, AABB2 aabb)
    {
        return p.x >= aabb.min.x && p.x <= aabb.max.x && p.y >= aabb.min.y && p.y <= aabb.max.y;
    }

    static void SetMaterialTransparent(Material m)
    {
        m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = (int)RenderQueue.Transparent;
    }
    static void SetMaterialOpaque(Material m)
    {
        m.SetInt("_SrcBlend", (int)BlendMode.One);
        m.SetInt("_DstBlend", (int)BlendMode.Zero);
        m.SetInt("_ZWrite", 1);
        m.DisableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = (int)RenderQueue.Geometry;
    }

    // tiny no-GC list pool
    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new();
        public static List<T> Get() => pool.Count > 0 ? pool.Pop() : new List<T>(16);
        public static void Release(List<T> list) { list.Clear(); pool.Push(list); }
    }
}
