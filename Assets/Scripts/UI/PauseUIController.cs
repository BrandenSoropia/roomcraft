using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PauseUIController : MonoBehaviour
{
    [Header("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] InputActionAsset uiInputActionAsset;
    string actionMapNameBeforePause;


    [Header("Position")]
    [SerializeField] bool isShown = false;
    [SerializeField] Vector3 onScreenPosition;
    [SerializeField] Vector3 offScreenPosition;

    [Header("Control UIs"), Tooltip("Used to show/hide when paused")]
    [SerializeField] GameObject progressContainerUI;
    [SerializeField] GameObject buildControlsContainerUI;
    [SerializeField] GameObject selectedPieceContainerUI;
    [SerializeField] GameObject manualContainerUI;
    [SerializeField] GameObject isometricControlsContainerUI;


    RectTransform myRectTransform;
    GameManager gameManager;

    void Start()
    {
        myRectTransform = GetComponent<RectTransform>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (!isShown)
        {
            HidePauseMenu();
        }
    }

    void ShowPauseMenu()
    {
        myRectTransform.anchoredPosition = onScreenPosition;
    }

    public void HandleShowPauseMenu()
    {
        if (uiInputActionAsset != null)
        {
            Debug.Log("### enabling UI Input Module");
            uiInputActionAsset.FindActionMap("UI").Enable();

        }
        ShowPauseMenu();

        if (gameManager.CurrentState == GameState.BuildMode)
        {
            DisplayBuildModeControls(false);
        }
        else if (gameManager.CurrentState == GameState.IsometricMode)
        {
            DisplayIsometricControls(false);
        }

        gameManager.SetState(GameState.Paused);

        isShown = true;

        // Save action map to restore once pause is closed
        actionMapNameBeforePause = playerInput.currentActionMap.name;
        Debug.Log("## action map" + actionMapNameBeforePause);
        // "UI" map used to disable player controls while pause is open.
        playerInput.SwitchCurrentActionMap("UI");
    }

    void HidePauseMenu()
    {
        myRectTransform.anchoredPosition = offScreenPosition;
    }

    public void HandleHidePauseMenu()
    {

        if (uiInputActionAsset != null)
        {
            Debug.Log("### disabling UI Input Module");
            uiInputActionAsset.FindActionMap("UI").Disable();
        }

        HidePauseMenu();

        if (gameManager.PreviousState == GameState.BuildMode)
        {
            DisplayBuildModeControls(true);
        }
        else if (gameManager.PreviousState == GameState.IsometricMode)
        {
            DisplayIsometricControls(true);
        }

        gameManager.UsePreviousStateAsNextState();

        isShown = false;
        Debug.Log("## reverting action map" + actionMapNameBeforePause);

        // Revert controls
        // Careful, can trigger twice when we press A on controller.
        // It is due to MenuSelector calling .invoke, while the button has an onPress naturally handling it submits.
        // I disabled interactability on the pause menu buttons to fix this since we are
        // programatically invoking onClicks via MenuSelector
        if (actionMapNameBeforePause != null)
        {
            playerInput.SwitchCurrentActionMap(actionMapNameBeforePause);
            actionMapNameBeforePause = null;
        }

    }

    // UI container display controls
    void DisplayBuildModeControls(bool newState)
    {
        buildControlsContainerUI.SetActive(newState);
        selectedPieceContainerUI.SetActive(newState);
        manualContainerUI.SetActive(newState);
        progressContainerUI.SetActive(newState);
    }

    void DisplayIsometricControls(bool newState)
    {
        isometricControlsContainerUI.SetActive(newState);
        progressContainerUI.SetActive(newState);
    }

    // Button Functionality

    public void Resume()
    {
        Debug.Log("Resuming game");
        HandleHidePauseMenu();
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting level");

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void Settings()
    {
        Debug.Log("Showing Settings");

    }

    public void GoToStartScreen()
    {
        Debug.Log("Going to Start Screen");
        SceneManager.LoadScene("StartScreen");
    }

    public void ExitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}
