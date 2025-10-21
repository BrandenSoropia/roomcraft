using UnityEngine;

public class PlayerSFXController : MonoBehaviour
{
    private AudioSource myAudioSource;

    [Header("Build SFX")]

    [SerializeField] AudioClip attachSfx;
    [SerializeField] AudioClip rotateSfx;
    [SerializeField] AudioClip selectPieceSfx;
    [SerializeField] AudioClip selectBuildPieceSfx;
    [SerializeField] AudioClip deselectPieceSfx;

    [Header("Inventory SFX")]
    [SerializeField] AudioClip closeInventorySfx;
    [SerializeField] AudioClip openInventorySfx;

    [Header("Toggle Game Mode SFX")]
    [SerializeField] AudioClip toBuildModeSfx;
    [SerializeField] AudioClip toIsometricModeSfx;

    [Header("UI SFX")]
    [SerializeField] AudioClip navigateUISfx;

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

    public void PlaySelectBuildPieceSFX()
    {
        myAudioSource.PlayOneShot(selectBuildPieceSfx);
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

    // Game Mode
    public void PlayToBuildModeSFX()
    {
        myAudioSource.PlayOneShot(toBuildModeSfx);
    }

    public void PlayToIsometricModeSFX()
    {
        myAudioSource.PlayOneShot(toIsometricModeSfx);
    }

    // UI
    public void PlayNavigateUISFX()
    {
        myAudioSource.PlayOneShot(navigateUISfx);
    }
}
