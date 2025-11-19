using StarterAssets;
using UnityEngine;

public class GlowOutlineController : MonoBehaviour
{
    public MeshRenderer targetMeshRenderer; // assign in Inspector
    public int totalPieces;
    private int currPieces;

    public float minScale = 1.03f;
    public float maxScale = 1.12f;
    public float speed = 1f;

    public Material outline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currPieces = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (outline != null)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);  // goes between 0 and 1
            float scale = Mathf.Lerp(minScale, maxScale, t);
            outline.SetFloat("_outline_scale", scale);
        }
    }


    public void RemoveOutline()
    {
        targetMeshRenderer.enabled = false;
        Debug.Log("remove glow outline");
    }

    public void AddPieces()
    {
        currPieces += 1;

        if (currPieces >= totalPieces)
        {
            RemoveOutline();
        }
    }
}
