using System;
using System.Collections;
using UnityEngine;

public class RadioController : MonoBehaviour
{
    [Header("Music Manager"), Tooltip("Music Manager GO for isolated music controls")]
    [SerializeField] AudioSource audioSourceA;
    [SerializeField] AudioSource audioSourceB;
    [SerializeField] float fadeDuration = 1.5f;
    [SerializeField] bool loop = true;

    [Header("Songs")]
    [SerializeField] BackgroundMusicSO[] songConfigs;

    [Header("Internal State")]
    [SerializeField] int currentSongIdx = 0;
    [SerializeField] bool isAudioSourceAPlaying = true;

    private Coroutine fadeCoroutine = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (songConfigs == null || songConfigs.Length == 0)
        {
            Debug.LogWarning("songConfigs empty, fill it in or RadioController won't work!");
            return;
        }

        // Config both sources to loop
        audioSourceA.loop = loop;
        audioSourceB.loop = loop;

        audioSourceB.volume = 0f; // make sure B is "off"

        PlayCurrentSong();
    }

    BackgroundMusicSO GetCurrentSongConfig()
    {
        return songConfigs[currentSongIdx];
    }

    public void NextSong()
    {
        if (songConfigs == null || songConfigs.Length == 0) return;

        // Handles cycling index
        currentSongIdx = (currentSongIdx + 1) % songConfigs.Length;

        // Setup for fading music between both sources.
        AudioSource active = isAudioSourceAPlaying ? audioSourceA : audioSourceB;
        AudioSource next = isAudioSourceAPlaying ? audioSourceB : audioSourceA;
        isAudioSourceAPlaying = !isAudioSourceAPlaying;

        // Handle multiple calls cleanly!
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        BackgroundMusicSO nextSong = songConfigs[currentSongIdx];

        next.clip = nextSong.audioClip;
        next.volume = 0f;
        next.Play();

        fadeCoroutine = StartCoroutine(Crossfade(active, next));
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float t = 0f;
        float fromVolumeOriginal = from.volume;
        float toVolumeFinal = GetCurrentSongConfig().volume;


        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;
            from.volume = Mathf.Lerp(fromVolumeOriginal, 0f, normalized);
            to.volume = Mathf.Lerp(0f, toVolumeFinal, normalized);
            yield return null;
        }

        from.Stop();
        from.volume = toVolumeFinal;

        fadeCoroutine = null; // once fade in time is done, clear coroutine
    }

    void PlayCurrentSong()
    {
        BackgroundMusicSO initialBGMConfig = GetCurrentSongConfig();

        audioSourceA.volume = initialBGMConfig.volume;
        audioSourceA.clip = initialBGMConfig.audioClip;
        audioSourceA.Play();
    }

}
