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

    void Update()
    {
        HandleToggleOverheadView();
    }

    public void OnToggleInventory(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        uiController.UItab();
    }

    private void HandleToggleOverheadView()
    {
        if (_input.isOverheadViewEnabled)
        {
            if (_isOverheadViewEnabled)
            {
                DisableOverheadView();
                EnablePlayer();
                myIsoFurnitureController.enabled = false;
            }
            else
            {
                EnableOverheadView();
                DisablePlayer();
                myIsoFurnitureController.enabled = true;
            }

            // Treats the toggle button as a one time press event. switch to context.isPressed later
            _input.isOverheadViewEnabled = false;
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
