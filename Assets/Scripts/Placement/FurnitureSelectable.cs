using UnityEngine;

[DisallowMultipleComponent]
public class FurnitureSelectable : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowIntensity = 2f;

    public Rigidbody RB { get; private set; }   // <— bring RB back

    private Renderer[] renderersToTint;

    void Awake()
    {
        RB = GetComponent<Rigidbody>(); // cache once

        // auto-grab all renderers
        renderersToTint = GetComponentsInChildren<Renderer>(true);

        // make sure emission keyword exists; start off
        foreach (var r in renderersToTint)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (renderersToTint == null) return;

        foreach (var r in renderersToTint)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor",
                        selected ? glowColor * glowIntensity : Color.black);
                }
            }
        }
    }

    public Vector3 GetWorldRotationPivot()
    {
        // Use renderers to estimate footprint center (XZ) and bottom (Y)
        var rends = GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return transform.position;
        Bounds wb = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) wb.Encapsulate(rends[i].bounds);
        return new Vector3(wb.center.x, wb.min.y, wb.center.z);
    }
}
