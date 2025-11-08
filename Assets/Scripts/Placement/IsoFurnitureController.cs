using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class IsoFurnitureController : MonoBehaviour
{
    [Header("Discovery")]
    [SerializeField] private FurnitureSelectable[] furniture;

    // Active, movable items we can select/move.
    private List<FurnitureSelectable> activeFurniture = new List<FurnitureSelectable>();

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float maxSpeed = 3.5f;

    [Header("Rotation")]
    [SerializeField] private Camera cam;
    [SerializeField] private float rotationStep = 90f; // each click rotates by 90 degrees

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;
    private InputAction pitchUpAction;
    private InputAction nextAction;
    private InputAction prevAction;

    private int _index = -1;
    private FurnitureSelectable _current;
    private Rigidbody _currentRB;
    private Vector3 _desiredVelocity;

    private bool _prevNext, _prevPrev;
    private bool _prevPitchUpHeld;
    private bool _prevYawLeft, _prevYawRight;

    public FurnitureSelectable CurrentSelection => _current;
    public Transform CurrentSelectionTransform => _current ? _current.transform : null;

    public event System.Action<FurnitureSelectable> SelectionChanged;


    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Discover all with Rigidbody (do NOT filter by IsMovable here so we can subscribe to changes).
        if (furniture == null || furniture.Length == 0)
        {
#if UNITY_2023_1_OR_NEWER
            furniture = Object.FindObjectsByType<FurnitureSelectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            furniture = Object.FindObjectsOfType<FurnitureSelectable>();
#endif
        }
        // Keep only items that exist and have RB; movability handled dynamically.
        furniture = System.Array.FindAll(furniture, f => f != null && f.RB != null);

        foreach (var f in furniture)
        {
            f.MovableChanged -= OnFurnitureMovableChanged;
            f.MovableChanged += OnFurnitureMovableChanged;
        }

        // Build initial active list and select if available.
        RefreshActiveFurniture();

        // Bind actions
        var map = inputActions.FindActionMap("Placement");
        moveAction = map.FindAction("Move");
        rotateLeftAction = map.FindAction("RotateLeft");
        rotateRightAction = map.FindAction("RotateRight");
        nextAction = map.FindAction("Next");
        prevAction = map.FindAction("Prev");
        pitchUpAction = map.FindAction("PitchUp", false);
        map.Enable();

        if (activeFurniture.Count > 0)
            SelectIndex(0);
        else
            Debug.LogWarning("[IsoFurnitureController] No movable FurnitureSelectable found at start.");
    }

    void OnDestroy()
    {
        foreach (var f in furniture)
            if (f != null) f.MovableChanged -= OnFurnitureMovableChanged;
    }

    void Update()
    {
        // --- Movement ---
        Vector2 move2 = moveAction.ReadValue<Vector2>();
        Vector3 moveWorld = CameraAlignedMove(move2);
        if (moveWorld.magnitude > maxSpeed) moveWorld = moveWorld.normalized * maxSpeed;
        _desiredVelocity = moveWorld * moveSpeed;

        // --- Rotation ---
        bool yawLeft = rotateLeftAction.IsPressed();
        bool yawRight = rotateRightAction.IsPressed();
        bool pitchUp = IsPitchUpHeld();

        if (activeFurniture.Count > 0 && _currentRB != null)
        {
            if (yawLeft && !_prevYawLeft) RotateFurniture(0, -rotationStep, 0);
            if (yawRight && !_prevYawRight) RotateFurniture(0, rotationStep, 0);
            if (pitchUp && !_prevPitchUpHeld) RotateFurniture(rotationStep, 0, 0);
        }

        _prevYawLeft = yawLeft;
        _prevYawRight = yawRight;
        _prevPitchUpHeld = pitchUp;

        // --- Selection ---
        bool nextSel = nextAction.IsPressed();
        bool prevSel = prevAction.IsPressed();

        if (activeFurniture.Count > 0)
        {
            if (nextSel && !_prevNext) Cycle(1);
            if (prevSel && !_prevPrev) Cycle(-1);
        }

        _prevNext = nextSel;
        _prevPrev = prevSel;
    }

    void FixedUpdate()
    {
        if (_currentRB != null)
        {
            _currentRB.MovePosition(_currentRB.position + _desiredVelocity * Time.fixedDeltaTime);
        }
    }

    // ---------- Rotation ----------
    void RotateFurniture(float x, float y, float z)
    {
        if (_currentRB == null) return;

        // Calculate pivot from combined bounds center
        Bounds b = GetCombinedBounds(_currentRB.transform);
        Vector3 pivotPoint = b.center;

        _currentRB.transform.RotateAround(pivotPoint, Vector3.right, x);
        _currentRB.transform.RotateAround(pivotPoint, Vector3.up, y);
        _currentRB.transform.RotateAround(pivotPoint, Vector3.forward, z);

        Debug.Log($"Rotated furniture {_currentRB.name} by (x={x}, y={y}, z={z}) around {pivotPoint}");
    }

    Bounds GetCombinedBounds(Transform t)
    {
        Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(t.position, Vector3.zero);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }

    // ---------- Selection ----------
    void SelectIndex(int newIndex)
    {
        if (activeFurniture == null || activeFurniture.Count == 0) return;

        if (_current != null)
            _current.SetSelected(false); // always turn off highlight

        _index = Mathf.Clamp(newIndex, 0, activeFurniture.Count - 1);
        _current = activeFurniture[_index];
        _currentRB = _current.RB;

        // ✅ Only highlight if the assigned camera is active
        if (cam != null && cam.isActiveAndEnabled)
            _current.SetSelected(true);

        SelectionChanged?.Invoke(_current);
    }

    void Cycle(int dir)
    {
        int n = activeFurniture.Count;
        if (n == 0) return;
        int newIdx = ((_index + dir) % n + n) % n;
        SelectIndex(newIdx);
    }

    // ---------- Dynamic movability handling ----------
    void OnFurnitureMovableChanged(FurnitureSelectable f, bool movable)
    {
        RefreshActiveFurniture();
    }

    void RefreshActiveFurniture()
    {
        var prev = _current;

        // Rebuild active list from all discovered items.
        activeFurniture.Clear();
        foreach (var it in furniture)
            if (it != null && it.RB != null && it.IsMovable)
                activeFurniture.Add(it);

        if (activeFurniture.Count == 0)
        {
            if (_current != null) _current.SetSelected(false);
            _current = null;
            _currentRB = null;
            _index = -1;
            SelectionChanged?.Invoke(null);
            return;
        }

        // Keep current if still movable; otherwise pick a valid one.
        if (prev != null && prev.IsMovable)
        {
            int idx = activeFurniture.IndexOf(prev);
            if (idx >= 0)
            {
                _index = idx;
                _current = prev;
                _currentRB = _current.RB;
                if (cam != null && cam.isActiveAndEnabled) _current.SetSelected(true);
                SelectionChanged?.Invoke(_current);
                return;
            }
        }

        SelectIndex(Mathf.Clamp(_index, 0, activeFurniture.Count - 1));
    }

    // ---------- Utilities ----------
    Vector3 CameraAlignedMove(Vector2 input)
    {
        if (cam == null) cam = Camera.main;

        float yaw = cam.transform.eulerAngles.y;
        Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);

        Vector3 fwd = yawOnly * Vector3.forward;
        Vector3 right = yawOnly * Vector3.right;

        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        Vector3 move = right * input.x + fwd * input.y;
        return move.sqrMagnitude > 1e-4f ? move.normalized : Vector3.zero;
    }

    public void SetCameraActive(bool isActive)
    {
        if (_current != null)
            _current.SetSelected(isActive);
    }

    // ---------- Pitch controls ----------
    bool IsPitchUpHeld()
    {
        bool held = pitchUpAction != null && pitchUpAction.IsPressed();
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame) held = true;
        return held;
    }
}

