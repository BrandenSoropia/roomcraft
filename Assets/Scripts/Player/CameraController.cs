using StarterAssets;
using UnityEngine;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) // Press V to toggle
        {
            isFirstPerson = !isFirstPerson;

            firstPersonCamera.enabled = isFirstPerson;
            thirdPersonCamera.enabled = !isFirstPerson;

            if (isFirstPerson)
            {
                firstPersonController.MainCamera = firstPersonCamera;
                vg.playerCamera = firstPersonCamera;
            }
            else
            {
                firstPersonController.MainCamera = thirdPersonCamera;
                vg.playerCamera = thirdPersonCamera;
            }
        }
    }
}