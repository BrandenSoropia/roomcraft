using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{

    [Header("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] InputActionAsset uiInputActionAsset;
    string actionMapNameBeforePause;

    [Header("Settings UI")]
    [SerializeField] GameObject windowGO;

    [Header("Build/Isometric UIs"), Tooltip("Used to show/hide when paused")]
    [SerializeField] GameObject progressContainerUI;
    [SerializeField] GameObject buildControlsContainerUI;
    [SerializeField] GameObject selectedPieceContainerUI;
    [SerializeField] GameObject manualContainerUI;
    [SerializeField] GameObject isometricControlsContainerUI;

    // Controller UI

    PauseUIController pauseUIController;
    SettingsUIController settingsUIController;
    ControlSchemeUIController controlSchemeUIController;

    GameManager gameManager;

    // =====================
    // State Management
    // =====================
    private readonly List<PauseState> _stateStack = new();
    public IReadOnlyList<PauseState> StateHistory => _stateStack.AsReadOnly();

    public event Action<PauseState> OnPauseStateChanged;

    // =====================
    // Lifecycle
    // =====================
    private void Awake()
    {
        // initialize with a default state
        Debug.Log("PauseManager: Awake");
        ResetState();
    }

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        pauseUIController = FindFirstObjectByType<PauseUIController>();
        settingsUIController = FindFirstObjectByType<SettingsUIController>();
        controlSchemeUIController = FindFirstObjectByType<ControlSchemeUIController>();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnEnable()
    {
        OnPauseStateChanged += HandleInternalPauseStateChanged;
    }

    private void OnDisable()
    {
        OnPauseStateChanged -= HandleInternalPauseStateChanged;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("PauseManager initialized/reset state...");
        ResetState();
    }

    // Clear everything when scene loads.
    private void OnSceneUnloaded(Scene arg0)
    {
        Debug.Log("PauseManager shutting down...");

        // Clear state history
        if (StateHistory != null)
            _stateStack.Clear();

        // Remove all listeners
        OnPauseStateChanged = null;
    }

    // =====================
    // State Access
    // =====================
    public PauseState CurrentState => _stateStack[^1];

    // =====================
    // State Transitions
    // =====================
    public void ResetState()
    {
        Debug.Log("[PauseManager] Resetting PauseManager for new scene" + _stateStack);

        _stateStack.Clear();
        _stateStack.Add(PauseState.Off);

        // Fire event so UI initializes cleanly
        OnPauseStateChanged?.Invoke(PauseState.Off);
    }


    public void TurnOff()
    {
        ResetState();
    }

    public void TurnOn()
    {
        // Must be called to activate UI controls and disable player controls.
        SetupControls();

        _stateStack.Add(PauseState.Main);
        OnPauseStateChanged?.Invoke(PauseState.Main);
    }

    public void PushState(PauseState newState)
    {
        Debug.Log("PauseManager: PUSH " + newState);
        if (newState == CurrentState)
            return;

        _stateStack.Add(newState);
        OnPauseStateChanged?.Invoke(newState);
    }

    public void PopState()
    {
        if (_stateStack.Count <= 1)
            return; // Don't pop the base state

        _stateStack.RemoveAt(_stateStack.Count - 1);

        OnPauseStateChanged?.Invoke(CurrentState);
    }

    // =====================
    // Handle State Changes
    // =====================
    private void HandleInternalPauseStateChanged(PauseState newState)
    {
        switch (newState)
        {
            case PauseState.Off:
                // TODO: turn all screens off
                HandleTurnOff();
                break;
            case PauseState.Main:
                ShowMain();
                break;
            case PauseState.Settings:
                ShowSettings();
                break;
            case PauseState.ControlScheme:
                ShowControlScheme();
                break;
            default:
                Debug.Log($"PauseManager: {newState} is not handled.");
                break;
        }
    }

    private void ShowSettings()
    {
        Debug.Log("PauseManager: ShowSettings called");
        pauseUIController.Close();
        settingsUIController.Open();
    }

    private void ShowControlScheme()
    {
        Debug.Log("PauseManager: ShowControlScheme called");
        pauseUIController.Close();
        controlSchemeUIController.Open();
    }

    private void HandleTurnOff()
    {
        Debug.Log("PauseManager: Off called");
        // TODO: Add all other screens here to hide
        pauseUIController.Close();
        settingsUIController.Close();
        controlSchemeUIController.Close();

        windowGO.SetActive(false);

        RevertControls();

        // Show build/iso UI again
        DisplayGameModeControlsUI(true);
    }

    void ShowMain()
    {
        // Hide build/iso UI
        DisplayGameModeControlsUI(false);

        // Handle showing UI
        windowGO.SetActive(true);
        pauseUIController.Open();
    }

    void SetupControls()
    {
        // Handle inputs
        if (uiInputActionAsset != null)
        {
            Debug.Log("### enabling UI Input Module");
            uiInputActionAsset.FindActionMap("UI").Enable();
        }

        // Save action map to restore once pause is closed
        actionMapNameBeforePause = playerInput.currentActionMap.name;
        Debug.Log("## action map" + actionMapNameBeforePause);

        // "UI" map used to disable player controls while pause is open.
        playerInput.SwitchCurrentActionMap("UI");
    }

    void RevertControls()
    {
        // Disable so we don't have the UI reading inputs while in build/iso mode
        if (uiInputActionAsset != null)
        {
            Debug.Log("### disabling UI Input Module");
            // uiInputActionAsset.FindActionMap("UI").Disable();
        }

        Debug.Log("## reverting action map" + actionMapNameBeforePause);

        // Revert controls
        if (actionMapNameBeforePause != null)
        {
            playerInput.SwitchCurrentActionMap(actionMapNameBeforePause);
            actionMapNameBeforePause = null;
        }

    }

    void DisplayGameModeControlsUI(bool shouldShow)
    {
        // Handle hiding current game mode's UI
        if (gameManager.CurrentState == GameState.BuildMode)
        {
            buildControlsContainerUI.SetActive(shouldShow);
            selectedPieceContainerUI.SetActive(shouldShow);
            manualContainerUI.SetActive(shouldShow);
            progressContainerUI.SetActive(shouldShow);
        }
        else if (gameManager.CurrentState == GameState.IsometricMode)
        {
            isometricControlsContainerUI.SetActive(shouldShow);
            progressContainerUI.SetActive(shouldShow);
        }
    }

    public void TogglePause()
    {
        if (CurrentState == PauseState.Off)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }
}
