using UnityEngine;
using UnityEngine.InputSystem;

public class VacuumGun : MonoBehaviour
{
    [Header("Input (drag from .inputactions)")]
    [SerializeField] private InputActionReference suck;   // Player/Suck
    [SerializeField] private InputActionReference shoot;  // Player/Shoot

    [Header("Raycast & Firing")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float suckRange = 12f;
    [SerializeField] private Transform muzzleOrHoldPoint;
    [SerializeField] private float shootForce = 20f;

    [Header("Targeting")]
    [SerializeField] private string suckableTag = "Suckable"; // tag your pieces

    private GameObject storedPrefab; // what we "sucked" (simple version)

    private void OnEnable()
    {
        if (suck != null)
        {
            suck.action.performed += OnSuck;
            suck.action.Enable();
        }
        if (shoot != null)
        {
            shoot.action.performed += OnShoot;
            shoot.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (suck != null)
        {
            suck.action.performed -= OnSuck;
            suck.action.Disable();
        }
        if (shoot != null)
        {
            shoot.action.performed -= OnShoot;
            shoot.action.Disable();
        }
    }

    private void OnSuck(InputAction.CallbackContext _)
    {
        // add console message
        Debug.Log("Attempting to suck...");
        if (storedPrefab != null) return; // already have one stored

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, suckRange))
        {
            var go = hit.collider.gameObject;
            if (go.CompareTag(suckableTag))
            {
                // simplest: "store" it and hide the original
                storedPrefab = go;
                go.SetActive(false);
                // add console message
                Debug.Log("Sucked: " + go.name);
            }
        }
    }

    private void OnShoot(InputAction.CallbackContext _)
    {
        if (storedPrefab == null) return;

        // Recreate/launch it from the muzzle
        GameObject spawned = Instantiate(storedPrefab, muzzleOrHoldPoint.position, muzzleOrHoldPoint.rotation);
        spawned.SetActive(true);

        if (spawned.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = muzzleOrHoldPoint.forward * shootForce;

            // add console message
        Debug.Log("Shot: " + spawned.name);

        storedPrefab = null;
    }
}
