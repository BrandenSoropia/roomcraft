using UnityEngine;

public class ModelGhost : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool noCollision;
    public Color collideColor;
    public Color spawnColor;
    public MeshRenderer meshRenderer;
    void Start()
    {
        noCollision = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Furniture"))
        {
            noCollision = false;
            SetMaterialColor(collideColor);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Furniture"))
        {
            noCollision = true;
            SetMaterialColor(spawnColor);
        }
    }

    public void SetMaterialColor(Color color)
    {
        if (meshRenderer != null)
        {
            // Get all materials (creates instances for runtime modification)
            Material[] mats = meshRenderer.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                // Check if the material has the property
                if (mats[i].HasProperty("_BaseColor"))
                {
                    mats[i].SetColor("_BaseColor", color);
                }
            }

            // Apply modified materials back to the renderer
            meshRenderer.materials = mats;
        }
    }
}
