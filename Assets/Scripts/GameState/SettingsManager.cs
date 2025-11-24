using System;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;



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
    [SerializeField] float initialMasterVolume = 0.7f;
    [SerializeField] float masterIncrement = 0.1f;
    [SerializeField] MinMax masterRange;



    [Header("SFX Volume")]
    [SerializeField] float initialSFXVolume = 1;
    [SerializeField] float sfxIncrement = 0.1f;
    [SerializeField] MinMax sfxRange;


    [Header("Aim Sensitivity")]
    [SerializeField] float aimSensitivityIncrement = 0.1f;
    [SerializeField] MinMax aimSensitivityRange;

    public float UpdateAimSensitivity(int direction)
    {
        float newRotationSpeed = (float)Math.Round(Mathf.Clamp(firstPersonController.RotationSpeed + (direction * aimSensitivityIncrement), aimSensitivityRange.min, aimSensitivityRange.max), 1);

        if (newRotationSpeed == firstPersonController.RotationSpeed) return firstPersonController.RotationSpeed;

        firstPersonController.RotationSpeed = newRotationSpeed;

        return newRotationSpeed;
    }
}
