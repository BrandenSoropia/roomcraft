using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;
using System;

public class MenuSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform highlightWood;  // the moving wood image
    [SerializeField] private List<Button> buttons = new List<Button>(); // buttons in order: top → bottom
    [SerializeField] private float moveSpeed = 8f;

    private int currentIndex = 0;
    private RectTransform targetRect;

    // Needed to listen to controller events
    InputSystemUIInputModule _uiModule;
    // References to remove listeners
    InputAction _moveAction;
    bool upTriggered = false;
    bool downTriggered = false;
    [SerializeField, Range(0, 1)] float moveSensitivity = 0.9f;
    InputAction _submitAction;

    void Start()
    {
        if (buttons.Count == 0 || highlightWood == null)
        {
            Debug.LogError("[MenuSelector] Missing buttons or highlightWood reference.");
            return;
        }

        // Initialize
        currentIndex = 0;
        targetRect = buttons[currentIndex].GetComponent<RectTransform>();
        highlightWood.position = targetRect.position;

        // Make sure an EventSystem exists
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
    }

    void OnEnable()
    {
        /*
        Input System UI Input Module doesn't work like Player Input's event handler with its "On..." handlers.

        We have to manually subscribe to UI Input's events like below. Each control will have a matching name
        from the ones seen in the "Input System UI Input Module" component on the EventSystem.
        */
        _uiModule = EventSystem.current?.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        if (_uiModule == null)
        {
            Debug.LogWarning("No InputSystemUIInputModule found on current EventSystem.");
            return;
        }

        if (_uiModule != null)
        {
            // Note: can't use ".started" since UI / Move is config to pass-through while ".started" is only for buttons
            if (_uiModule.move != null)
            {
                Debug.Log("MenuSelector: onMove attached");
                _moveAction = _uiModule.move;
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_uiModule.submit != null)
            {
                Debug.Log("MenuSelector: onSubmit attached");
                _submitAction = _uiModule.submit;
                _submitAction.performed += OnSubmitPerformed;
            }

        }
    }

    void OnDisable()
    {
        if (_moveAction != null)
            _moveAction.performed -= OnMovePerformed;

        if (_submitAction != null)
            _submitAction.performed -= OnSubmitPerformed;
    }

    void Update()
    {
        if (buttons.Count == 0 || highlightWood == null) return;

        // Smoothly move the wood towards the target button
        if (targetRect)
        {
            highlightWood.position = Vector3.Lerp(
                highlightWood.position,
                targetRect.position,
                Time.deltaTime * moveSpeed
            );
        }

        // Keyboard navigation
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }

        // Confirm
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            buttons[currentIndex].onClick.Invoke();
        }
    }

    private void MoveSelection(int direction)
    {
        int newIndex = Mathf.Clamp(currentIndex + direction, 0, buttons.Count - 1);
        if (newIndex == currentIndex) return;

        currentIndex = newIndex;
        targetRect = buttons[currentIndex].GetComponent<RectTransform>();
        EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
    }

    // CONTROLLER SUPPORT
    void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        buttons[currentIndex].onClick.Invoke();
    }

    // Uses up/down triggered flags to make this handle the movement "once" until the move stick returns to 0
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 value = ctx.ReadValue<Vector2>();

        if (value.y > moveSensitivity && !upTriggered)
        {
            upTriggered = true;
            downTriggered = false;

            MoveSelection(-1);
        }
        else if (value.y < -moveSensitivity && !downTriggered)
        {
            upTriggered = false;
            downTriggered = true;
            MoveSelection(1);
        }
        else if (value.y == 0)
        {
            // reset once the stick is brought back to 0
            upTriggered = false;
            downTriggered = false;
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        upTriggered = false;
        downTriggered = false;
    }
}
