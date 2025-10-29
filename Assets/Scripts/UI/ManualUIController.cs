using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ManualUIController : MonoBehaviour
{
    [Header("Manual Data")]
    [SerializeField] ManualDataSO[] manuals;

    [Header("SFX")]
    [SerializeField] AudioClip navigateSfx;

    AudioSource myAudioSource;

    private GameObject _lastSelected;

    // Actions
    private InputAction _moveAction;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
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


    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

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
