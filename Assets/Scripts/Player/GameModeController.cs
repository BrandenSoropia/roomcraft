using UnityEngine;
/*
Source: https://docs.unity3d.com/6000.2/Documentation/Manual/MultipleCameras.html

Requirements:
- Attach to root player GO
- Attach the 2 cameras to controll

*/
public class GameModeController : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] GameManager gameManager;

    [Header("Controlled Cameras")]
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject overheadCamera;

    // Actual state controller for transitioning cameras
    bool _isOverheadViewEnabled = false;

    public void ToggleOverheadView()
    {
        if (_isOverheadViewEnabled)
        {
            _isOverheadViewEnabled = false;
            ShowOriginalView();
        }
        else
        {
            _isOverheadViewEnabled = true;
            ShowOverheadView();
        }
    }


    public void ShowOverheadView()
    {

        playerCamera.SetActive(false);
        overheadCamera.SetActive(true);
        overheadCamera.tag = "MainCamera";
        // TODO: disable furniture building
        // TODO: disable UI
        gameManager.SetIsBuildingEnabled(false);
    }

    public void ShowOriginalView()
    {
        playerCamera.SetActive(true);
        overheadCamera.SetActive(false);
        overheadCamera.tag = "Untagged";

        // TODO: enable furniture building
        // TODO: enable UI
        gameManager.SetIsBuildingEnabled(true);

    }
}
