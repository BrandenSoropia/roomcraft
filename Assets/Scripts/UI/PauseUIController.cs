using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseUIController : MonoBehaviour
{
    [Header("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    string actionMapNameBeforePause;


    [Header("Position")]
    [SerializeField] bool isShown = false;
    [SerializeField] Vector3 onScreenPosition;
    [SerializeField] Vector3 offScreenPosition;

    RectTransform myRectTransform;
    GameManager gameManager;


    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        myRectTransform = GetComponent<RectTransform>();

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
        ShowPauseMenu();

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
        HidePauseMenu();

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
