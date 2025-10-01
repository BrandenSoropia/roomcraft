using UnityEngine;

public class ProximityPlay : MonoBehaviour
{
    [SerializeField] AudioSource bgMusicSource;

    AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bgMusicSource.Stop();
            myAudioSource.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            myAudioSource.Stop();
            bgMusicSource.Play();
        }
    }
}
