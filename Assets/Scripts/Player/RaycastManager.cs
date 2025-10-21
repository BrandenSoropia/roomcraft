// RaycastManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastManager : MonoBehaviour
{
    [SerializeField] float raycastDistance = 200f;
    [SerializeField] LayerMask layerMask;

    FurnitureBuilder myFurnitureBuilder;
    FurnitureRotator myFurnitureRotator;

    void Start()
    {
        myFurnitureBuilder = GetComponent<FurnitureBuilder>();
        myFurnitureRotator = GetComponent<FurnitureRotator>();
    }

    void Update()
    {
        Ray ray = _GetCurrentScreenCenterRay();

        // 🔹 Draw the ray continuously in Scene view
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.green);

        // Perform the raycast each frame (so you can visualize hit point live)
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            // Draw a red line to the exact hit point
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            // Draw a small yellow line showing the surface normal
            Debug.DrawRay(hit.point, hit.normal * 0.25f, Color.yellow);
        }
    }

    public void OnSelect(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        HandleRaycast();
    }

    /*
    Project a ray forward from the player's viewpoint (a.k.a the screen). This is required for aiming.
    Example: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html
    */
    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }


    private void HandleRaycast()
    {
        if (!Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit, Mathf.Infinity, layerMask)) return;
        Debug.Log("Hit: " + hit.transform.name);

        // Maybe move interact controller here? Keeps only 1 raycasting script this way?

        // Raycast hit handlers w/ independent state
        myFurnitureBuilder.OnRaycastHit(hit);
        myFurnitureRotator.OnRaycastHit(hit);
    }
}
