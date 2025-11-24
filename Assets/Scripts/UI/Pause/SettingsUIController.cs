using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SettingsUIController : MonoBehaviour
{
    [SerializeField] GameObject myContent;

    [Header("Value Text Boxes")]
    [SerializeField] TextMeshProUGUI volumeText;
    [SerializeField] TextMeshProUGUI sfxText;
    [SerializeField] TextMeshProUGUI aimSensitivityText;

    PauseManager pauseManager;
    SettingsManager settingsManager;

    InputSystemUIInputModule _uiModule;
    InputAction _cancel;

    void Start()
    {
        pauseManager = FindFirstObjectByType<PauseManager>();
        settingsManager = FindFirstObjectByType<SettingsManager>();
    }

    void OnEnable()
    {
        _uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        if (_uiModule == null)
        {
            Debug.LogWarning("SettingsUIController: No InputSystemUIInputModule found on current EventSystem.");
            return;
        }

        if (_uiModule != null)
        {
            if (_uiModule.cancel != null)
            {
                Debug.Log("SettingsUIController: cancel attached");
                _cancel = _uiModule.cancel;
                _cancel.performed += OnCancelPerformed;
            }
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("SettingsUIController: back called");
        Back();
    }

    void OnDisable()
    {
        Debug.Log("SettingsUIController: cancel deattached");


        if (_cancel != null)
            _cancel.performed -= OnCancelPerformed;
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

    public void IncreaseAimSensitivity()
    {
        float newAimSensitivity = settingsManager.UpdateAimSensitivity(1);

        aimSensitivityText.text = newAimSensitivity.ToString();
        Debug.Log($"+aim vol, new: {newAimSensitivity}");

    }

    public void DecreaseAimSensitivity()
    {
        float newAimSensitivity = settingsManager.UpdateAimSensitivity(-1);

        aimSensitivityText.text = newAimSensitivity.ToString();
        Debug.Log($"-aim vol, new: {newAimSensitivity}");
    }

    public void Back()
    {
        if (pauseManager.CurrentState != PauseState.Settings) return;

        Close();
        pauseManager.PopState();
    }
}
