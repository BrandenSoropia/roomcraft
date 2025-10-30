using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ManualUIController : MonoBehaviour
{
    [SerializeField] Vector3 offscreenOffset = new Vector3(0, 0, 0);

    [Header("Manual Data")]
    [SerializeField] ManualDataSO[] manuals;

    [Header("SFX")]
    [SerializeField] AudioClip navigateSfx;

    AudioSource myAudioSource;

    private GameObject _lastSelected;
    private bool _isDisplayed = false;

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
        if (uiInput != null)
        {
            if (uiInput.move != null)
            {
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


    private void OnMove(InputAction.CallbackContext ctx)
    {
        // Only handle movement when displayed
        if (!_isDisplayed || !ctx.performed) return;

        // Check if d-pad up/down page idx +/- 1
        // Bumpers to change manuals idx +/- 

        var current = EventSystem.current.currentSelectedGameObject;

        // If selection changed
        if (current != null && current != _lastSelected)
        {
            _lastSelected = current;
            myAudioSource.PlayOneShot(navigateSfx);
        }
    }
}
