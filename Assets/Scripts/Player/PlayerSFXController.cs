using UnityEngine;

public class PlayerSFXController : MonoBehaviour
{
    [Header("Build SFX")]

    [SerializeField] AudioClip attachSfx;
    [SerializeField] AudioClip rotateSfx;
    [SerializeField] AudioClip selectPieceSfx;
    [SerializeField] AudioClip deselectPieceSfx;
    [SerializeField] AudioClip unboxSfx;


    [Header("Inventory SFX")]
    [SerializeField] AudioClip closeInventorySfx;
    [SerializeField] AudioClip openInventorySfx;
    [SerializeField] AudioClip spawnPieceSfx;

    [Header("Toggle Game Mode SFX")]
    [SerializeField, Range(0f, 1f)] float gameModeVolume;

    [SerializeField] AudioClip toBuildModeSfx;
    [SerializeField] AudioClip toIsometricModeSfx;

    private AudioSource _myAudioSource;
    float _initialVolume;

    void Start()
    {
        _myAudioSource = GetComponent<AudioSource>();
        _initialVolume = _myAudioSource.volume;
    }

    // Build SFX

    public void PlayRotateSFX()
    {
        _myAudioSource.PlayOneShot(rotateSfx);
    }

    public void PlayAttachSFX()
    {
        _myAudioSource.PlayOneShot(attachSfx);
    }

    public void PlaySelectPieceSFX()
    {
        _myAudioSource.PlayOneShot(selectPieceSfx);
    }

    public void PlayDeselectPieceSFX()
    {
        _myAudioSource.PlayOneShot(deselectPieceSfx);
    }

    public void PlayUnboxSFX()
    {
        _myAudioSource.PlayOneShot(unboxSfx);
    }

    // Inventory SFX

    public void PlayCloseInventorySFX()
    {
        _myAudioSource.PlayOneShot(closeInventorySfx);
    }

    public void PlayOpenInventorySFX()
    {
        _myAudioSource.PlayOneShot(openInventorySfx);
    }

    public void PlaySpawnPieceSFX()
    {
        _myAudioSource.PlayOneShot(spawnPieceSfx);
    }

    /*
    Game Mode SFX

    These are a little loud so just quiet them down programatically.
    */
    public void PlayToBuildModeSFX()
    {
        _myAudioSource.PlayOneShot(toBuildModeSfx, gameModeVolume);
    }

    public void PlayToIsometricModeSFX()
    {
        _myAudioSource.PlayOneShot(toIsometricModeSfx, gameModeVolume);
    }
}
