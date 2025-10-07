using System.Linq;
using UnityEngine;

/*
Requirements:
- An empty game object with name "<furniture_name>_FContainer" must exist per furniture.
  - These containers are used to have the FurnitureSelectable and rigidbody needed for placement controls
- Per unique furniture model, every piece has to have the same base name. Example: PRP_Bed_01... for sides, base, matress etc. 
*/
public class FurnitureManager : MonoBehaviour
{
    [SerializeField] GameObject[] furnitureContainers;

    public void AttachPieceToFContainer(GameObject piece)
    {
        string[] namePieces = piece.name.Split(new[] { "_" }, System.StringSplitOptions.None);

        string baseName = $"{namePieces[0]}_{namePieces[1]}_{namePieces[2]}";

        // get the container with the matching base name of the selected piece
        GameObject found = furnitureContainers.FirstOrDefault(obj =>
                    obj.name.Contains(baseName, System.StringComparison.CurrentCultureIgnoreCase));

        piece.transform.SetParent(found.transform, true);
    }
}
