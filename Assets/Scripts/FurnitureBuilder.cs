using UnityEngine;
using System.Collections.Generic;

public class FurnitureBuilder : MonoBehaviour
{
    // Game Manager
    [SerializeField] GameManager gameManager;
    // Visual Effect
    public GameObject placingEffectModel;

    // Audio 
    public MusicManager musicManager;

    private GameObject selectedPiece;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;

    // Marker highlighting
    private List<Renderer> highlightedMarkers = new List<Renderer>();
    private Dictionary<Renderer, Color> markerOriginalColors = new Dictionary<Renderer, Color>();

    void Update()
    {
        if (!gameManager.GetIsBuildingEnabled()) return;

        // Doesn't work! Competes with FurnitureRotator's code. Hacky way to turn off/on player movement
        // if (selectedPiece != null && gameManager.GetIsPlayerMovementEnabled())
        // {
        //     gameManager.SetIsPlayerMovementEnabled(false);
        // }
        // else if (selectedPiece == null && !gameManager.GetIsPlayerMovementEnabled())
        // {
        //     gameManager.SetIsPlayerMovementEnabled(true);

        // }

        HandleSelection();
        HandleAttachment();
    }

    void HandleSelection()
    {

        if (!IsRightClick()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        GameObject clicked = hit.collider.gameObject;

        // Ignore markers for selection
        if (!clicked.CompareTag("Marker"))
        {
            if (clicked == selectedPiece)
                DeselectPiece();
            else
                SelectPiece(clicked);
        }
    }

    void HandleAttachment()
    {
        if (!IsRightClick() || selectedPiece == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        GameObject clicked = hit.collider.gameObject;

        if (clicked.CompareTag("Marker"))
        {
            AttachPieceToMarker(clicked.transform);

            // Destroy the original selected piece
            Destroy(selectedPiece);
            DeselectPiece();
        }
    }

    bool IsRightClick()
    {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                return UnityEngine.InputSystem.Mouse.current != null &&
                       UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame;
        #else
                return Input.GetMouseButtonDown(1);
        #endif
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
            selectedRenderer.material.color = Color.red;
        }

        HighlightAllMarkers();

        Debug.Log("Selected piece: " + selectedPiece.name);
    }

    void DeselectPiece()
    {
        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        selectedPiece = null;
        selectedRenderer = null;

        RestoreMarkerColors();
    }

    void HighlightAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

        foreach (GameObject marker in markers)
        {
            // Match format: Piece_marker, Piece_marker (1), etc.
            if (marker.name.EndsWith("_marker") || marker.name.Contains("_marker ("))
            {
                Renderer rend = marker.GetComponent<Renderer>();
                if (rend != null && !markerOriginalColors.ContainsKey(rend))
                {
                    markerOriginalColors[rend] = rend.material.color;
                    rend.material.color = Color.red;
                    highlightedMarkers.Add(rend);
                }
            }
        }
    }

    void RestoreMarkerColors()
    {
        foreach (Renderer rend in highlightedMarkers)
        {
            if (rend != null && markerOriginalColors.ContainsKey(rend))
                rend.material.color = markerOriginalColors[rend];
        }

        highlightedMarkers.Clear();
        markerOriginalColors.Clear();
    }

    void AttachPieceToMarker(Transform marker)
    {
        if (selectedPiece == null)
        {
            Debug.LogError("No piece selected to attach!");
            return;
        }

        GameObject newPiece = Instantiate(selectedPiece);

        // Instantiate the effect
        GameObject placingEffect = Instantiate(placingEffectModel);
        placingEffect.transform.position = marker.position;
        Destroy(placingEffect, 2.5f);

        // Audio Effect
        musicManager.PlayAttaching();

        // Match rotation with marker
        newPiece.transform.rotation = marker.rotation;

        // Snap piece so bottom aligns with marker
        Renderer rend = newPiece.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Bounds localBounds = rend.localBounds;
            Vector3 bottomLocal = localBounds.center - Vector3.up * localBounds.extents.y;
            Vector3 bottomWorld = rend.transform.TransformPoint(bottomLocal);
            Vector3 offset = marker.position - bottomWorld;
            newPiece.transform.position += offset;
        }
        else
        {
            newPiece.transform.position = marker.position;
        }

        // Use the original piece's parent, not the marker's parent
        if (selectedPiece != null && selectedPiece.transform.parent != null)
        {
            newPiece.transform.SetParent(selectedPiece.transform.parent, true);
        }
        else if (marker.parent != null) // fallback to marker parent
        {
            newPiece.transform.SetParent(marker.parent, true);
        }

        Debug.Log($"Attached new piece {newPiece.name} to marker {marker.name}");
    }
}
