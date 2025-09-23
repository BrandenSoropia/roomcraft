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
public class PlayerController : MonoBehaviour
{

    // Stuff that controls player
    GameModeController myGameModeController;
    FirstPersonController myFirstPersonController;
    PlayerAnimationController myPlayerAnimationController;

    private StarterAssetsInputs _input;

    // State Flags
    bool _isOverheadViewEnabled = false;


    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();

        myGameModeController = GetComponent<GameModeController>();
        myFirstPersonController = GetComponent<FirstPersonController>();
        myPlayerAnimationController = GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        HandleToggleOverheadView();
    }

    private void HandleToggleOverheadView()
    {
        if (_input.isOverheadViewEnabled)
        {
            if (_isOverheadViewEnabled)
            {
                DisableOverheadView();
                EnablePlayer();
            }
            else
            {
                EnableOverheadView();
                DisablePlayer();
            }

            _input.isOverheadViewEnabled = false;
        }
    }

    private void DisableOverheadView()
    {
        _isOverheadViewEnabled = false;
        myGameModeController.ShowOriginalView();
    }

    private void EnableOverheadView()
    {
        _isOverheadViewEnabled = true;
        myGameModeController.ShowOverheadView();
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
