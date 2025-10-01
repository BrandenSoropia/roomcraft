using UnityEngine;
using System.Collections.Generic;

public class FurnitureRotator : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] GameManager gameManager;

    private List<GameObject> selectedParts = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    private GameObject pivot; // Temporary pivot for group rotations

    public InventoryManager inventoryManager;

    void Update()
    {
        if (!gameManager.GetIsBuildingEnabled()) return;

        // // Hacky way to turn off/on player movement
        // if (selectedParts.Count > 0 && gameManager.GetIsPlayerMovementEnabled())
        // {
        //     gameManager.SetIsPlayerMovementEnabled(false);
        // }
        // else if (selectedParts.Count == 0 && !gameManager.GetIsPlayerMovementEnabled())
        // {
        //     gameManager.SetIsPlayerMovementEnabled(true);

        // }

        HandleSelection();
        HandleRotation();
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

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

                if (clickedObject.CompareTag("FurnitureBox"))
                {
                    FurnitureBox fb = clickedObject.GetComponent<FurnitureBox>();

                    if (fb != null)
                    {
                        inventoryManager.Unbox(fb.items);
                    }

                    Destroy(clickedObject, 1.5f);
                    
                    return;
                }

                if (selectedParts.Contains(clickedObject))
                {
                    // Deselect parent and its markers
                    RestoreColor(clickedObject);
                    DeselectMarkers(clickedObject);

                    selectedParts.Remove(clickedObject);
                    Debug.Log("Deselected: " + clickedObject.name);

                    RebuildPivot();
                }
                else
                {
                    // Select parent
                    Highlight(clickedObject, Color.yellow);
                    selectedParts.Add(clickedObject);
                    Debug.Log("Selected: " + clickedObject.name);

                    // Also highlight its marker children
                    SelectMarkers(clickedObject);

                    RebuildPivot();
                }
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
                Highlight(child.gameObject, Color.yellow);
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

    void HandleRotation()
    {
        if (selectedParts.Count == 0) return;

        Transform targetTransform = selectedParts.Count > 0 && pivot != null ? pivot.transform : selectedParts[0].transform;

        if (Input.GetKeyDown(KeyCode.DownArrow)) // Down
        {
            targetTransform.Rotate(90f, 0f, 0f, Space.World);
            Debug.Log("Rotated +90° on X axis");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Up
        {
            targetTransform.Rotate(-90f, 0f, 0f, Space.World);
            Debug.Log("Rotated -90° on X axis");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) // Left
        {
            targetTransform.Rotate(0f, -90f, 0f, Space.World);
            Debug.Log("Rotated -90° on Y axis");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) // Right
        {
            targetTransform.Rotate(0f, 90f, 0f, Space.World);
            Debug.Log("Rotated +90° on Y axis");
        }
    }

    void RebuildPivot()
    {
        // If pivot exists, unparent its children safely
        if (pivot != null)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in pivot.transform)
            {
                if (child != null)
                    children.Add(child);
            }

            foreach (Transform child in children)
                child.SetParent(null, true);

            Destroy(pivot);
            pivot = null;
        }

        // Create new pivot only if more than 0 object selected
        if (selectedParts.Count > 0)
        {
            pivot = new GameObject("RotationPivot");

            Vector3 center = Vector3.zero;
            foreach (var obj in selectedParts)
                center += obj.transform.position;
            center /= selectedParts.Count;

            pivot.transform.position = center;

            foreach (var obj in selectedParts)
            {
                if (obj != null)
                    obj.transform.SetParent(pivot.transform, true);
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
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null && originalColors.ContainsKey(obj))
        {
            rend.material.color = originalColors[obj];
        }
    }
}
