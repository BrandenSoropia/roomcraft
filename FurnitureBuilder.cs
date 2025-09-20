using UnityEngine;

public class FurnitureBuilder : MonoBehaviour
{
    public bool deselectAfterAttach = true;

    private GameObject selectedLeg;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;

    void Update()
    {
        // Right click handling (works with legacy and new Input System)
        if (IsRightClick())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                // If clicked object is a marker -> try attach
                if (clicked.CompareTag("Marker"))
                {
                    if (selectedLeg != null)
                    {
                        AttachLegToMarker(selectedLeg, clicked.transform);
                        if (deselectAfterAttach) DeselectLeg();
                    }
                }
                else
                {
                    // Otherwise treat the clicked object as a selectable "leg/piece"
                    if (clicked == selectedLeg) DeselectLeg();
                    else SelectLeg(clicked);
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
        // restore previous
        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        selectedLeg = leg;
        selectedRenderer = selectedLeg.GetComponent<Renderer>();

        if (selectedRenderer != null)
        {
            selectedOriginalColor = selectedRenderer.material.color;
            selectedRenderer.material.color = Color.red;
        }

        Debug.Log("Selected as target: " + selectedLeg.name);
    }

    void DeselectLeg()
    {
        if (selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        if (selectedLeg != null)
            Debug.Log("Deselected: " + selectedLeg.name);

        selectedLeg = null;
        selectedRenderer = null;
    }

    void AttachLegToMarker(GameObject leg, Transform marker)
    {
        if (leg == null || marker == null) return;

        // Assume local Y axis is the leg’s long axis
        Vector3 legUp = leg.transform.up;

        // Get leg length in world units
        MeshFilter mf = leg.GetComponentInChildren<MeshFilter>();
        if (mf == null)
        {
            Debug.LogWarning("Leg has no MeshFilter to compute length.");
            return;
        }

        Bounds localBounds = mf.sharedMesh.bounds;
        float legLength = localBounds.size.y * leg.transform.lossyScale.y;

        // Get table reference (parent of marker)
        Transform table = marker.parent;
        Vector3 tableUp = table != null ? table.up : Vector3.up;

        // Decide which end of the leg should touch the marker
        // If leg’s up axis points in the same direction as table’s up → bottom should go to marker
        // If opposite → top should go to marker
        float dot = Vector3.Dot(legUp, tableUp);
        int side = (dot > 0) ? -1 : 1;

        // Snap the correct end of leg to marker
        Vector3 offset = leg.transform.up * (legLength / 2f) * side;
        leg.transform.position = marker.position - offset;

        // Align orientation so leg "up" matches table "up"
        leg.transform.rotation = Quaternion.LookRotation(table.forward, tableUp);

        if (table != null)
            leg.transform.SetParent(table, true);

        Debug.Log($"Attached {leg.name} to {marker.name}, correct side aligned");
    }
}
