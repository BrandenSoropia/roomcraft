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

    [Header("Wall Hiding")]
    [SerializeField] WallAutoHider_ScreenTriangle wallHider;

    public void ShowOverheadView()
    {
        DeselectAllFurniture(); // Ensure selections/pivots are cleared before switching
        playerCamera.SetActive(false);
        overheadCamera.SetActive(true);
        overheadCamera.tag = "MainCamera";

        var h = GetWallHider();
        if (h) h.EnableHiding();
    }

    public void ShowOriginalView()
    {
        playerCamera.SetActive(true);
        overheadCamera.SetActive(false);
        overheadCamera.tag = "Untagged";

        var h = GetWallHider();
        if (h) h.DisableHiding();
    }

    // Resolve the hider if not assigned
    WallAutoHider_ScreenTriangle GetWallHider()
    {
        if (wallHider) return wallHider;
        if (overheadCamera)
        {
            wallHider = overheadCamera.GetComponent<WallAutoHider_ScreenTriangle>();
            if (!wallHider)
                wallHider = overheadCamera.GetComponentInChildren<WallAutoHider_ScreenTriangle>(true);
        }
        if (!wallHider)
        {
#if UNITY_2023_1_OR_NEWER
            wallHider = Object.FindFirstObjectByType<WallAutoHider_ScreenTriangle>();
#else
            wallHider = Object.FindObjectOfType<WallAutoHider_ScreenTriangle>();
#endif
        }
        return wallHider;
    }

    // Deselect all furniture selections and remove any pivots
    void DeselectAllFurniture()
    {
#if UNITY_2023_1_OR_NEWER
        var rotators = Object.FindObjectsByType<FurnitureRotator>(FindObjectsSortMode.None);
#else
        var rotators = Object.FindObjectsOfType<FurnitureRotator>();
#endif
        foreach (var r in rotators)
        {
            if (r) r.DeselectAll();
        }
    }
}
