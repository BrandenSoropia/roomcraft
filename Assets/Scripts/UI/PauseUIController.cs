using UnityEngine;

public class PauseUIController : MonoBehaviour
{

    [Header("Position")]
    [SerializeField] bool isShown = false;
    [SerializeField] Vector3 onScreenPosition;
    [SerializeField] Vector3 offScreenPosition;

    RectTransform myRectTransform;
    GameManager gameManager;


    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        myRectTransform = GetComponent<RectTransform>();

        if (!isShown)
        {
            HidePauseMenu();
        }
    }

    public void ShowPauseMenu()
    {
        myRectTransform.anchoredPosition = onScreenPosition;

        gameManager.SetState(GameState.Paused);

        isShown = true;
    }

    public void HidePauseMenu()
    {
        myRectTransform.anchoredPosition = offScreenPosition;

        gameManager.UsePreviousStateAsNextState();

        isShown = false;
    }
}
