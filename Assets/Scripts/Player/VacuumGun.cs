using UnityEngine;
using UnityEngine.InputSystem;

public class VacuumGun : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference vacuum; // Player/Vacuum

    [Header("Raycast & Firing")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float suckRange = 12f;
    [SerializeField] private Transform muzzleOrHoldPoint;
    [SerializeField] private float shootForce = 20f;

    [Header("Targeting")]
    [SerializeField] private string suckableTag = "Suckable";

    // this stores the thing we grabbed
    private GameObject storedPrefab;

    private void OnEnable()
    {
        if (vacuum != null)
        {
            vacuum.action.performed += OnVacuumPressed;
            vacuum.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (vacuum != null)
        {
            vacuum.action.performed -= OnVacuumPressed;
            vacuum.action.Disable();
        }
    }

    private void OnVacuumPressed(InputAction.CallbackContext _)
    {
        // if we don't have anything yet -> try to suck
        if (storedPrefab == null)
        {
            TrySuck();
        }
        else
        {
            // we already have something -> shoot it
            TryShoot();
        }
    }

    private void TrySuck()
    {
        // raycast from center of screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, suckRange))
        {
            GameObject target = hit.collider.gameObject;

            // only grab valid targets
            if (target.CompareTag(suckableTag))
            {
                storedPrefab = target;

                // hide the original object so it looks "captured"
                storedPrefab.SetActive(false);

                // optional: you could play a suck FX here
            }
        }
    }

    private void TryShoot()
    {
        // safety check
        if (storedPrefab == null) return;

        // spawn a copy at the muzzle / hold point
        GameObject spawned =
            Instantiate(storedPrefab, muzzleOrHoldPoint.position, muzzleOrHoldPoint.rotation);

        spawned.SetActive(true);

        if (spawned.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = muzzleOrHoldPoint.forward * shootForce;
        }

        // we "used up" the stored item
        storedPrefab = null;

        // optional: you could also Destroy() the original hidden object now,
        // or leave it inactive forever, depending on your design:
        // Destroy(storedPrefab); storedPrefab = null;
    }
}
