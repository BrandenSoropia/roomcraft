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
    [SerializeField] private float rotationSpeed = 120f;

    [Header("Camera Alignment")]
    [SerializeField] private Camera cam;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;
    private InputAction nextAction;
    private InputAction prevAction;

    private int _index = -1;
    private FurnitureSelectable _current;
    private Rigidbody _currentRB;
    private Vector3 _desiredVelocity;

    private bool _prevNext, _prevPrev;
    private float _rotInput;

    public FurnitureSelectable CurrentSelection => _current;
    public Transform CurrentSelectionTransform => _current ? _current.transform : null;

    public event System.Action<FurnitureSelectable> SelectionChanged;


    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Discover all with Rigidbody (do NOT filter by IsMovable here so we can subscribe to changes).
        if (furniture == null || furniture.Length == 0)
        {
            furniture = Object.FindObjectsByType<FurnitureSelectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
        }
        // Keep only items that exist and have RB; movability handled dynamically.
        furniture = System.Array.FindAll(furniture, f => f != null && f.RB != null);

        // Subscribe to movability changes.
        for (int i = 0; i < furniture.Length; i++)
        {
            furniture[i].MovableChanged -= OnFurnitureMovableChanged; // avoid dupes
            furniture[i].MovableChanged += OnFurnitureMovableChanged;
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

        map.Enable();

        if (activeFurniture.Count > 0)
            SelectIndex(0);
        else
            Debug.LogWarning("[IsoFurnitureController] No movable FurnitureSelectable found at start.");
    }

    void OnDestroy()
    {
        // Unsubscribe from movability changes.
        if (furniture != null)
        {
            for (int i = 0; i < furniture.Length; i++)
            {
                if (furniture[i] != null)
                    furniture[i].MovableChanged -= OnFurnitureMovableChanged;
            }
        }
    }

    void Update()
    {
        // --- Movement ---
        Vector2 move2 = moveAction.ReadValue<Vector2>();
        Vector3 moveWorld = CameraAlignedMove(move2);
        if (moveWorld.magnitude > maxSpeed) moveWorld = moveWorld.normalized * maxSpeed;
        _desiredVelocity = moveWorld * moveSpeed;

        // --- Rotation ---
        _rotInput = 0f;
        if (rotateLeftAction.IsPressed()) _rotInput -= 1f;
        if (rotateRightAction.IsPressed()) _rotInput += 1f;

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

            if (Mathf.Abs(_rotInput) > 0f)
            {
                float delta = _rotInput * rotationSpeed * Time.fixedDeltaTime;
                Quaternion q = Quaternion.Euler(0f, delta, 0f);

                Vector3 pivot = _current.GetWorldRotationPivot();
                Vector3 p0 = _currentRB.position;
                Vector3 p1 = pivot + q * (p0 - pivot);

                _currentRB.MovePosition(p1);
                _currentRB.MoveRotation(q * _currentRB.rotation);
            }
        }
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
        for (int i = 0; i < furniture.Length; i++)
        {
            var it = furniture[i];
            if (it != null && it.RB != null && it.IsMovable)
                activeFurniture.Add(it);
        }

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
}
