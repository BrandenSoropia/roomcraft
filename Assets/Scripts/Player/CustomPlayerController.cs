using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/*
Potentially anti-pattern that manages enabling/disabling specific Player components.
Currently used to disable First Person Controller when switching views.


Requirements:
- Make sure this is attached to the player's root GO
- Make sure an input is defined for Toggle View and it's properly added to StarterAssets.
*/
public class CustomPlayerController : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    [Header("UI Controllers")]
    [SerializeField] GameObject buildControlsContainerUI;
    [SerializeField] GameObject isometricControlsContainerUI;
    [SerializeField] GameObject selectedItemContainerUI;
    [SerializeField] GameObject manualContainerUI;

    [Header("Player Controllers")]
    [SerializeField] PlayerInput myPlayerInput;
    [SerializeField] PlayerSFXController playerSFXController;

    // Stuff that controls player
    GameModeController myGameModeController;
    FirstPersonController myFirstPersonController;
    PlayerAnimationController myPlayerAnimationController;
    IsoFurnitureController myIsoFurnitureController;
    [SerializeField] ManualUIController manualUIController;


    void Start()
    {
        myGameModeController = GetComponent<GameModeController>();
        myFirstPersonController = GetComponent<FirstPersonController>();
        myPlayerAnimationController = GetComponent<PlayerAnimationController>();
        myIsoFurnitureController = GetComponent<IsoFurnitureController>();

        // Make sure isometric controller is off first thing. Sometimes we forget to turn it off in editor.
        myIsoFurnitureController.enabled = false;
    }

    public void OnQuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }

    public void OnToggleIsometricView(InputValue inputValue)
    {

        if (!inputValue.isPressed) return;

        if (gameManager.CurrentState == GameState.IsometricMode)
        {
            gameManager.SetState(GameState.BuildMode);

            isometricControlsContainerUI.SetActive(false);
            DisplayBuildModeContainerUI(true);

            myIsoFurnitureController.enabled = false;
            SetPlayerScriptsEnabledState(true);
            myPlayerInput.SwitchCurrentActionMap("Player");
            playerSFXController.PlayToBuildModeSFX();
            myGameModeController.ShowOriginalView();
            myIsoFurnitureController.SetCameraActive(false);
        }
        else
        {
            gameManager.SetState(GameState.IsometricMode);

            isometricControlsContainerUI.SetActive(true);
            DisplayBuildModeContainerUI(false);

            myIsoFurnitureController.enabled = true;
            SetPlayerScriptsEnabledState(false);
            myPlayerInput.SwitchCurrentActionMap("Placement");
            playerSFXController.PlayToIsometricModeSFX();
            myGameModeController.ShowOverheadView();
            myIsoFurnitureController.SetCameraActive(true);
        }
    }

    void DisplayBuildModeContainerUI(bool newState)
    {
        buildControlsContainerUI.SetActive(newState);
        selectedItemContainerUI.SetActive(newState);
        manualContainerUI.SetActive(newState);
    }

    public void OnToggleDisplayManual(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        manualUIController.ToggleDisplay();
    }

    void SetPlayerScriptsEnabledState(bool newState)
    {
        myFirstPersonController.enabled = newState;
        // myPlayerAnimationController.enabled = newState;
    }
}
