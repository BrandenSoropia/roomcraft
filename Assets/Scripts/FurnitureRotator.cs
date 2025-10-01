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

    private GameObject pivot; // Temporary pivot for group rotations

    /*
    Project a ray forward from the player's viewpoint (a.k.a the screen). This is required for aiming.
    Example: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html
    */
    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    void Update()
    {
        if (!gameManager.GetIsBuildingEnabled()) return;
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
                Highlight(clickedObject, Color.yellow);
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

    void _HandleRotation(float x, float y, float z)
    {
        if (selectedParts.Count == 0) return;

        playerSFXController.PlayRotateSFX();

        Transform targetTransform = selectedParts.Count > 0 && pivot != null ? pivot.transform : selectedParts[0].transform;
        targetTransform.Rotate(x, y, z, Space.World);

        Debug.Log($"Rotated {x} on X axis\n{y} on Y axis\n{z} on Z axis");
    }


    public void OnRotateObjectUp(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(-90f, 0f, 0f);
    }

    public void OnRotateObjectRight(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(0f, 90f, 0f);
    }

    public void OnRotateObjectDown(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(90f, 0f, 0f);
    }

    public void OnRotateObjectLeft(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        _HandleRotation(0f, -90f, 0f);
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
