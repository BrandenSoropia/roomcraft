using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class IsoFurnitureController : MonoBehaviour
{
    [Header("Discovery")]
    [Tooltip("If empty, will auto-find all FurnitureSelectable in the scene.")]
    [SerializeField] private FurnitureSelectable[] furniture;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float maxSpeed = 3.5f; // optional clamp
    [SerializeField] private float rotationSpeed = 120f;

    [Header("Camera Alignment")]
    [Tooltip("Leave empty to use Camera.main")]
    [SerializeField] private Camera cam;

    private int _index = -1;
    private FurnitureSelectable _current;
    private Vector3 _desiredVelocity;
    private float _rotateQueueDeg; // accumulated 45° steps to apply in FixedUpdate

    // edge-detect helpers
    private bool _prevRotateLeft;
    private bool _prevRotateRight;
    private bool _prevNext;
    private bool _prevPrev;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        if (furniture == null || furniture.Length == 0)
        {
            // Include inactive, no sorting (fastest)
            furniture = Object.FindObjectsByType<FurnitureSelectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
        }

        // Filter out any without Rigidbody just in case
        furniture = System.Array.FindAll(furniture, f => f != null && f.GetComponent<Rigidbody>() != null);

        if (furniture.Length > 0)
            SelectIndex(0);
        else
            Debug.LogWarning("No FurnitureSelectable found in scene.");
    }

    void Update()
    {
        // Read movement
        Vector2 move2 = ReadMove2D();
        Vector3 moveWorld = CameraAlignedMove(move2);
        if (moveWorld.magnitude > maxSpeed) moveWorld = moveWorld.normalized * maxSpeed;
        _desiredVelocity = moveWorld * moveSpeed;

        // Continuous rotation
        float rotInput = 0f;
        if (IsRotateLeftPressed()) rotInput -= 1f;
        if (IsRotateRightPressed()) rotInput += 1f;

        if (_current != null && _current.RB != null)
        {
            var rb = _current.RB;
            float deltaRot = rotInput * rotationSpeed * Time.deltaTime;
            rb.MoveRotation(Quaternion.Euler(0f, deltaRot, 0f) * rb.rotation);
        }

        // Selection cycling
        bool nextSel = IsNextPressed();
        bool prevSel = IsPrevPressed();
        if (nextSel && !_prevNext) Cycle(1);
        if (prevSel && !_prevPrev) Cycle(-1);
        _prevNext = nextSel;
        _prevPrev = prevSel;
    }

    void FixedUpdate()
    {
        if (_current == null || _current.RB == null) return;

        var rb = _current.RB;

        // Move using physics for proper collision
        Vector3 targetPos = rb.position + _desiredVelocity * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

    }

    // ======================
    // Selection
    // ======================
    void SelectIndex(int newIndex)
    {
        if (furniture == null || furniture.Length == 0) return;

        if (_current != null) _current.SetSelected(false);

        _index = Mathf.Clamp(newIndex, 0, furniture.Length - 1);
        _current = furniture[_index];

        _current.SetSelected(true);
    }

    void Cycle(int dir)
    {
        if (furniture == null || furniture.Length == 0) return;

        int n = furniture.Length;
        int newIdx = ((_index + dir) % n + n) % n;
        SelectIndex(newIdx);
    }

    // ======================
    // Input helpers
    // ======================
    Vector2 ReadMove2D()
    {
        // Keyboard WASD / Arrow keys
        Vector2 k = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) k.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) k.y -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) k.x += 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) k.x -= 1;
        }

        // Gamepad Left Stick
        if (Gamepad.current != null)
        {
            var g = Gamepad.current.leftStick.ReadValue();
            // If stick magnitude beats keyboard, prefer it; else combine
            if (g.sqrMagnitude > 0.05f) return g;
        }

        return k.normalized;
    }

    bool IsRotateLeftPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.qKey.isPressed;
        bool pad = Gamepad.current != null && Gamepad.current.leftShoulder.isPressed;
        return key || pad;
    }

    bool IsRotateRightPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        bool pad = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
        return key || pad;
    }

    bool IsNextPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.tabKey.isPressed && !Keyboard.current.shiftKey.isPressed;
        bool pad = Gamepad.current != null && Gamepad.current.dpad.right.isPressed;
        return key || pad;
    }

    bool IsPrevPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.tabKey.isPressed && Keyboard.current.shiftKey.isPressed;
        bool pad = Gamepad.current != null && Gamepad.current.dpad.left.isPressed;
        return key || pad;
    }

    Vector3 CameraAlignedMove(Vector2 input)
    {
        if (cam == null) cam = Camera.main;

        // Project camera right/forward onto XZ so WASD is aligned to iso view
        Vector3 fwd = cam ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam ? cam.transform.right : Vector3.right;

        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        Vector3 move = right * input.x + fwd * input.y;
        if (move.sqrMagnitude > 1e-4f) move.Normalize();
        return move;
    }
}
