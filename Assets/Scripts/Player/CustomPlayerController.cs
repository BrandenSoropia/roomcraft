using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
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

    [SerializeField] UIController uiController;
    [SerializeField] PlayerInput myPlayerInput;
    [SerializeField] PlayerSFXController playerSFXController;

    // Stuff that controls player
    GameModeController myGameModeController;
    FirstPersonController myFirstPersonController;
    PlayerAnimationController myPlayerAnimationController;
    IsoFurnitureController myIsoFurnitureController;

    // State Flags
    bool _isIsometricViewEnabled = false;


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

    public void OnToggleInventory(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        uiController.UItab();
    }

    public void OnToggleIsometricView(InputValue inputValue)
    {

        if (!inputValue.isPressed) return;

        if (_isIsometricViewEnabled)
        {
            _isIsometricViewEnabled = false;
            myIsoFurnitureController.enabled = false;
            SetPlayerScriptsEnabledState(true);
            myPlayerInput.SwitchCurrentActionMap("Player");
            playerSFXController.PlayCloseInventorySFX();
            myGameModeController.ShowOriginalView();
            myIsoFurnitureController.SetCameraActive(false);
        }
        else
        {
            _isIsometricViewEnabled = true;
            myIsoFurnitureController.enabled = true;
            SetPlayerScriptsEnabledState(false);
            myPlayerInput.SwitchCurrentActionMap("Placement");
            playerSFXController.PlayOpenInventorySFX();
            myGameModeController.ShowOverheadView();
            myIsoFurnitureController.SetCameraActive(true);
        }
    }

    void SetPlayerScriptsEnabledState(bool newState)
    {
        myFirstPersonController.enabled = newState;
        myPlayerAnimationController.enabled = newState;
    }
}
