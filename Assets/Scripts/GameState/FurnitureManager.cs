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
    [SerializeField] GameManager gameManager;
    [SerializeField] private List<FurnitureDataSO> furnitureList;
    [SerializeField] GameObject[] placementAreas;

    public Dictionary<string, FurnitureState> _furnitureStates;
    [SerializeField] List<GameObject> _furnitureContainers = new List<GameObject>();

    public PlacementFlash flashControler;


    // Build furniture state given furniture data and create FContainers
    void Awake()
    {
        BuildFurniturePieceState();
    }

    void BuildFurniturePieceState()
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
            SetPlacementAreaTargetFurniture(data, newFContainer.transform);
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
    void SetPlacementAreaTargetFurniture(FurnitureDataSO data, Transform targetTransform)
    {
        GameObject found = placementAreas.FirstOrDefault(obj =>
                    obj.name.Contains(data.baseName, System.StringComparison.CurrentCultureIgnoreCase));

        if (found != null)
        {
            AreaTrigger at = found.GetComponent<AreaTrigger>();
            at.parentObject = targetTransform;
            at.numPieces = data.numTotalPieces;
        }
    }

    public void AttachPieceToFContainer(GameObject piece)
    {
        string[] namePieces = piece.name.Split(new[] { "_" }, System.StringSplitOptions.None);

        string baseName = $"{namePieces[0]}_{namePieces[1]}_{namePieces[2]}";

        // get the container with the matching base name of the selected piece
        GameObject fContainer = _furnitureContainers.FirstOrDefault(obj =>
                    obj.name.Contains(baseName, System.StringComparison.CurrentCultureIgnoreCase));

        piece.transform.SetParent(fContainer.transform, true);

        TrackPieceCount(baseName, fContainer);
    }

    public void TrackPieceCount(string baseName, GameObject fContainer)
    {
        if (_furnitureStates.TryGetValue(baseName, out FurnitureState state))
        {
            if (!state.IsComplete)
            {
                state.assembledPieces++;
                Debug.Log($"{baseName} progress: {state.assembledPieces}/{state.data.numTotalPieces}");

                // Do the check again once incremented
                if (state.IsComplete)
                {
                    OnFurnitureCompleted(baseName, fContainer);
                }
            }
        }
        else
        {
            Debug.LogWarning($"No furniture with baseName: {baseName}");
        }
    }

    private void OnFurnitureCompleted(string baseName, GameObject fContainer)
    {
        Debug.Log($"{baseName} is fully built!");
        // log the fcontainer for further use
        
        Debug.Log($"FContainer: {fContainer.name} has been completed.");

        if (flashControler != null)
        {
            flashControler.ActivatePlacementFlash();
        }

        gameManager.IncrementNumBuilt();
        EnableFurniturePlacement(fContainer);
        fContainer.GetComponent<FurnitureSelectable>().SetMovable(true);
    }

    void EnableFurniturePlacement(GameObject fContainer)
    {
        fContainer.GetComponent<FurnitureSelectable>().enabled = true;
    }

    public FurnitureState GetState(string baseName)
    {
        return _furnitureStates.ContainsKey(baseName) ? _furnitureStates[baseName] : null;
    }
}
