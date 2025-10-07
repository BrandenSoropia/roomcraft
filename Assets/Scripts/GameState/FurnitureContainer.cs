using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class FurnitureContainer : MonoBehaviour
{
    [SerializeField] int totalPieces = 1;
    [SerializeField] int numAttachedPieces = 0;


    public void OnAssemblePiece()
    {
        numAttachedPieces += 1;
    }

    public bool CheckFullyAssembled()
    {
        return numAttachedPieces == totalPieces;
    }
}
