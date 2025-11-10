using System;
using UnityEngine;

public class RadioController : MonoBehaviour
{
    [Header("Music Manager")]
    [SerializeField, Tooltip("Music Manager GO for isolated music controls")] AudioSource bgmMusicManagerAudioSource;
    [SerializeField] bool loop = true;
    [Header("Songs")]
    [SerializeField] BackgroundMusicSO[] songConfigs;

    [Header("Internal State")]
    [SerializeField] int currentSongIdx = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        BackgroundMusicSO initialBGMConfig = songConfigs[currentSongIdx];

        bgmMusicManagerAudioSource.loop = loop;

        bgmMusicManagerAudioSource.volume = initialBGMConfig.volume;

        bgmMusicManagerAudioSource.PlayOneShot(initialBGMConfig.audioClip);
    }

}
