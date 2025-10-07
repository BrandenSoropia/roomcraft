using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class FurniturePiece : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public List<Mesh> models;
    public Mesh defaultModel;
    public Sprite icon;
    public bool isPlaced = false;
    // private Furniture parentFurniture;

    // private void Awake()
    // {
    //     parentFurniture = GetComponentInParent<Furniture>();
    // }

    // public void PlacePiece()
    // {
    //     if (!isPlaced)
    //     {
    //         isPlaced = true;
    //         parentFurniture.CheckCompletion();
    //     }
    // }
}
