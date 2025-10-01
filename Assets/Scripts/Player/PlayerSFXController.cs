using UnityEngine;

public class PlayerSFXController : MonoBehaviour
{
    private AudioSource myAudioSource;

    [Header("Build SFX")]

    [SerializeField] AudioClip attachSfx;
    [SerializeField] AudioClip rotateSfx;
    [SerializeField] AudioClip selectPieceSfx;
    [SerializeField] AudioClip deselectPieceSfx;

    [Header("Inventory SFX")]
    [SerializeField] AudioClip closeInventorySfx;
    [SerializeField] AudioClip openInventorySfx;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    public void PlayRotateSFX()
    {
        myAudioSource.PlayOneShot(rotateSfx);
    }

    public void PlayAttachSFX()
    {
        myAudioSource.PlayOneShot(attachSfx);
    }

    public void PlaySelectPieceSFX()
    {
        myAudioSource.PlayOneShot(selectPieceSfx);
    }

    public void PlayDeselectPieceSFX()
    {
        myAudioSource.PlayOneShot(deselectPieceSfx);
    }

    public void PlayCloseInventorySFX()
    {
        myAudioSource.PlayOneShot(closeInventorySfx);
    }

    public void PlayOpenInventorySFX()
    {
        myAudioSource.PlayOneShot(openInventorySfx);
    }
}
