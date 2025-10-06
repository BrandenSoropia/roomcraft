using UnityEngine;
using UnityEngine.InputSystem;

public class IsoFurnitureController : MonoBehaviour
{
    [Header("Discovery")]
    [SerializeField] private FurnitureSelectable[] furniture;

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

        // Filter furniture with Rigidbody
        if (furniture == null || furniture.Length == 0)
        {
            furniture = Object.FindObjectsByType<FurnitureSelectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
        }
        furniture = System.Array.FindAll(furniture, f => f != null && f.RB != null);
        if (furniture.Length == 0)
        {
            Debug.LogWarning("[IsoFurnitureController] No FurnitureSelectable with Rigidbody found.");
            return;
        }

        // Bind actions
        var map = inputActions.FindActionMap("Furniture");
        moveAction = map.FindAction("Move");
        rotateLeftAction = map.FindAction("RotateLeft");
        rotateRightAction = map.FindAction("RotateRight");
        nextAction = map.FindAction("Next");
        prevAction = map.FindAction("Prev");

        map.Enable();
        SelectIndex(0);
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

        if (nextSel && !_prevNext) Cycle(1);
        if (prevSel && !_prevPrev) Cycle(-1);

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
        if (furniture == null || furniture.Length == 0) return;

        if (_current != null) _current.SetSelected(false);

        _index = Mathf.Clamp(newIndex, 0, furniture.Length - 1);
        _current = furniture[_index];
        _currentRB = _current.RB;

        _current.SetSelected(true);
        SelectionChanged?.Invoke(_current);
    }

    void Cycle(int dir)
    {
        int n = furniture.Length;
        int newIdx = ((_index + dir) % n + n) % n;
        SelectIndex(newIdx);
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
}
