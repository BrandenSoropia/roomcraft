using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using System.Linq;

public class ManualUIController : MonoBehaviour
{
    [SerializeField] Vector3 onScreenPosition = new Vector3(0, 0, 0);
    [SerializeField] Vector3 offScreenPosition = new Vector3(0, 0, 0);


    [Header("Internal State")]
    [SerializeField] bool _isDisplayed = false;
    [SerializeField] int _currentManualIdx = 0;
    [SerializeField] int _currentPageIdx = 0;
    [SerializeField] ManualDataSO _currentManual;

    [Header("Manual Configs")]
    [SerializeField] Image myManualImageGO;
    [SerializeField] ManualDataSO[] manuals;

    [Header("Page UI")]
    [SerializeField] Color activeStepColor;
    [SerializeField] Color inactiveStepColor;
    [SerializeField] GameObject myPageControlsContainer;
    [SerializeField] GameObject myPageIndicator;
    [SerializeField] GameObject myManualIndicator;

    [Header("SFX")]
    [SerializeField] AudioClip slideInSfx;
    [SerializeField] AudioClip slideOutSfx;
    [SerializeField] AudioClip changePageSfx;
    [SerializeField] AudioClip changeManualSfx;

    // Need these since sfx is double playing for some reason... Might be a bug from attaching listeners
    bool _isPlayingChangePageSfx = false;
    bool _isPlayingChangeManualSfx = false;

    AudioSource myAudioSource;
    RectTransform myRectTransform;

    private InputAction _moveAction;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
        myRectTransform = GetComponent<RectTransform>();

        // Manuals setup
        SetCurrentManual(_currentManualIdx);
        UpdateTotalManualStepsDisplayed();
        SetActiveStep(myManualIndicator, _currentManualIdx);

        // Page setup, show first manual's first page
        UpdatePageDisplayed();
        UpdateTotalPageStepsDisplayed();
        SetActiveStep(myPageIndicator, _currentPageIdx);


        // Comment these out for easier dev
        myRectTransform.anchoredPosition = offScreenPosition;
        _isDisplayed = false;
        myPageControlsContainer.SetActive(false); // Hide this so it doesn't appear when manuals UI is hidden off to right
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
            myRectTransform.anchoredPosition = offScreenPosition;
            myPageControlsContainer.SetActive(false);
            _isDisplayed = false;
            myAudioSource.PlayOneShot(slideOutSfx);
        }
        else
        {
            myRectTransform.anchoredPosition = onScreenPosition;
            myPageControlsContainer.SetActive(true);
            _isDisplayed = true;
            myAudioSource.PlayOneShot(slideInSfx);
        }
    }


    /*
    Because we are manually listening to the UI actions,
    we have to filter which button is pressed to properly handle controls.
    */
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
        else if (value.y != 0)
        {
            HandleChangePage(value);
        }
    }

    // Note: Reversed idx increment/decrement to match up/down scrolling through pages
    void HandleChangePage(Vector2 value, bool enableSfx = true)
    {
        int prevPageIdx = _currentPageIdx;

        if (value.y > 0)
        {
            _currentPageIdx = Mathf.Max(0, _currentPageIdx - 1);
        }
        else if (value.y < 0)
        {
            _currentPageIdx = Mathf.Min(_currentManual.manualPages.Length - 1, _currentPageIdx + 1);
        }

        // Sfx double plays, so try to stop it with these flags
        if (enableSfx && !_isPlayingChangePageSfx)
        {
            _isPlayingChangePageSfx = true;
            myAudioSource.PlayOneShot(changePageSfx);
        }


        SetInactiveStep(myPageIndicator, prevPageIdx);
        SetActiveStep(myPageIndicator, _currentPageIdx);

        UpdatePageDisplayed();

        _isPlayingChangePageSfx = false;
    }

    void HandleChangeManual(Vector2 value)
    {
        int prevManualIdx = _currentManualIdx;

        if (value.x > 0)
        {
            _currentManualIdx = Mathf.Min(manuals.Length - 1, _currentManualIdx + 1);
        }
        else if (value.x < 0)
        {
            _currentManualIdx = Mathf.Max(0, _currentManualIdx - 1);
        }

        // Sfx double plays, so try to stop it with these flags
        if (!_isPlayingChangeManualSfx)
        {
            _isPlayingChangeManualSfx = true;
            myAudioSource.PlayOneShot(changeManualSfx);
        }


        SetInactiveStep(myManualIndicator, prevManualIdx);
        SetCurrentManual(_currentManualIdx);
        SetActiveStep(myManualIndicator, _currentManualIdx);

        // Draw the correct amount of circles for pages and start at the first
        _currentPageIdx = 0; // Reset to first page
        UpdateTotalPageStepsDisplayed();
        SetActiveStep(myPageIndicator, _currentPageIdx);

        UpdatePageDisplayed();

        _isPlayingChangeManualSfx = false;
    }

    void SetCurrentManual(int idx)
    {
        _currentManual = manuals[idx];
    }

    void UpdatePageDisplayed()
    {
        myManualImageGO.sprite = _currentManual.manualPages[_currentPageIdx];
    }

    void UpdateTotalPageStepsDisplayed()
    {
        int numPagesNeeded = _currentManual.manualPages.Length;

        int numStepsAvailable = myPageIndicator.transform.childCount;

        for (int i = 0; i < numStepsAvailable; i++)
        {
            GameObject currentIndicator = myPageIndicator.transform.GetChild(i).gameObject;

            if (i < numPagesNeeded)
            {
                currentIndicator.SetActive(true);
            }
            else
            {
                currentIndicator.SetActive(false);
            }

            currentIndicator.GetComponent<Image>().color = inactiveStepColor;
        }
    }

    void UpdateTotalManualStepsDisplayed()
    {
        int numStepsAvailable = myManualIndicator.transform.childCount;

        for (int i = 0; i < numStepsAvailable; i++)
        {
            GameObject currentIndicator = myManualIndicator.transform.GetChild(i).gameObject;

            if (i < manuals.Length)
            {
                currentIndicator.SetActive(true);
            }
            else
            {
                currentIndicator.SetActive(false);
            }

            currentIndicator.GetComponent<Image>().color = inactiveStepColor;
        }
    }

    void SetInactiveStep(GameObject targetIndicator, int step)
    {
        Debug.Log("target step " + targetIndicator.transform.GetChild(step).name);
        targetIndicator.transform.GetChild(step).GetComponent<Image>().color = inactiveStepColor;
    }

    void SetActiveStep(GameObject targetIndicator, int activeStep)
    {
        Debug.Log("target step " + targetIndicator.transform.GetChild(activeStep).name);
        targetIndicator.transform.GetChild(activeStep).GetComponent<Image>().color = activeStepColor;
    }
}
