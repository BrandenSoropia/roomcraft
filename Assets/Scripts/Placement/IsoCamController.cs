using UnityEngine;
using UnityEngine.InputSystem;

public class IsoCamController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private IsoFurnitureController controller;
    [SerializeField] private float radius = 10f;
    [SerializeField] private float isoPitch = 30f;
    [SerializeField] private float addHeight = 3f;

    [Header("Orbit Speeds")]
    [SerializeField] private float yawSpeed = 90f;
    [SerializeField] private float pitchSpeed = 90f;

    [Header("Pitch Limits")]
    [SerializeField] private float minPitch = 10f;
    [SerializeField] private float maxPitch = 90f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 12f;

    [Header("Smoothing")]
    [SerializeField] private float moveSmooth = 0.10f;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    private InputAction orbitAction;
    private InputAction zoomInAction;
    private InputAction zoomOutAction;
    private InputAction orbitKeyboardAction;

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

        // Input actions
        var camMap = inputActions.FindActionMap("Placement");
        orbitAction = camMap.FindAction("Orbit");
        zoomInAction = camMap.FindAction("ZoomIn");
        zoomOutAction = camMap.FindAction("ZoomOut");

        camMap.Enable();
    }

    void OnDestroy()
    {
        if (controller) controller.SelectionChanged -= OnSelectionChanged;
    }

    void Start()
    {
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

        // --- Orbit with right stick or keyboard 2D vector ---
        Vector2 stick = orbitAction.ReadValue<Vector2>();
        Vector2 orbitInput = stick + keys;

        yawDeg += orbitInput.x * yawSpeed * Time.deltaTime;
        pitchDeg += orbitInput.y * pitchSpeed * Time.deltaTime;
        pitchDeg = Mathf.Clamp(pitchDeg, minPitch, maxPitch);

        // --- Zoom ---
        if (zoomInAction.IsPressed())
            cam.orthographicSize = Mathf.Max(minZoom, cam.orthographicSize - zoomSpeed * Time.deltaTime);

        if (zoomOutAction.IsPressed())
            cam.orthographicSize = Mathf.Min(maxZoom, cam.orthographicSize + zoomSpeed * Time.deltaTime);

        // --- Camera Position ---
        Vector3 pivot = sel.GetWorldRotationPivot();
        Quaternion rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);

        float zoom01 = Mathf.InverseLerp(minZoom, maxZoom, cam.orthographicSize);
        Vector3 offset = rot * new Vector3(0f, addHeight * zoom01, -radius);
        Vector3 desiredPos = pivot + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref vel, moveSmooth);
        transform.rotation = rot;
    }
}
