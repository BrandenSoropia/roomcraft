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

    // Stuff that controls player
    GameModeController myGameModeController;
    FirstPersonController myFirstPersonController;
    PlayerAnimationController myPlayerAnimationController;
    IsoFurnitureController myIsoFurnitureController;

    private StarterAssetsInputs _input;

    // State Flags
    bool _isOverheadViewEnabled = false;


    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();

        myGameModeController = GetComponent<GameModeController>();
        myFirstPersonController = GetComponent<FirstPersonController>();
        myPlayerAnimationController = GetComponent<PlayerAnimationController>();
        myIsoFurnitureController = GetComponent<IsoFurnitureController>();
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

    public void OnToggleOverheadView(InputValue inputValue)
    {

        if (!inputValue.isPressed) return;

        if (_isOverheadViewEnabled)
        {
            DisableOverheadView();
            EnablePlayer();
            myIsoFurnitureController.enabled = false;
            myPlayerInput.SwitchCurrentActionMap("Player");

        }
        else
        {
            EnableOverheadView();
            DisablePlayer();
            myIsoFurnitureController.enabled = true;
            myPlayerInput.SwitchCurrentActionMap("Isometric");
        }
    }

    private void DisableOverheadView()
    {
        _isOverheadViewEnabled = false;
        myGameModeController.ToggleOverheadView();
    }

    private void EnableOverheadView()
    {
        _isOverheadViewEnabled = true;
        myGameModeController.ToggleOverheadView();
    }

    // Might not be needed if mapping switch works! 
    private void DisablePlayer()
    {
        myFirstPersonController.enabled = false;
        myPlayerAnimationController.enabled = false;
    }

    private void EnablePlayer()
    {
        myFirstPersonController.enabled = true;
        myPlayerAnimationController.enabled = true;
    }
}
