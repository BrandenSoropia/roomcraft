using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip rotationClip;
    public AudioClip attachingClip;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayRotation()
    {
        audioSource.PlayOneShot(rotationClip);
    }

    public void PlayAttaching()
    {
        audioSource.PlayOneShot(attachingClip);
    }
}
