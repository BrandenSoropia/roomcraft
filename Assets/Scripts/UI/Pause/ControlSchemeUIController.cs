using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/*
Disclaimer, since the GameObject is always enabled and we have event listeners setup,
they will always trigger even if the page isn't shown.

This is currently "fixed" by checking if the current state is this page.
*/
public class ControlSchemeUIController : MonoBehaviour
{

    [SerializeField] GameObject myContent;
    [SerializeField] List<GameObject> schemePages = new List<GameObject>();
    int currentPageIdx = 0;

    PauseManager pauseManager;
    InputSystemUIInputModule _uiModule;
    InputAction _tabLeft;
    InputAction _tabRight;
    InputAction _cancel;

    void Start()
    {
        pauseManager = FindFirstObjectByType<PauseManager>();
    }

    void OnEnable()
    {
        _uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        if (_uiModule == null)
        {
            Debug.LogWarning("ControlSchemeUIController: No InputSystemUIInputModule found on current EventSystem.");
            return;
        }

        if (_uiModule != null)
        {
            _tabLeft = _uiModule.actionsAsset.FindAction("TabLeft");
            _tabRight = _uiModule.actionsAsset.FindAction("TabRight");

            if (_tabLeft != null)
            {
                Debug.Log("ControlSchemeUIController: tabLeft attached");
                _tabLeft.performed += OnTabLeft;
            }

            if (_tabRight != null)
            {
                Debug.Log("ControlSchemeUIController: tabRight attached");
                _tabRight.performed += OnTabRight;
            }

            if (_uiModule.cancel != null)
            {
                Debug.Log("ControlSchemeUIController: cancel attached");
                _cancel = _uiModule.cancel;
                _cancel.performed += OnCancelPerformed;
            }
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("ControlSchemeUIController: back called");
        Back();
    }

    void OnDisable()
    {
        Debug.Log("ControlSchemeUIController: tabLeft/Right/cancel deattached");

        if (_tabLeft != null)
            _tabLeft.performed -= OnTabLeft;

        if (_tabRight != null)
            _tabRight.performed -= OnTabRight;

        if (_cancel != null)
            _cancel.performed -= OnCancelPerformed;
    }

    void MoveSelection(int direction)
    {
        if (pauseManager.CurrentState != PauseState.ControlScheme) return;

        int prevIdx = currentPageIdx;
        int newIndex = Mathf.Clamp(currentPageIdx + direction, 0, schemePages.Count - 1);
        if (newIndex == currentPageIdx) return;

        currentPageIdx = newIndex;

        schemePages[prevIdx].SetActive(false); // hide old page

        schemePages[currentPageIdx].SetActive(true); // show new
    }

    void OnTabLeft(InputAction.CallbackContext ctx)
    {
        if (pauseManager.CurrentState != PauseState.ControlScheme) return;

        Debug.Log("Prev Scheme");
        MoveSelection(-1);
    }

    void OnTabRight(InputAction.CallbackContext ctx)
    {

        if (pauseManager.CurrentState != PauseState.ControlScheme) return;

        Debug.Log("next Scheme");
        MoveSelection(1);
    }

    // Handle Show/Hides
    public void Open()
    {
        myContent.SetActive(true);
    }

    public void Close()
    {
        myContent.SetActive(false);
    }

    public void Back()
    {
        if (pauseManager.CurrentState != PauseState.ControlScheme) return;

        Close();
        pauseManager.PopState();
    }
}
