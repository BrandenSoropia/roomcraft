using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class FurnitureContainer : MonoBehaviour
{
    [SerializeField] int totalPieces;
    [SerializeField] int numAttachedPieces = 0;

    FurnitureSelectable myFurnitureSelectable;

    void Start()
    {
        myFurnitureSelectable = GetComponent<FurnitureSelectable>();
        myFurnitureSelectable.enabled = false;
    }


    public void OnAssemblePiece()
    {
        numAttachedPieces += 1;
    }

    public bool CheckFullyAssembled()
    {
        return numAttachedPieces == totalPieces;
    }
}
