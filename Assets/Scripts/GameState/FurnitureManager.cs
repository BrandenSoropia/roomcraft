using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/*
Requirements:
- An empty game object with name "<furniture_name>_FContainer" must exist per furniture.
  - These containers are used to have the FurnitureSelectable and rigidbody needed for placement controls
- Per unique furniture model, every piece has to have the same base name. Example: PRP_Bed_01... for sides, base, matress etc. 
*/
public class FurnitureManager : MonoBehaviour
{
    [SerializeField] private List<FurnitureDataSO> furnitureList;
    [SerializeField] GameObject[] placementAreas;

    private Dictionary<string, FurnitureState> _furnitureStates;
    [SerializeField] List<GameObject> _furnitureContainers = new List<GameObject>();


    // Build furniture state given furniture data and create FContainers
    void Awake()
    {
        _furnitureStates = new Dictionary<string, FurnitureState>();

        foreach (var data in furnitureList)
        {
            if (!_furnitureStates.ContainsKey(data.baseName))
            {
                _furnitureStates[data.baseName] = new FurnitureState(data);

            }
            else
            {
                Debug.LogWarning($"Duplicate baseName found: {data.baseName}. Only the first will be tracked.");
            }
        }
    }

    void Start()
    {
        foreach (var data in furnitureList)
        {
            GameObject newFContainer = CreateFurnitureContainer(data.baseName, data.fContainerPrefab);
            // SetPlacementAreaTargetFurniture(data.baseName, newFContainer.transform);
        }
    }

    GameObject CreateFurnitureContainer(string baseName, GameObject prefab)
    {
        GameObject newFContainer = Instantiate(prefab);
        newFContainer.name = baseName + "_FContainer";

        _furnitureContainers.Add(newFContainer);

        return newFContainer;
    }

    // Required or else placement areas won't know what specific furniture to check for
    void SetPlacementAreaTargetFurniture(string baseName, Transform targettransform)
    {
        GameObject found = placementAreas.FirstOrDefault(obj =>
                    obj.name.Contains(baseName, System.StringComparison.CurrentCultureIgnoreCase));

        found.GetComponent<AreaTrigger>().parentObject = targettransform;
    }

    public void AttachPieceToFContainer(GameObject piece)
    {
        string[] namePieces = piece.name.Split(new[] { "_" }, System.StringSplitOptions.None);

        string baseName = $"{namePieces[0]}_{namePieces[1]}_{namePieces[2]}";

        // get the container with the matching base name of the selected piece
        GameObject found = _furnitureContainers.FirstOrDefault(obj =>
                    obj.name.Contains(baseName, System.StringComparison.CurrentCultureIgnoreCase));

        piece.transform.SetParent(found.transform, true);
    }

    public void AddPiece(string baseName)
    {
        if (_furnitureStates.TryGetValue(baseName, out FurnitureState state))
        {
            state.assembledPieces++;
            Debug.Log($"{baseName} progress: {state.assembledPieces}/{state.data.numTotalPieces}");

            if (state.IsComplete)
            {
                OnFurnitureCompleted(baseName);
            }
        }
        else
        {
            Debug.LogWarning($"No furniture with baseName: {baseName}");
        }
    }

    private void OnFurnitureCompleted(string baseName)
    {
        Debug.Log($"{baseName} is fully built!");
        // Add completion logic (rewards, unlock next stage, etc.)
    }

    public FurnitureState GetState(string baseName)
    {
        return _furnitureStates.ContainsKey(baseName) ? _furnitureStates[baseName] : null;
    }
}
