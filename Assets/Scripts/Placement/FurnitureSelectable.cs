using UnityEngine;

[DisallowMultipleComponent]
public class FurnitureSelectable : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowIntensity = 2f;

    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    public bool IsMovable => canMove;

    public event System.Action<FurnitureSelectable, bool> MovableChanged;

    public Rigidbody RB { get; private set; }   // <— bring RB back

    private Renderer[] renderersToTint;
    private bool _lastCanMove;

    void Awake()
    {
        RB = GetComponent<Rigidbody>(); // cache once
        _lastCanMove = canMove;

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

    // Call this from code to toggle movability at runtime.
    public void SetMovable(bool movable)
    {
        if (canMove == movable) return;
        canMove = movable;
        _lastCanMove = canMove;
        MovableChanged?.Invoke(this, canMove);
    }

    // Detect changes made in the Inspector during play mode.
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (_lastCanMove != canMove)
        {
            _lastCanMove = canMove;
            MovableChanged?.Invoke(this, canMove);
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
