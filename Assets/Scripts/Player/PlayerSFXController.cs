using UnityEngine;

public class PlayerSFXController : MonoBehaviour
{

    [Header("Build SFX")]

    [SerializeField] AudioClip attachSfx;
    [SerializeField] AudioClip rotateSfx;
    [SerializeField] AudioClip selectPieceSfx;
    [SerializeField] AudioClip deselectPieceSfx;
    [SerializeField] AudioClip throwSfx;
    [SerializeField] AudioClip unboxSfx;


    [Header("Inventory SFX")]
    [SerializeField] AudioClip closeInventorySfx;
    [SerializeField] AudioClip openInventorySfx;
    [SerializeField] AudioClip spawnPieceSfx;

    [Header("Toggle Game Mode SFX")]
    [SerializeField, Range(0f, 1f)] float gameModeVolume;

    [SerializeField] AudioClip toBuildModeSfx;
    [SerializeField] AudioClip toIsometricModeSfx;

    private AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Build SFX
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

    public void PlayThrowSFX()
    {
        myAudioSource.PlayOneShot(throwSfx);
    }

    public void PlayUnboxSFX()
    {
        myAudioSource.PlayOneShot(unboxSfx);
    }

    // Inventory SFX

    public void PlayCloseInventorySFX()
    {
        myAudioSource.PlayOneShot(closeInventorySfx);
    }

    public void PlayOpenInventorySFX()
    {
        myAudioSource.PlayOneShot(openInventorySfx);
    }

    public void PlaySpawnPieceSFX()
    {
        myAudioSource.PlayOneShot(spawnPieceSfx);
    }

    /*
    Game Mode SFX
    These are a little loud so just quiet them down programatically.
    */
    public void PlayToBuildModeSFX()
    {
        myAudioSource.PlayOneShot(toBuildModeSfx, gameModeVolume);
    }

    public void PlayToIsometricModeSFX()
    {
        myAudioSource.PlayOneShot(toIsometricModeSfx, gameModeVolume);
    }
}
