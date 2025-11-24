using UnityEngine;

public class PlayerSFXController : MonoBehaviour
{

    [Header("Build SFX")]

    public AudioClip attachSfx;
    public AudioClip rotateSfx;
    public AudioClip selectPieceSfx;
    public AudioClip deselectPieceSfx;
    public AudioClip suckSfx;
    public AudioClip dropSfx;
    public AudioClip throwSfx;
    public AudioClip unboxSfx;
    public AudioClip uiMoveSfx;

    [Header("Build Audio Sources")]
    public AudioSource attachSource;
    public AudioSource rotateSource;
    public AudioSource selectPieceSource;
    public AudioSource deselectPieceSource;
    public AudioSource suckSource;
    public AudioSource dropSource;
    public AudioSource throwSource;
    public AudioSource unboxSource;
    public AudioSource uiMoveSource;



    [Header("Inventory SFX")]
    public AudioClip closeInventorySfx;
    public AudioClip openInventorySfx;
    public AudioClip spawnPieceSfx;

    [Header("Inventory Audio Sources")]
    public AudioSource closeInventorySource;
    public AudioSource openInventorySource;
    public AudioSource spawnPiecesSource;

    [Header("Toggle Game Mode SFX")]

    public AudioClip toBuildModeSfx;
    public AudioClip toIsometricModeSfx;

    [Header("Toggle Game Mode Audio Sources")]
    public AudioSource toBuildModeSource;
    public AudioSource toIsometricModeSource;

    private AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Build SFX
    public void PlayRotateSFX()
    {
        rotateSource.PlayOneShot(rotateSfx);
    }

    public void PlayAttachSFX()
    {
        attachSource.PlayOneShot(attachSfx);
    }

    public void PlaySelectPieceSFX()
    {
        selectPieceSource.PlayOneShot(selectPieceSfx);
    }

    public void PlayDeselectPieceSFX()
    {
        deselectPieceSource.PlayOneShot(deselectPieceSfx);
    }

    public void PlaySuckSFX()
    {
        suckSource.PlayOneShot(suckSfx);
    }

    public void PlayDropSFX()
    {
        dropSource.PlayOneShot(dropSfx);
    }

    public void PlayThrowSFX()
    {
        throwSource.PlayOneShot(throwSfx);
    }

    public void PlayUnboxSFX()
    {
        unboxSource.PlayOneShot(unboxSfx);
    }

    public void PlayUIMoveSFX()
    {
        uiMoveSource.PlayOneShot(uiMoveSfx);
    }

    // Inventory SFX

    public void PlayCloseInventorySFX()
    {
        closeInventorySource.PlayOneShot(closeInventorySfx);
    }

    public void PlayOpenInventorySFX()
    {
        openInventorySource.PlayOneShot(openInventorySfx);
    }

    public void PlaySpawnPieceSFX()
    {
        spawnPiecesSource.PlayOneShot(spawnPieceSfx);
    }


    //Game Mode SFX
    public void PlayToBuildModeSFX()
    {
        toBuildModeSource.PlayOneShot(toBuildModeSfx);
    }

    public void PlayToIsometricModeSFX()
    {
        toIsometricModeSource.PlayOneShot(toIsometricModeSfx);
    }
}
