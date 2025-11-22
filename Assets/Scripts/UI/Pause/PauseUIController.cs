using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseUIController : MonoBehaviour
{
    [SerializeField] GameObject myContent;
    PauseManager pauseManager;

    void Start()
    {
        pauseManager = FindFirstObjectByType<PauseManager>();
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

    // Button Functionality

    public void Resume()
    {
        Debug.Log("Resuming game");
        pauseManager.TurnOff();
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting level");

        Scene activeScene = SceneManager.GetActiveScene();
        pauseManager.TurnOff(); // Do this in case weird stuff gets carried over.
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void Settings()
    {
        Debug.Log("Showing Settings");
        pauseManager.PushState(PauseState.Settings);
    }

    public void GoToStartScreen()
    {
        Debug.Log("Going to Start Screen");
        pauseManager.TurnOff(); // Do this in case weird stuff gets carried over.
        SceneManager.LoadScene("StartScreen");
    }

    public void ExitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}
