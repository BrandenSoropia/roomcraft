using StarterAssets;
using UnityEngine;

/*
DISCLAIMER!

Currently not using toggle to 3rd person but we need this script to
properly configure the main camera. Once we move the config to the scene
instead of this script, we can delete this file. 
*/
public class CameraSwitcher : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public FirstPersonController firstPersonController;
    public VacuumGun vg;

    private bool isFirstPerson = true;

    void Start()
    {
        // Ensure only one camera is active at start
        firstPersonCamera.enabled = true;
        firstPersonController.MainCamera = firstPersonCamera;
        vg.playerCamera = firstPersonCamera;
        thirdPersonCamera.enabled = false;
    }

    // void Update()
    // {
    //     // Button9: Xbox/ps, Button10: Switch
    //     if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.JoystickButton9) || Input.GetKeyDown(KeyCode.JoystickButton10))
    //     {
    //         isFirstPerson = !isFirstPerson;

    //         firstPersonCamera.enabled = isFirstPerson;
    //         thirdPersonCamera.enabled = !isFirstPerson;

    //         if (isFirstPerson)
    //         {
    //             firstPersonController.MainCamera = firstPersonCamera;
    //             vg.playerCamera = firstPersonCamera;
    //         }
    //         else
    //         {
    //             firstPersonController.MainCamera = thirdPersonCamera;
    //             vg.playerCamera = thirdPersonCamera;
    //         }
    //     }
    // }
}