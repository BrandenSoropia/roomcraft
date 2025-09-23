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
    [SerializeField] private float rotationSpeed = 120f; // deg/s while held

    [Header("Camera Alignment")]
    [SerializeField] private Camera cam;

    private int _index = -1;
    private FurnitureSelectable _current;
    private Rigidbody _currentRB;        // cache RB to avoid GetComponent each frame
    private Vector3 _desiredVelocity;

    private bool _prevNext, _prevPrev;
    private float _rotInput; // -1..1 each frame

    void Start()
    {
        if (cam == null) cam = Camera.main;

        if (furniture == null || furniture.Length == 0)
        {
            furniture = Object.FindObjectsByType<FurnitureSelectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
        }

        // keep only those that actually have a rigidbody
        furniture = System.Array.FindAll(furniture, f => f != null && f.RB != null);

        if (furniture.Length == 0)
        {
            Debug.LogWarning("[IsoFurnitureController] No FurnitureSelectable with Rigidbody found.");
            return;
        }

        SelectIndex(0);
    }

    void Update()
    {
        // planar move
        Vector2 move2 = ReadMove2D();
        Vector3 moveWorld = CameraAlignedMove(move2);
        if (moveWorld.magnitude > maxSpeed) moveWorld = moveWorld.normalized * maxSpeed;
        _desiredVelocity = moveWorld * moveSpeed;

        // continuous rotation
        float rotInput = 0f;
        if (IsRotateLeftHeld()) rotInput -= 1f;
        if (IsRotateRightHeld()) rotInput += 1f;

        _rotInput = 0f;
        if (IsRotateLeftHeld()) _rotInput -= 1f;
        if (IsRotateRightHeld()) _rotInput += 1f;

        // if (_currentRB != null && Mathf.Abs(rotInput) > 0f)
        // {
        //     float deltaRot = rotInput * rotationSpeed * Time.deltaTime;
        //     _currentRB.MoveRotation(Quaternion.Euler(0f, deltaRot, 0f) * _currentRB.rotation);
        // }

        // cycle selection (edge-triggered)
        bool nextSel = IsNextPressed();
        bool prevSel = IsPrevPressed();
        if (nextSel && !_prevNext) Cycle(1);
        if (prevSel && !_prevPrev) Cycle(-1);
        _prevNext = nextSel; _prevPrev = prevSel;
    }

    void FixedUpdate()
    {
        if (_currentRB != null)
        {
            // translate first
            _currentRB.MovePosition(_currentRB.position + _desiredVelocity * Time.fixedDeltaTime);

            // then rotate around the computed pivot
            if (Mathf.Abs(_rotInput) > 0f)
            {
                float delta = _rotInput * rotationSpeed * Time.fixedDeltaTime;
                Quaternion q = Quaternion.Euler(0f, delta, 0f);

                Vector3 pivot = _current.GetWorldRotationPivot();   // bound-based pivot
                Vector3 p0 = _currentRB.position;
                Vector3 p1 = pivot + q * (p0 - pivot);              // new position after rotation about pivot

                _currentRB.MovePosition(p1);
                _currentRB.MoveRotation(q * _currentRB.rotation);
            }
        }

    }
    // ---------- selection ----------
    void SelectIndex(int newIndex)
    {
        if (furniture == null || furniture.Length == 0) return;

        if (_current != null) _current.SetSelected(false);

        _index = Mathf.Clamp(newIndex, 0, furniture.Length - 1);
        _current = furniture[_index];
        _currentRB = _current.RB;

        _current.SetSelected(true);
    }

    void Cycle(int dir)
    {
        int n = furniture.Length;
        int newIdx = ((_index + dir) % n + n) % n;
        SelectIndex(newIdx);
    }

    // ---------- input ----------
    Vector2 ReadMove2D()
    {
        Vector2 k = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) k.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) k.y -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) k.x += 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) k.x -= 1;
        }
        if (Gamepad.current != null)
        {
            var g = Gamepad.current.leftStick.ReadValue();
            if (g.sqrMagnitude > 0.05f) return g;
        }
        return k.sqrMagnitude > 1e-4f ? k.normalized : Vector2.zero;
    }

    bool IsRotateLeftHeld()
    {
        bool key = Keyboard.current != null && Keyboard.current.qKey.isPressed;
        bool shoulder = Gamepad.current != null && Gamepad.current.leftShoulder.isPressed;
        bool trigger = Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.3f;
        return key || shoulder || trigger;
    }

    bool IsRotateRightHeld()
    {
        bool key = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        bool shoulder = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
        bool trigger = Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.3f;
        return key || shoulder || trigger;
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
        Vector3 fwd = cam ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam ? cam.transform.right : Vector3.right;
        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        Vector3 move = right * input.x + fwd * input.y;
        if (move.sqrMagnitude > 1e-4f) move.Normalize();
        return move;
    }
}
