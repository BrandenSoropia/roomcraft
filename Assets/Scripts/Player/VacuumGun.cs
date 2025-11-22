using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class VacuumGun : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference buildButton;     // A button — suck / build / place
    [SerializeField] private InputActionReference throwTrigger;    // Right trigger — charge & throw

    [Header("References")]
    [SerializeField] public Camera playerCamera;
    [SerializeField] private FurnitureBuilder furnitureBuilder;
    [SerializeField] private SelectedPieceUIController selectedPieceUIController;
    [SerializeField] private PlayerSFXController playerSFXController;
    [SerializeField] InteractUIController interactUIController;

    [Header("UI")]
    [SerializeField] private Image throwPowerBar;   // optional power visualization (0–1 fill)
    
    [SerializeField] private GameObject powerBarContainer; // optional container to enable/disable

    [Header("Physics / Spawn")]
    [SerializeField] private Transform muzzleOrHoldPoint;
    [SerializeField] private float suckRange = 12f;
    [SerializeField] private float dropDistance = 1.5f;

    [Header("Throw Settings")]
    [SerializeField] private float maxThrowForce = 30f;
    [SerializeField] private float minThrowForce = 3f;
    [SerializeField] private float chargeSpeed = 40f;     // % per second at full trigger
    [SerializeField] private float dropThresholdPercent = 10f;

    [Header("Tags")]
    [SerializeField] private string suckableTag = "Suckable";

    [Header("Scaling FX")]
    [SerializeField, Range(0.05f, 1f)] private float spawnScaleFactor = 0.25f;
    [SerializeField, Range(0.05f, 0.6f)] private float popDuration = 0.18f;
    [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // --- State ---
    private GameObject storedPrefab;
    public bool isHoldingItem;
    private GameObject previewInstance;
    private bool isHoldingPreview = false;

    // --- Throw tracking ---
    public float throwPercent = 0f;      // 0–100 %
    private bool isChargingThrow = false;
    private bool wasTriggerPressed = false;

    private void OnEnable()
    {
        buildButton.action.performed += OnBuildButtonPressed;
        buildButton.action.Enable();

        throwTrigger.action.Enable();  // We’ll poll manually
    }

    private void OnDisable()
    {
        buildButton.action.performed -= OnBuildButtonPressed;
        buildButton.action.Disable();

        throwTrigger.action.Disable();
    }

    private void Update()
    {
        float triggerValue = throwTrigger.action.ReadValue<float>(); // always 0–1
        bool isPressed = triggerValue > 0.05f;

        // --- Start charging ---
        if (!wasTriggerPressed && isPressed && storedPrefab != null)
        {
            isChargingThrow = true;
            throwPercent = 0f;
        }

        // --- Charging ---
        if (isChargingThrow && storedPrefab != null)
        {
            float chargeRate = chargeSpeed * triggerValue * Time.deltaTime;
            throwPercent = Mathf.Clamp(throwPercent + chargeRate, 0f, 100f);

            if (powerBarContainer != null)
                powerBarContainer.SetActive(true);
            if (throwPowerBar != null)
                throwPowerBar.fillAmount = throwPercent / 100f;
        }

        // --- Release detected ---
        if (wasTriggerPressed && !isPressed)
        {
            if (isChargingThrow)
                ReleaseThrow(triggerValue);
            isChargingThrow = false;
            if (powerBarContainer != null)
                powerBarContainer.SetActive(false);
        }

        wasTriggerPressed = isPressed;
    }

    // ==========================================================
    //  A BUTTON — SUCK / PREVIEW / PLACE
    // ==========================================================
    private void OnBuildButtonPressed(InputAction.CallbackContext ctx)
    {
        if (storedPrefab == null)
        {
            TrySuck();
            return;
        }

        if (!isHoldingPreview)
        {
            previewInstance = Instantiate(storedPrefab, new Vector3(0, 0, -10), Quaternion.identity);
            previewInstance.SetActive(true);
            furnitureBuilder.PrepareNewPiece(previewInstance);
            isHoldingPreview = true;
            Debug.Log("Prepared preview for placement.");
        }

        bool placed = furnitureBuilder.PlacePendingPiece();
        if (placed)
        {
            isHoldingItem = false;

            storedPrefab = null;
            isHoldingPreview = false;
            previewInstance = null;
            selectedPieceUIController.ClearSelectedPieceImage();
            interactUIController.ShowPickUpText();

            Debug.Log("✅ Placed furniture.");
        }
        else
        {
            Debug.Log("❌ Invalid marker — cannot place.");
        }
    }

    // ==========================================================
    //  THROW SYSTEM
    // ==========================================================
    private void ReleaseThrow(float finalValue)
    {
        if (storedPrefab == null) return;

        // Cancel any preview
        if (isHoldingPreview && previewInstance != null)
        {
            Destroy(previewInstance);
            furnitureBuilder.DeselectExternally();
            isHoldingPreview = false;
            previewInstance = null;
        }

        Debug.Log($"Releasing throw at {throwPercent:0.0}% power (finalValue={finalValue:0.00})");

        // Drop if below threshold
        if (throwPercent < dropThresholdPercent)
        {
            DropItem();
        }
        else
        {
            float t = throwPercent / 100f;
            float force = Mathf.Lerp(minThrowForce, maxThrowForce, t);
            Debug.Log($"Throw percent: {throwPercent:0.0}%, finalValue: {finalValue:0.00}, computed force: {force:0.0}");
            ThrowItem(force);
        }

        if (throwPowerBar != null)
            throwPowerBar.fillAmount = 0f;

        selectedPieceUIController.ClearSelectedPieceImage();
        interactUIController.ShowPickUpText();

        furnitureBuilder.DeselectExternally();

        storedPrefab = null;
        throwPercent = 0f;
    }

    // ==========================================================
    //  SUCK LOGIC
    // ==========================================================
    private void TrySuck()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, suckRange)) return;

        GameObject target = hit.collider.gameObject;
        if (!target.CompareTag(suckableTag) && target.transform.parent != null && target.transform.parent.CompareTag(suckableTag))
            target = target.transform.parent.gameObject;

        if (!target.CompareTag(suckableTag)) return;

        isHoldingItem = true;

        storedPrefab = target;
        storedPrefab.SetActive(false);

        selectedPieceUIController.SetSelectedPlaceholderPiece();
        interactUIController.ShowBuildText();

        var rb = storedPrefab.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        furnitureBuilder.SelectExternally(storedPrefab);
        Debug.Log("🌀 Sucked up: " + storedPrefab.name);
    }

    // ==========================================================
    //  DROP & THROW IMPLEMENTATIONS
    // ==========================================================
    private void DropItem()
    {
        GameObject dropped = Instantiate(storedPrefab, muzzleOrHoldPoint.position + muzzleOrHoldPoint.forward * dropDistance, Quaternion.identity);
        dropped.SetActive(true);

        var rb = dropped.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("🪶 Dropped item lightly.");

        isHoldingItem = false;
    }

    private void ThrowItem(float force)
    {
        GameObject thrown = Instantiate(storedPrefab, muzzleOrHoldPoint.position, muzzleOrHoldPoint.rotation);
        thrown.SetActive(true);

        Vector3 finalScale = thrown.transform.localScale;
        thrown.transform.localScale = finalScale * spawnScaleFactor;

        var rb = thrown.GetComponent<Rigidbody>();
        var col = thrown.GetComponent<Collider>();
        if (col) col.enabled = false;

        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = muzzleOrHoldPoint.forward * force;
        }

        StartCoroutine(ReenableColliderNextFrame(col));
        StartCoroutine(PopScale(thrown.transform, finalScale, popDuration, popCurve));

        Debug.Log($"💥 Threw item with {throwPercent:0}% power ({force:0.0}).");

        isHoldingItem = false;
    }

    // ==========================================================
    //  UTILITIES
    // ==========================================================
    private IEnumerator ReenableColliderNextFrame(Collider c)
    {
        if (c == null) yield break;
        yield return null;
        yield return null;
        c.enabled = true;
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
    
    public GameObject GetHeldPreview()
    {
        // If the player is holding a preview, return the preview itself
        if (isHoldingPreview && previewInstance != null)
            return previewInstance;

        // If sucking but preview has not been spawned yet, return the prefab as key info
        if (isHoldingItem && storedPrefab != null)
            return storedPrefab;

        return null;
    }
}
