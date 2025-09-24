using UnityEngine;

[DisallowMultipleComponent]
public class FurnitureSelectable : MonoBehaviour
{
    [Header("Optional highlight")]
    [SerializeField] private Renderer[] renderersToTint;
    [SerializeField] private float selectedEmission = 1.25f;

    public Rigidbody RB { get; private set; }

    Color[] _baseEmissions;
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        RB = GetComponent<Rigidbody>();
        if (renderersToTint == null || renderersToTint.Length == 0)
            renderersToTint = GetComponentsInChildren<Renderer>(true);

        _baseEmissions = new Color[renderersToTint.Length];
        for (int i = 0; i < renderersToTint.Length; i++)
        {
            var r = renderersToTint[i];
            if (r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty(EmissionColor))
            {
                _baseEmissions[i] = r.sharedMaterial.GetColor(EmissionColor);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (renderersToTint == null) return;

        for (int i = 0; i < renderersToTint.Length; i++)
        {
            var r = renderersToTint[i];
            if (r == null || r.material == null) continue;

            if (r.material.HasProperty("_EmissionColor"))
            {
                if (selected)
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", Color.yellow * 2f); // bright yellow glow
                }
                else
                {
                    r.material.SetColor("_EmissionColor", Color.black); // reset
                }
            }
        }
    }

}
