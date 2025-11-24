using UnityEngine;

[ExecuteAlways]
public class WallHidingShaderController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The material using the SeeThroughWall shader")]
    public Material wallMaterial;

    [Tooltip("Enable or disable the wall hiding effect")]
    public bool hidingEnabled = true;
    
    [Tooltip("The object to center the cutout on (e.g. Player). If null, uses screen center.")]
    public Transform targetObject;

    [Tooltip("Radius of the cutout in viewport space (0-1)")]
    [Range(0f, 1f)]
    public float cutoutRadius = 0.7f;

    [Tooltip("Softness of the cutout edge")]
    [Range(0f, 0.5f)]
    public float cutoutSoftness = 0.1f;

    private Camera cam;
    private static readonly int CutoutCenterID = Shader.PropertyToID("_CutoutCenter");
    private static readonly int CutoutRadiusID = Shader.PropertyToID("_CutoutRadius");
    private static readonly int CutoutSoftnessID = Shader.PropertyToID("_CutoutSoftness");

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (wallMaterial == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector2 center = new Vector2(0.5f, 0.5f);

        if (targetObject != null)
        {
            Vector3 screenPos = cam.WorldToViewportPoint(targetObject.position);
            // Check if target is behind camera
            if (screenPos.z < 0)
            {
                // If behind, maybe move cutout off screen or keep last known? 
                // For now, let's just keep it at center or hide it.
                // Setting radius to 0 effectively hides the cutout.
                // But let's just clamp it.
            }
            center = new Vector2(screenPos.x, screenPos.y);
        }

        wallMaterial.SetVector(CutoutCenterID, center);
        
        // If hiding is disabled, set radius to -1 to make everything opaque
        float effectiveRadius = hidingEnabled ? cutoutRadius : -1f;
        wallMaterial.SetFloat(CutoutRadiusID, effectiveRadius);
        wallMaterial.SetFloat(CutoutSoftnessID, cutoutSoftness);
    }
}
