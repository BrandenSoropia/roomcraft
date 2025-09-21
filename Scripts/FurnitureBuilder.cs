using UnityEngine;
using System.Collections.Generic;

public class FurnitureBuilder : MonoBehaviour
{
    public GameObject legPrefab;
    private GameObject selectedLeg;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;

    // For markers
    private List<Renderer> highlightedMarkers = new List<Renderer>();
    private Dictionary<Renderer, Color> markerOriginalColors = new Dictionary<Renderer, Color>();

    void Update()
    {
        HandleSelection();
        HandleAttachment();
    }

    void HandleSelection()
    {
        // Right click to select a leg
        if (IsRightClick())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                // Ignore markers for selection
                if (!clicked.CompareTag("Marker"))
                {
                    if (clicked == selectedLeg)
                        DeselectLeg();
                    else
                        SelectLeg(clicked);
                }
            }
        }
    }

    void HandleAttachment()
    {
        // Right click on marker to attach the selected leg
        if (IsRightClick() && selectedLeg != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                if (clicked.CompareTag("Marker"))
                {
                    AttachLegToMarker(clicked.transform);

                    // Destroy the original selected leg
                    Destroy(selectedLeg);
                    DeselectLeg();
                }
            }
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

    void SelectLeg(GameObject leg)
    {
        // Restore previous selection
        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        RestoreMarkerColors();

        selectedLeg = leg;
        selectedRenderer = selectedLeg.GetComponent<Renderer>();

        if (selectedRenderer != null)
        {
            selectedOriginalColor = selectedRenderer.material.color;
            selectedRenderer.material.color = Color.red;
        }

        HighlightAllMarkers();

        Debug.Log("Selected leg: " + selectedLeg.name);
    }

    void DeselectLeg()
    {
        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        selectedLeg = null;
        selectedRenderer = null;

        RestoreMarkerColors();
    }

    void HighlightAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

        foreach (GameObject marker in markers)
        {
            // Match "Leg_marker", "Leg_marker (1)", "Leg_marker (2)" ...
            if (marker.name.StartsWith("Leg_marker"))
            {
                Renderer rend = marker.GetComponent<Renderer>();
                if (rend != null)
                {
                    // Save original color if not already saved
                    if (!markerOriginalColors.ContainsKey(rend))
                        markerOriginalColors[rend] = rend.material.color;

                    // Highlight in red
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

    void AttachLegToMarker(Transform marker)
    {
        if (legPrefab == null)
        {
            Debug.LogError("Leg prefab not assigned in the Inspector!");
            return;
        }

        GameObject newLeg = Instantiate(legPrefab);

        // Align rotation with marker/table
        newLeg.transform.rotation = marker.rotation;

        // Position leg so its bottom sits exactly on marker
        Renderer rend = newLeg.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Bounds localBounds = rend.localBounds;
            Vector3 legBottomLocal = localBounds.center - Vector3.up * localBounds.extents.y;

            // Transform leg bottom to world space
            Vector3 legBottomWorld = rend.transform.TransformPoint(legBottomLocal);

            // Offset leg so bottom aligns with marker
            Vector3 offset = marker.position - legBottomWorld;
            newLeg.transform.position += offset;
        }
        else
        {
            // Place at marker position
            newLeg.transform.position = marker.position;
        }

        // Parent to marker's parent (usually the table)
        if (marker.parent != null)
            newLeg.transform.SetParent(marker.parent, true);

        Debug.Log($"Attached new leg {newLeg.name} exactly on top of marker {marker.name}");
    }
}
