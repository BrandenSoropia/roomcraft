using UnityEngine;
using UnityEngine.InputSystem;

public class IsoCamController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private IsoFurnitureController controller;
    [SerializeField] private float radius = 10f;     // orbit distance
    [SerializeField] private float isoPitch = 30f;   // default pitch if not moved
    [SerializeField] private float addHeight = 3f;   // extra vertical offset

    [Header("Orbit Speeds")]
    [SerializeField] private float yawSpeed = 90f;   // deg/sec per stick deflection
    [SerializeField] private float pitchSpeed = 90f; // deg/sec per stick deflection

    [Header("Pitch Limits")]
    [SerializeField] private float minPitch = 10f;   // shallow
    [SerializeField] private float maxPitch = 90f;   // top-down

    [Header("Zoom (orthographic size)")]
    [SerializeField] private float zoomSpeed = 3f;   // units/sec with buttons/keys
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 12f;

    [Header("Smoothing (seconds)")]
    [SerializeField] private float moveSmooth = 0.10f;

    FurnitureSelectable sel;
    float yawDeg;
    float pitchDeg;
    Vector3 vel;
    Camera cam;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        if (!controller) controller = Object.FindFirstObjectByType<IsoFurnitureController>();
#else
        if (!controller) controller = Object.FindObjectOfType<IsoFurnitureController>();
#endif
        if (controller) controller.SelectionChanged += OnSelectionChanged;

        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void OnDestroy()
    {
        if (controller) controller.SelectionChanged -= OnSelectionChanged;
    }

    void Start()
    {
        // start from isoPitch
        pitchDeg = Mathf.Clamp(isoPitch, minPitch, maxPitch);
        sel = controller ? controller.CurrentSelection : null;

        if (sel)
        {
            Vector3 pivot = sel.GetWorldRotationPivot();
            Vector3 rel = transform.position - new Vector3(pivot.x, transform.position.y, pivot.z);
            if (rel.sqrMagnitude > 1e-4f)
                yawDeg = Mathf.Atan2(rel.x, rel.z) * Mathf.Rad2Deg;
        }
    }

    void OnSelectionChanged(FurnitureSelectable s) => sel = s;

    void Update()
    {
        if (!sel && controller) sel = controller.CurrentSelection;
        if (!sel) return;

        // --- Orbit with right stick (yaw/pitch) ---
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.rightStick.ReadValue();
            if (Mathf.Abs(stick.x) > 0.1f)
                yawDeg += stick.x * yawSpeed * Time.deltaTime;
            if (Mathf.Abs(stick.y) > 0.1f)
                pitchDeg += stick.y * pitchSpeed * Time.deltaTime;
        }

        // --- Keyboard backup for orbit: IJKL ---
        // I = pitch up, K = pitch down, J = yaw left, L = yaw right
        if (Keyboard.current != null)
        {
            if (Keyboard.current.jKey.isPressed) yawDeg -= yawSpeed * Time.deltaTime;
            if (Keyboard.current.lKey.isPressed) yawDeg += yawSpeed * Time.deltaTime;
            if (Keyboard.current.iKey.isPressed) pitchDeg += pitchSpeed * Time.deltaTime;
            if (Keyboard.current.kKey.isPressed) pitchDeg -= pitchSpeed * Time.deltaTime;

            // (Optional) keep arrows as additional fallback
            if (Keyboard.current.leftArrowKey.isPressed)  yawDeg -= yawSpeed * Time.deltaTime;
            if (Keyboard.current.rightArrowKey.isPressed) yawDeg += yawSpeed * Time.deltaTime;
        }

        pitchDeg = Mathf.Clamp(pitchDeg, minPitch, maxPitch);

        // --- Zoom with Y/A buttons ---
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonNorth.isPressed) // Y = zoom in
                cam.orthographicSize = Mathf.Max(minZoom, cam.orthographicSize - zoomSpeed * Time.deltaTime);
            if (Gamepad.current.buttonSouth.isPressed) // A = zoom out
                cam.orthographicSize = Mathf.Min(maxZoom, cam.orthographicSize + zoomSpeed * Time.deltaTime);
        }

        // --- Keyboard backup for zoom: R/F ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.isPressed) // zoom in
                cam.orthographicSize = Mathf.Max(minZoom, cam.orthographicSize - zoomSpeed * Time.deltaTime);
            if (Keyboard.current.fKey.isPressed) // zoom out
                cam.orthographicSize = Mathf.Min(maxZoom, cam.orthographicSize + zoomSpeed * Time.deltaTime);

            // (Optional) keep PageUp/PageDown as well
            if (Keyboard.current.pageUpKey.isPressed)
                cam.orthographicSize = Mathf.Max(minZoom, cam.orthographicSize - zoomSpeed * Time.deltaTime);
            if (Keyboard.current.pageDownKey.isPressed)
                cam.orthographicSize = Mathf.Min(maxZoom, cam.orthographicSize + zoomSpeed * Time.deltaTime);
        }

        // --- Position & Rotation ---
        Vector3 pivot = sel.GetWorldRotationPivot();
        Quaternion rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);

        // addHeight is still included, scaled by zoom (so zooming pushes camera vertically a bit)
        float zoom01 = Mathf.InverseLerp(minZoom, maxZoom, cam.orthographicSize);
        Vector3 offset = rot * new Vector3(0f, addHeight * zoom01, -radius);
        Vector3 desiredPos = pivot + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref vel, moveSmooth);
        transform.rotation = rot;
    }
}
