using UnityEngine;

public class SettingsUIController : MonoBehaviour
{
    [SerializeField] GameObject myContent;
    PauseManager pauseManager;

    void Start()
    {
        pauseManager = FindFirstObjectByType<PauseManager>();
    }

    public void Open()
    {
        myContent.SetActive(true);
    }
    public void Close()
    {
        myContent.SetActive(false);
    }

    // Button functions

    public void IncreaseMasterVolume()
    {
        Debug.Log("+master vol");
    }

    public void DecreaseMasterVolume()
    {
        Debug.Log("-master vol");
    }
    public void IncreaseSFXVolume()
    {
        Debug.Log("+sfx vol");
    }
    public void DecreaseSFXVolume()
    {
        Debug.Log("-sfx vol");
    }

    public void Back()
    {
        Close();
        pauseManager.PopState();
    }
}
