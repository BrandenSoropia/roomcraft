using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FurnitureBuilder : MonoBehaviour
{
    [Header("Game Manager")]
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

    // Button Press State
    [SerializeField] bool _isRightTriggerPressed = false;

    void Update()
    {
        if (!gameManager.GetIsBuildingEnabled()) return;
    }

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

        Debug.Log("### clicked: " + clicked.gameObject.name);

        if (selectedPiece == null)
        {
            SelectPiece(clicked);
        }
        else if (selectedPiece != null && clicked == selectedPiece)
        {
            DeselectPiece();
        }
        else if (selectedPiece != null && clicked.CompareTag("Marker"))
        {
            HandleAttachment(clicked);
        }
    }

    void HandleAttachment(GameObject clicked)
    {
        AttachPieceToMarker(clicked.transform);

        // Destroy the original selected piece
        Destroy(selectedPiece);
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
            selectedRenderer.material.color = Color.red;
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

        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

        foreach (GameObject marker in markers)
        {
            // Match only markers that start with markerPrefix
            if (marker.name.StartsWith(markerPrefix))
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

        // Apply local scale of the selected piece
        newPiece.transform.localScale = selectedPiece.transform.localScale;

        // If parent is scaled, match that too
        if (selectedPiece.transform.parent != null)
        {
            newPiece.transform.localScale = Vector3.Scale(
                selectedPiece.transform.localScale,
                selectedPiece.transform.parent.localScale
            );
        }

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
