using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FurnitureBuilder : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] GameManager gameManager;
    [SerializeField] FurnitureManager furnitureManager;

    // Visual Effect
    public GameObject placingEffectModel;
    // Audio 
    public MusicManager musicManager;

    [Header("SFX Controller")]
    [SerializeField] PlayerSFXController playerSFXController;


    private GameObject selectedPiece;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;
    
    private readonly Color lightGreen = new Color(0.66f, 0.78f, 0.52f, 1f); 

    // Marker highlighting
    private List<Renderer> highlightedMarkers = new List<Renderer>();
    private Dictionary<Renderer, Color> markerOriginalColors = new Dictionary<Renderer, Color>();

    /*
    Project a ray forward from the player's viewpoint (a.k.a the screen). This is required for aiming.
    Example: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html
    */
    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    /*
    Listen for the BuildSelection button to be pressed.
    Thus it should be triggered only when the button is pressed once! 
    Configured from Player Input.StarterAssets.
    */
    public void OnBuildSelection(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        if (!Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit)) return;

        GameObject clicked = hit.collider.gameObject;

        if (clicked.CompareTag("Untagged") || clicked.CompareTag("FurnitureBox"))
        {
            return;
        }

        Debug.Log("### clicked: " + clicked.gameObject.name);

        if (selectedPiece == null)
        {
            if (clicked.CompareTag("Furniture"))
        	{
            	SelectPiece(clicked);
                playerSFXController.PlaySelectPieceSFX();
            } 
        	else
        	{
            	Debug.Log("Clicked object is not part of Furniture — selection ignored.");
        	}
        }
        else if (selectedPiece != null && clicked == selectedPiece)
        {
            DeselectPiece();
            playerSFXController.PlayDeselectPieceSFX();

        }
        else if (selectedPiece != null && clicked.CompareTag("Marker"))
        {
            HandleAttachment(clicked);
            playerSFXController.PlayAttachSFX();

        }
    }

    void HandleAttachment(GameObject clicked)
    {
        AttachPieceToMarker(clicked);

        // Destroy the original selected piece
        // Destroy(selectedPiece);
        DeselectPiece();
    }

    void SelectPiece(GameObject piece)
    {
        // Restore previous selection
        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        RestoreMarkerColors();

        selectedPiece = piece;
        selectedRenderer = selectedPiece.GetComponent<Renderer>();

        if (selectedRenderer != null)
        {
            selectedOriginalColor = selectedRenderer.material.color;
            selectedRenderer.material.color = lightGreen;
        }

        HighlightAllMarkers();

        Debug.Log("Selected piece: " + selectedPiece.name);
    }

    void DeselectPiece()
    {
        Debug.Log("### Build Deselected!");

        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        selectedPiece = null;
        selectedRenderer = null;

        RestoreMarkerColors();
    }

    void HighlightAllMarkers()
    {
        // Derive base name"
        string baseName = selectedPiece.name;
        if (baseName.Contains("("))
        {
            baseName = baseName.Substring(0, baseName.IndexOf("(")).Trim();
        }

        // Expected marker prefix"
        string markerPrefix = baseName + "_marker";
        string sideMarkerPrefix = baseName + "_side_marker";

        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

        foreach (GameObject marker in markers)
        {
            // Match only markers that start with markerPrefix
            if (marker.name.StartsWith(markerPrefix) || marker.name.StartsWith(sideMarkerPrefix))
            {
                Renderer rend = marker.GetComponent<Renderer>();
                if (rend != null && !markerOriginalColors.ContainsKey(rend))
                {
                    markerOriginalColors[rend] = rend.material.color;
                    rend.material.color = lightGreen;
                    highlightedMarkers.Add(rend);
                }
            }
        }
    }

    void RestoreMarkerColors()
    {
        if (highlightedMarkers.Count != 0)
        {
            foreach (Renderer rend in highlightedMarkers)
            {
                if (rend != null && markerOriginalColors.ContainsKey(rend))
                    rend.material.color = markerOriginalColors[rend];
            }

            highlightedMarkers.Clear();
            markerOriginalColors.Clear();
        }
    }

    void AttachPieceToMarker(GameObject marker)
    {
        if (selectedPiece == null)
        {
            Debug.LogError("No piece selected to attach!");
            return;
        }

        // GameObject newPiece = Instantiate(selectedPiece);

        // Apply local scale of the selected piece
        // newPiece.transform.localScale = selectedPiece.transform.localScale;

        // // If parent is scaled, match that too
        // if (selectedPiece.transform.parent != null)
        // {
        //     newPiece.transform.localScale = Vector3.Scale(
        //         selectedPiece.transform.localScale,
        //         selectedPiece.transform.parent.localScale
        //     );
        // }

        // Instantiate the effect
        GameObject placingEffect = Instantiate(placingEffectModel);
        placingEffect.transform.position = marker.transform.position;
        Destroy(placingEffect, 2.5f);

        // Audio Effect
        musicManager.PlayAttaching();

        // Match rotation with marker
        selectedPiece.transform.rotation = marker.transform.rotation;


        // Snap piece depending on marker type
        Renderer rend = selectedPiece.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Bounds localBounds = rend.localBounds;
            Vector3 offset = Vector3.zero;

            if (marker.name.Contains("_side_marker"))
            {
                // Snap the side of the piece to the marker
                Vector3 sideLocal = localBounds.center - Vector3.forward * localBounds.extents.z; // front face of piece
                Vector3 sideWorld = rend.transform.TransformPoint(sideLocal);
                offset = marker.transform.position - sideWorld;
            }
            else
            {
                // Default: snap bottom of the piece to marker
                Vector3 bottomLocal = localBounds.center - Vector3.up * localBounds.extents.y;
                Vector3 bottomWorld = rend.transform.TransformPoint(bottomLocal);
                offset = marker.transform.position - bottomWorld;
            }

            selectedPiece.transform.position += offset;
        }
        else
        {
            selectedPiece.transform.position = marker.transform.position;
        }

        /*
        When the first ever piece of a furniture is attached to another,
        basically set the FContainer as both their parent. 
        */
        Transform basePieceParentTransform = marker.transform.parent.transform?.parent;
        if ((basePieceParentTransform == null) || !basePieceParentTransform.CompareTag("FContainer"))
        {
            furnitureManager.AttachPieceToFContainer(marker.transform.parent.gameObject);
        }

        furnitureManager.AttachPieceToFContainer(selectedPiece);
        
        // Assign marker's parent as parent of selected piece
        if (marker.transform.parent != null)
        {
            selectedPiece.transform.SetParent(marker.transform.parent, true);
        }


        Debug.Log($"Attached new piece {selectedPiece.name} to marker {marker.name}");
    }

    public void OnDeletePiece(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        if (selectedPiece != null)
        {
            GameObject _pieceToDestroy = selectedPiece;

            DeselectPiece(); // Reset state before deleting

            Destroy(_pieceToDestroy);

            playerSFXController.PlayDeselectPieceSFX();
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
            case GameState.IsometricMode:
                DeselectPiece();
                break;
        }
    }
}
