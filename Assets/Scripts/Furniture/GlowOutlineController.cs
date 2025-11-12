using StarterAssets;
using UnityEngine;

public class GlowOutlineController : MonoBehaviour
{
    public MeshRenderer targetMeshRenderer; // assign in Inspector
    public int totalPieces;
    private int currPieces;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currPieces = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
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
