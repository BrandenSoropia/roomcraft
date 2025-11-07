using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class VacuumGun : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference vacuum; // main trigger (suck/shoot)
    [SerializeField] private InputActionReference place;  // new place button

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FurnitureBuilder furnitureBuilder;

    [Header("Raycast & Firing")]
    [SerializeField] private float suckRange = 12f;
    [SerializeField] private Transform muzzleOrHoldPoint;
    [SerializeField] private float shootForce = 20f;

    [Header("Tags")]
    [SerializeField] private string suckableTag = "Suckable";

    [Header("Scaling")]
    [SerializeField, Range(0.05f, 1f)] private float spawnScaleFactor = 0.25f;
    [SerializeField, Range(0.05f, 0.6f)] private float popDuration = 0.18f;
    [SerializeField]
    private AnimationCurve popCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI")]
    [SerializeField] SelectedPieceUIController selectedPieceUIController;

    [Header("Player SFX")]
    [SerializeField] PlayerSFXController playerSFXController;

    // currently held prefab reference
    public GameObject storedPrefab;
    // a cached copy of the original prefab before deactivation
    private GameObject originalPrefab;

    private bool isHoldingPiece = false;
    private GameObject previewInstance;

    private void OnEnable()
    {
        vacuum.action.performed += OnVacuumPressed;
        vacuum.action.Enable();

        place.action.performed += OnPlacePressed;
        place.action.Enable();
    }

    private void OnDisable()
    {
        vacuum.action.performed -= OnVacuumPressed;
        vacuum.action.Disable();

        place.action.performed -= OnPlacePressed;
        place.action.Disable();
    }

    private void OnVacuumPressed(InputAction.CallbackContext _)
    {
        if (storedPrefab == null)
        {
            TrySuck();
        }
        else
        {
            TryShoot();
        }
    }

    private void OnPlacePressed(InputAction.CallbackContext _)
    {
        if (storedPrefab == null) return;

        // Step 1: spawn preview if not holding
        if (!isHoldingPiece)
        {
            previewInstance = Instantiate(storedPrefab, new Vector3(0, -10, 0), Quaternion.identity);
            previewInstance.SetActive(true);
            furnitureBuilder.PrepareNewPiece(previewInstance);
            isHoldingPiece = true;
            Debug.Log("Prepared new piece at Z=-10 for building");
        }

        // Step 2: Try to place the preview
        bool placed = furnitureBuilder.PlacePendingPiece();

        if (placed)
        {
            Debug.Log("Piece placed successfully");
            storedPrefab = null;          // clear stored reference
            previewInstance = null;       // clear preview
            isHoldingPiece = false;

            selectedPieceUIController.ClearSelectedPieceImage();
        }
        else
        {
            Debug.Log("No valid marker — placement failed");
        }
    }

    private void TrySuck()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, suckRange))
        {
            GameObject target = hit.collider.gameObject;
            
            if (!target.CompareTag(suckableTag) && target.transform.parent != null && target.transform.parent.CompareTag(suckableTag)) 
            {
                target = target.transform.parent.gameObject;
            }

            if (target.CompareTag(suckableTag))
            {
                // keep a reference to the original prefab
                originalPrefab = target;

                // hide the original, but don't destroy it
                originalPrefab.SetActive(false);

                // store for later instantiation
                storedPrefab = originalPrefab;

                selectedPieceUIController.SetSelectedPlaceholderPiece();

                // stop motion & disable physics while "held"
                var rb = storedPrefab.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // visually mark as selected in the builder
                furnitureBuilder.SelectExternally(storedPrefab);

                Debug.Log("🌀 Sucked up object: " + storedPrefab.name);
            }
        }
    }

    private void TryShoot()
    {
        // If preview is active (unplaced), delete it
        if (isHoldingPiece && previewInstance != null)
        {
            Debug.Log("Cancelling placement preview and shooting instead");
            Destroy(previewInstance);
            furnitureBuilder.DeselectExternally();
            isHoldingPiece = false;
            previewInstance = null;
        }

        if (storedPrefab == null) return;

        playerSFXController.PlayThrowSFX();

        GameObject spawned = Instantiate(storedPrefab, muzzleOrHoldPoint.position, muzzleOrHoldPoint.rotation);
        spawned.SetActive(true);

        Vector3 finalScale = spawned.transform.localScale;
        spawned.transform.localScale = finalScale * spawnScaleFactor;

        var rb = spawned.GetComponent<Rigidbody>();
        var col = spawned.GetComponent<Collider>();
        if (col) col.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = muzzleOrHoldPoint.forward * shootForce;
        }

        StartCoroutine(ReenableColliderNextFrame(col));
        StartCoroutine(PopScale(spawned.transform, finalScale, popDuration, popCurve));

        furnitureBuilder.DeselectExternally();

        storedPrefab = null;

        selectedPieceUIController.ClearSelectedPieceImage();
    }

    private IEnumerator PopScale(Transform t, Vector3 finalScale, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        Vector3 start = t.localScale;
        Vector3 overshoot = finalScale * 1.08f;
        float t1 = duration * 0.6f;
        while (elapsed < t1)
        {
            float p = Mathf.Clamp01(elapsed / t1);
            t.localScale = Vector3.LerpUnclamped(start, overshoot, curve.Evaluate(p));
            elapsed += Time.deltaTime;
            yield return null;
        }
        float t2 = duration - t1;
        elapsed = 0f;
        while (elapsed < t2)
        {
            float p = Mathf.Clamp01(elapsed / t2);
            t.localScale = Vector3.LerpUnclamped(overshoot, finalScale, curve.Evaluate(p));
            elapsed += Time.deltaTime;
            yield return null;
        }
        t.localScale = finalScale;
    }

    private IEnumerator ReenableColliderNextFrame(Collider c)
    {
        if (c == null) yield break;
        yield return null;
        yield return null;
        c.enabled = true;
    }
}
