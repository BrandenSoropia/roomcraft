using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FurnitureRotator : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] GameManager gameManager;

    [Header("SFX Controller")]
    [SerializeField] PlayerSFXController playerSFXController;

    private List<GameObject> selectedParts = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private Dictionary<GameObject, Transform> originalParents = new Dictionary<GameObject, Transform>();

    private GameObject pivot; // Temporary pivot for group rotations
    public InventoryManager inventoryManager;

    private readonly Color softYellow = new Color(0.988f, 0.933f, 0.447f, 1f);
    
    /*
    Project a ray forward from the player's viewpoint (a.k.a the screen). This is required for aiming.
    Example: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html
    */
    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    public void OnRotateSelection()
    {
        HandleSelection();
    }

    void HandleSelection()
    {
        if (Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Untagged"))
            {
                return;
            }

            // If it's a Marker, treat its parent as the clicked object
            if (clickedObject.CompareTag("Marker") && clickedObject.transform.parent != null)
            {
                clickedObject = clickedObject.transform.parent.gameObject;
                Debug.Log("Marker clicked, treating as parent: " + clickedObject.name);
            }

            if (clickedObject.CompareTag("Environment"))
            {
                return;
            }

            if (selectedParts.Contains(clickedObject))
            {
                // Deselect parent and its markers
                RestoreColor(clickedObject);
                DeselectMarkers(clickedObject);

                selectedParts.Remove(clickedObject);
                Debug.Log("Deselected: " + clickedObject.name);

                playerSFXController.PlayDeselectPieceSFX();


                RebuildPivot();
            }
            else
            {
                // Select parent
                Highlight(clickedObject, softYellow);
                selectedParts.Add(clickedObject);
                Debug.Log("Selected: " + clickedObject.name);

                // Also highlight its marker children
                SelectMarkers(clickedObject);

                playerSFXController.PlaySelectPieceSFX();

                RebuildPivot();
            }
        }

    }

    // Highlight & mark all child markers
    void SelectMarkers(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag("Marker"))
            {
                Highlight(child.gameObject, softYellow);
                Debug.Log("Also highlighted marker: " + child.name);
            }
        }
    }

    // Restore all child marker colors
    void DeselectMarkers(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag("Marker"))
            {
                RestoreColor(child.gameObject);
                Debug.Log("Deselected marker: " + child.name);
            }
        }
    }

    void _HandleRotation(float x, float y, float z)
    {
        if (selectedParts.Count == 0) return;

        playerSFXController.PlayRotateSFX();

        Transform targetTransform = selectedParts.Count > 0 && pivot != null ? pivot.transform : selectedParts[0].transform;
        targetTransform.Rotate(x, y, z, Space.World);

        Debug.Log($"Rotated {x} on X axis\n{y} on Y axis\n{z} on Z axis");
    }

    public void OnChangePlane(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(0, 0f, 90f);
    }

    public void OnRotateObjectRight(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(0f, 90f, 0f);
    }

    public void OnRotateObjectLeft(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(0f, -90f, 0f);
    }

    void RebuildPivot()
    {
        // Restore all previous parent-child relationships before rebuilding
        if (pivot != null)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in pivot.transform)
            {
                if (child != null)
                    children.Add(child);
            }

            foreach (Transform child in children)
            {
                if (originalParents.ContainsKey(child.gameObject) && originalParents[child.gameObject] != null)
                {
                    child.SetParent(originalParents[child.gameObject], true); // restore
                }
                else
                {
                    child.SetParent(null, true); // fallback
                }
            }

            Destroy(pivot);
            pivot = null;
            originalParents.Clear(); // reset tracking
        }

        // Create a new pivot if any parts are still selected
        if (selectedParts.Count > 0)
        {
            pivot = new GameObject("RotationPivot");

            // Compute the geometric center of selected objects
            Vector3 center = Vector3.zero;
            foreach (var obj in selectedParts)
                center += obj.transform.position;
            center /= selectedParts.Count;

            pivot.transform.position = center;

            // Temporarily parent all selected parts under this pivot
            foreach (var obj in selectedParts)
            {
                if (obj != null)
                {
                    // Save original parent before re-parenting
                    if (!originalParents.ContainsKey(obj))
                    {
                        originalParents[obj] = obj.transform.parent;
                    }

                    obj.transform.SetParent(pivot.transform, true);
                }
            }
        }
    }

    void Highlight(GameObject obj, Color color)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            if (!originalColors.ContainsKey(obj))
                originalColors[obj] = rend.material.color;

            rend.material.color = color;
        }
    }

    void RestoreColor(GameObject obj)
    {
        if (obj != null)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null && originalColors.ContainsKey(obj))
            {
                rend.material.color = originalColors[obj];
            }
        }
    }

    // Deselect everything and ensure the pivot is rebuilt/removed
    public void DeselectAll()
    {
        if (selectedParts.Count > 0)
        {
            foreach (var obj in new List<GameObject>(selectedParts))
            {
                RestoreColor(obj);
                DeselectMarkers(obj);
            }
            selectedParts.Clear();
            originalColors.Clear();
            playerSFXController.PlayDeselectPieceSFX();
        }

        // Always rebuild to ensure any existing pivot is removed
        RebuildPivot();
    }

    public void OnDeletePiece(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        if (selectedParts.Count > 0)
        {
            playerSFXController.PlayDeselectPieceSFX();

            List<GameObject> _piecesToDestroy = selectedParts;

            // Reset state
            selectedParts = new List<GameObject>();
            originalColors.Clear();

            _piecesToDestroy.ForEach(p =>
            {
                Destroy(p);
            });

            // Probably has a bug if we have multiple RotationPivots
            GameObject rotationPivotGO = GameObject.Find("RotationPivot");
            if (rotationPivotGO)
            {
                Destroy(rotationPivotGO);
            }
        }
    }
}
