using System.ComponentModel;
using UnityEngine;

/*
This is used to auto-generate FContainers and link PlacementAreas to a furniture.

Requirements:
- A PlacementArea game object must exist in scene. We have to place them manually anyways.
*/
[CreateAssetMenu(fileName = "FurnitureSO", menuName = "Scriptable Objects/FurnitureSO")]
public class FurnitureDataSO : ScriptableObject
{
    public string baseName;
    public int numTotalPieces;
    public GameObject fContainerPrefab;   // prefab reference for this furniture
}
