using System.Collections.Generic;
using UnityEngine;

/*
Place this on each PlacementArea.
1 PlacementArea to 1 furniture.

Requirements:
- Each furniture piece must have:
  - a collider
  - tagged as "Furniture" since we have to count all colliders present
*/
public class AreaTrigger : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    [Header("Indicator Colours")]
    [SerializeField] Material emptyMaterial;
    [SerializeField] Material correctPlacementMaterial;
    [SerializeField] Material incorrectPlacementMaterial;


    public Transform parentObject { get; set; } // the root object whose children we care about
    public int numPieces { get; set; }

    private HashSet<Collider> inside = new HashSet<Collider>();
    private bool _hasReducedPlacementCall = false;

    MeshRenderer myMeshRenderer;
    BoxCollider myBoxCollider;

    void Start()
    {
        myMeshRenderer = GetComponent<MeshRenderer>();
        myMeshRenderer.material = emptyMaterial;
        myBoxCollider = GetComponent<BoxCollider>();

        // Turn off once game starts, but can leave on while developing so we can still see it
        myMeshRenderer.enabled = false;
        myBoxCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        string myBaseName = MyUtils.GetFurnitureGOBaseName(transform.name);

        if (other.CompareTag("Suckable"))
        {
            bool isCorrectFurniture = other.name.Contains(myBaseName, System.StringComparison.CurrentCultureIgnoreCase);

            if (!isCorrectFurniture)
            {
                myMeshRenderer.material = incorrectPlacementMaterial;
            }
            else if (isCorrectFurniture && !inside.Contains(other))
            {
                inside.Add(other);
                Debug.Log($"+1: Colliders in: {inside.Count}/{numPieces}");

                CheckIfEntirelyInArea();
            }
        }
    }

    void CheckIfEntirelyInArea()
    {
        if (inside.Count == 1)
        {
            gameManager.IncrementNumCorrectPlacementFurniture();
            _hasReducedPlacementCall = false; // Reset this so we can reduce the total placed counter again
            myMeshRenderer.material = correctPlacementMaterial;
        }
    }

    void OnTriggerExit(Collider other)
    {
        string myBaseName = MyUtils.GetFurnitureGOBaseName(transform.name);

        if (other.CompareTag("Furniture"))
        {
            if (other.name.Contains(myBaseName, System.StringComparison.CurrentCultureIgnoreCase)
            && inside.Contains(other))
            {
                inside.Remove(other);
                Debug.Log($"-1: Colliders in: {inside.Count}/{numPieces}");

                /*
                Only allow reducing the total placement counter once per exit.
                Without this, it will decrement the progress each time any piece exits the area.
                */
                if (!_hasReducedPlacementCall)
                {
                    gameManager.DecrementNumCorrectPlacementFurniture();
                    _hasReducedPlacementCall = true;
                }
            }

            // Reset to empty colour
            if (inside.Count == 0 && myMeshRenderer.material != emptyMaterial)
            {
                myMeshRenderer.material = emptyMaterial;
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe when enabled
        gameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Always unsubscribe to avoid memory leaks
        gameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.BuildMode:
                HideAreaTrigger();
                break;
            case GameState.IsometricMode:
                ShowAreaTrigger();
                break;
            default:
                Debug.Log("Unknown state changed");
                break;
        }
    }

    void HideAreaTrigger()
    {
        myBoxCollider.enabled = false;
        myMeshRenderer.enabled = false;
    }

    void ShowAreaTrigger()
    {
        myBoxCollider.enabled = true;
        myMeshRenderer.enabled = true;
    }
}