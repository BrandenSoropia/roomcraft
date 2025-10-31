using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    [Header("Scaling")]
    [SerializeField, Range(0.05f, 1f)] private float spawnScaleFactor = 0.25f;
    [SerializeField, Range(0.05f, 0.6f)] private float popDuration = 0.18f;
    [SerializeField] private AnimationCurve popCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

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
        if (storedPrefab == null) return;

        // Spawn at muzzle
        GameObject spawned = Instantiate(
            storedPrefab, muzzleOrHoldPoint.position, muzzleOrHoldPoint.rotation
        );
        spawned.SetActive(true);

        // --- POP SCALE SETUP ---
        // Cache its "real" scale, then set it small to start
        Vector3 finalScale = spawned.transform.localScale;
        spawned.transform.localScale = finalScale * spawnScaleFactor;

        // OPTIONAL: briefly ignore collisions to avoid clipping on spawn
        // (especially if muzzle is near a wall or the player)
        var rb = spawned.GetComponent<Rigidbody>();
        var col = spawned.GetComponent<Collider>();
        if (col) col.enabled = false;

        // Launch
        if (rb != null)
        {
            // Use rb.velocity for built-in Rigidbody (PhysX).
            // If you're using Unity Physics (DOTS) you'd use linearVelocity on a different component.
            rb.linearVelocity = muzzleOrHoldPoint.forward * shootForce;
        }

        // Re-enable collider after a short delay (so the pop doesn’t explode on contact)
        StartCoroutine(ReenableColliderNextFrame(col));

        // Animate scale up
        StartCoroutine(PopScale(spawned.transform, finalScale, popDuration, popCurve));

        // Clear stored
        storedPrefab = null;
    }

    private IEnumerator PopScale(Transform t, Vector3 finalScale, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        Vector3 start = t.localScale;
        // Optional: slight overshoot for a juicy feel (1.08x)
        Vector3 overshoot = finalScale * 1.08f;

        // Phase 1: grow to overshoot (60% of time)
        float t1 = duration * 0.6f;
        while (elapsed < t1)
        {
            float p = Mathf.Clamp01(elapsed / t1);
            // use curve for easing
            t.localScale = Vector3.LerpUnclamped(start, overshoot, curve.Evaluate(p));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: settle back to final (remaining 40%)
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
        // wait a few frames so we’re safely out of the muzzle/player
        yield return null;
        yield return null;
        c.enabled = true;
    }
}
