// RaycastManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastManager : MonoBehaviour
{
    FurnitureBuilder myFurnitureBuilder;
    FurnitureRotator myFurnitureRotator;

    void Start()
    {
        myFurnitureBuilder = GetComponent<FurnitureBuilder>();
        myFurnitureRotator = GetComponent<FurnitureRotator>();
    }

    public void OnSelect(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;
        Debug.Log("### Raycast manager OnSelect Called");
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
        if (!Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit)) return;
        Debug.Log("Hit: " + hit.transform.name);

        // Maybe move interact controller here? Keeps only 1 raycasting script this way?

        // Raycast hit handlers w/ independent state
        myFurnitureBuilder.OnRaycastHit(hit);
        myFurnitureRotator.OnRaycastHit(hit);
    }
}
