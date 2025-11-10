using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundMusicSO", menuName = "Scriptable Objects/BackgroundMusicSO")]
public class BackgroundMusicSO : ScriptableObject
{
    public AudioClip audioClip;
    [Range(0, 1)] public float volume = 1f;
}
