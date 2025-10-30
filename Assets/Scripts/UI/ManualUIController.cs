using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ManualUIController : MonoBehaviour
{
    [SerializeField] Vector3 offscreenOffset = new Vector3(0, 0, 0);

    [Header("Internal State")]
    [SerializeField] bool _isDisplayed = false;
    [SerializeField] int _currentManualIdx = 0;
    [SerializeField] int _currentPageIdx = 0;

    [Header("Manual Data")]
    [SerializeField] ManualDataSO[] manuals;

    [Header("SFX")]
    [SerializeField] AudioClip navigateSfx;

    AudioSource myAudioSource;

    // Actions
    private InputAction _moveAction;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();

        // transform.position = offscreenOffset;
    }


    private void OnEnable()
    {
        /*
        Input System UI Input Module doesn't work like Player Input's event handler with its "On..." handlers.

        We have to manually subscribe to UI Input's events like below. Each control will have a matching name
        from the ones seen in the "Input System UI Input Module" component on the EventSystem.
        */
        var uiInput = EventSystem.current?.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        if (uiInput == null)
        {
            Debug.LogWarning("No InputSystemUIInputModule found on current EventSystem.");
            return;
        }

        if (uiInput != null)
        {
            if (uiInput.move != null)
            {
                Debug.Log("onMove attached");
                _moveAction = uiInput.move;
                _moveAction.performed += OnMove; // This gives you your own OnMove handler
            }
        }
    }

    private void OnDisable()
    {
        if (_moveAction != null)
            _moveAction.performed -= OnMove;
    }

    public void ToggleDisplay()
    {
        if (_isDisplayed)
        {
            transform.position = -offscreenOffset;
            _isDisplayed = false;
        }
        else
        {
            transform.position = Vector3.zero;
            _isDisplayed = true;
        }
    }

    void HandleChangePage(Vector2 value)
    {
        if (value.y > 0)
            Debug.Log("Navigate Up");
        else if (value.y < 0)
            Debug.Log("Navigate Down");
    }

    void HandleChangeManual(Vector2 value)
    {
        if (value.x > 0)
            Debug.Log("Navigate Right");
        else if (value.x < 0)
            Debug.Log("Navigate Left");
    }



    private void OnMove(InputAction.CallbackContext ctx)
    {
        bool fromDpad = ctx.control is DpadControl;
        bool fromArrowKey = ctx.control is KeyControl key &&
                (key.keyCode == Key.UpArrow ||
                 key.keyCode == Key.DownArrow ||
                 key.keyCode == Key.LeftArrow ||
                 key.keyCode == Key.RightArrow);
        bool isValidControlPressed = fromDpad || fromArrowKey;

        // Only handle movement when displayed
        if (!_isDisplayed || !ctx.performed || !isValidControlPressed) return;

        Vector2 value = ctx.ReadValue<Vector2>();


        // Check if d-pad up/down page idx +/- 1
        if (value.x != 0)
        {
            HandleChangeManual(value);
        }
        else
        {
            HandleChangePage(value);

        }
    }
}
