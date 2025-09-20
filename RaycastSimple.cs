using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

/*
By default, raycasting returns the closest hit to origin!

Requirements:
- Apply this script to Camera
- Using first person controller from unity, add toggle to show mouse to support both first person and mouse raycasting 
- Attach reference to Player GO's StarterAssetsInputs so raycasting can toggle between camera facing or mouse position
*/
public class RaycastSimple : MonoBehaviour
{

    [Header("StarterAssetsInputs Player Source")]
    [SerializeField] StarterAssetsInputs _input;

    [Header("Raycast")]
    [SerializeField] float raycastDistance = 50f;

    [Range(0f, 1f)]
    [SerializeField] float raycastDrawAlpha = 0.75f;

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;
    Renderer _targetRenderer;
    Material _targetOriginalMaterial;


    // See Order of Execution for Event Functions for information on FixedUpdate() and Update() related to physics queries
    void FixedUpdate()
    {
        RaycastFurniture();
    }

    Ray _GetRaycastingMode()
    {
        Ray directionRay = new Ray(transform.position, transform.forward);

        // Magic that generates a ray in the center of the camera and move according to mouse position. Good for click support.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        return _input.cursorLocked ? directionRay : mouseRay;
    }

    void RaycastFurniture()
    {

        Ray ray = _GetRaycastingMode();

        RaycastHit hit;

        // Limit collision detection to Furniture tagged GameObjects only
        if (Physics.Raycast(ray, out hit, raycastDistance, LayerMask.GetMask("Furniture")))
        {
            // Debug.Log("Hit: " + hit.transform.name);

            _targetRenderer = hit.transform.GetComponent<Renderer>();

            SaveTargetOriginalState(_targetRenderer);
            HighlightTarget(_targetRenderer);

        }
        else
        {
            ResetTarget();
        }
    }

    void SaveTargetOriginalState(Renderer r)
    {
        Material targetMaterial = r.material;

        // Save original material
        if (_targetOriginalMaterial == null)
        {
            _targetOriginalMaterial = targetMaterial;
        }
    }

    /*
    Source: https://discussions.unity.com/t/how-do-you-highlight-an-object-pointed-to-by-a-raycast-unity-3d/189617
    */
    void HighlightTarget(Renderer r)
    {
        // Apply highlight material 
        if (r.material != highlightMaterial)
        {
            r.material = highlightMaterial;
        }
    }

    /*
    Reset target's original state when not hit by raycast, only if we have something saved
    */
    void ResetTarget()
    {
        if (_targetRenderer)
        {
            _targetRenderer.material = _targetOriginalMaterial;

            _targetRenderer = null;
            _targetOriginalMaterial = null;
        }
    }

    /*
    Draw line to see raycast when Gizmos button is turned on.
    */
    private void OnDrawGizmos()
    {
        Vector3 endPoint = transform.TransformDirection(transform.position + (Vector3.forward * raycastDistance));

        // Set the color with custom alpha
        Gizmos.color = new Color(1f, 1f, 0f, raycastDrawAlpha); // Yellow with custom alpha

        // Draw the line
        Gizmos.DrawLine(transform.position, endPoint);

        // Draw spheres at start and end points
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawSphere(endPoint, 0.1f);

        // Calculate and display the midpoint
        Vector3 midpoint = (transform.position + endPoint) / 2f;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(midpoint, 0.15f);

        // Display the distance
        float distance = Vector3.Distance(transform.position, endPoint);
        UnityEditor.Handles.Label(midpoint, $"Distance: {distance:F2}");
    }
}