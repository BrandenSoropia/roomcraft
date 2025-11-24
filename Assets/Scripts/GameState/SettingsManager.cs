using System;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;



public class SettingsManager : MonoBehaviour
{
    [System.Serializable]
    public struct MinMax
    {
        public float min;
        public float max;
    }

    [SerializeField] FirstPersonController firstPersonController;

    [Header("Master Volume")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] float initialMasterVolume = 1f;
    [SerializeField] float masterIncrement = 0.1f;
    [SerializeField] MinMax masterRange;

    [Header("SFX Volume")]
    [SerializeField] private AudioMixer sfxMixer;

    [SerializeField] float initialSFXVolume = 1f;
    [SerializeField] float sfxIncrement = 0.1f;
    [SerializeField] MinMax sfxRange;


    [Header("Aim Sensitivity")]
    [SerializeField] float aimSensitivityIncrement = 0.1f;
    [SerializeField] MinMax aimSensitivityRange;

    void Start()
    {
        SetMusicVolumeLinear(masterMixer, initialMasterVolume, masterRange.max);
        SetMusicVolumeLinear(sfxMixer, initialSFXVolume, sfxRange.max);
    }

    // Getters
    public float GetCurrentMasterVolume()
    {
        return GetMusicVolumeLinear(masterMixer);
    }

    public float GetCurrentSFXVolume()
    {
        return GetMusicVolumeLinear(sfxMixer);
    }

    public float GetCurrentAimSensitivity()
    {
        return firstPersonController.RotationSpeed;
    }

    // Audio converting floats to dB.

    float LinearToDecibel(float linear, float max)
    {
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, max)) * 20f;
    }

    static float DecibelToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

    float GetMusicVolumeLinear(AudioMixer mixer)
    {
        // "Master" is the name of exposed mixer variable from the target Audio Mixer. Customed named it.
        mixer.GetFloat("Master", out float db);
        return DecibelToLinear(db);  // 0–1 range
    }

    void SetMusicVolumeLinear(AudioMixer mixer, float value, float max)
    {
        mixer.SetFloat("Master", LinearToDecibel(value, max));
    }

    public float UpdateMasterVolume(int direction)
    {
        float currentVolume = GetMusicVolumeLinear(masterMixer);

        float newMasterVolume = (float)Math.Round(Mathf.Clamp(currentVolume + (direction * masterIncrement), masterRange.min, masterRange.max), 1);

        if (newMasterVolume == currentVolume) return currentVolume;

        SetMusicVolumeLinear(masterMixer, newMasterVolume, masterRange.max);

        return newMasterVolume;
    }

    public float UpdateSFXVolume(int direction)
    {
        float currentVolume = GetMusicVolumeLinear(sfxMixer);

        float newSfxVolume = (float)Math.Round(Mathf.Clamp(currentVolume + (direction * sfxIncrement), sfxRange.min, sfxRange.max), 1);

        if (newSfxVolume == currentVolume) return currentVolume;

        SetMusicVolumeLinear(sfxMixer, newSfxVolume, sfxRange.max);

        return newSfxVolume;
    }


    public float UpdateAimSensitivity(int direction)
    {
        float newRotationSpeed = (float)Math.Round(Mathf.Clamp(firstPersonController.RotationSpeed + (direction * aimSensitivityIncrement), aimSensitivityRange.min, aimSensitivityRange.max), 1);

        if (newRotationSpeed == firstPersonController.RotationSpeed) return firstPersonController.RotationSpeed;

        firstPersonController.RotationSpeed = newRotationSpeed;

        return newRotationSpeed;
    }
}
