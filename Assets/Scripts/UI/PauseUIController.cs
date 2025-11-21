using UnityEngine;

public class PauseUIController : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] bool isShown = false;
    [SerializeField] Vector3 onScreenPosition;
    [SerializeField] Vector3 offScreenPosition;

    RectTransform myRectTransform;

    void Start()
    {
        myRectTransform = GetComponent<RectTransform>();

        if (!isShown)
        {
            HidePauseMenu();
        }
    }

    void ShowPauseMenu()
    {
        myRectTransform.anchoredPosition = onScreenPosition;
    }

    void HidePauseMenu()
    {
        myRectTransform.anchoredPosition = offScreenPosition;
    }

    public void TogglePauseMenu()
    {
        if (isShown)
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }

        isShown = !isShown;
    }
}
