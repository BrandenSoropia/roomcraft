using UnityEngine;
using UnityEngine.UI;

public class ReticleController : MonoBehaviour
{
    [SerializeField] VacuumGun vacuumGun;

    [Header("Default")]
    [SerializeField] Sprite defaultReticle;
    [SerializeField] Vector3 defaultReticleScale;


    [Header("PickUp/Interact")]

    [SerializeField] Sprite pickupInteractReticle;
    [SerializeField] Vector3 pickupInteractReticleScale;

    [Header("Build")]

    [SerializeField] Sprite buildReticle;
    [SerializeField] Vector3 buildReticleScale;

    [Header("Blocked")]

    [SerializeField] Sprite blockedReticle;
    [SerializeField] Vector3 blockedReticleScale;

    Image myImage;

    void Start()
    {
        myImage = GetComponent<Image>();
        transform.localScale = defaultReticleScale;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeReticle();
    }

    void UseDefaultReticle()
    {
        myImage.sprite = defaultReticle;
        transform.localScale = defaultReticleScale;
    }

    void UsePickupInteractReticle()
    {
        myImage.sprite = pickupInteractReticle;
        transform.localScale = pickupInteractReticleScale;
    }

    void UseBuildReticle()
    {
        myImage.sprite = buildReticle;
        transform.localScale = buildReticleScale;
    }

    void UseBlockedReticle()
    {
        myImage.sprite = blockedReticle;
        transform.localScale = blockedReticleScale;
    }

    void HandleRenderingBuildReticle()
    {
        if (vacuumGun.isHoldingItem)
        {
            UseBuildReticle();
        }
        else
        {
            UseDefaultReticle();
        }
    }
    /*
    Project a ray forward from the player's viewpoint (a.k.a the screen). This is required for aiming.
    Example: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html
    */
    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    void ChangeReticle()
    {
        if (!Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit)) return;

        GameObject target = hit.collider.gameObject;

        switch (target.tag)
        {
            case "Furniture":
            case "Radio":
                UsePickupInteractReticle();
                break;
            case "Marker":
                HandleRenderingBuildReticle();
                break;
            case "BasePiece":
                UseBlockedReticle();
                break;
            default:
                UseDefaultReticle();
                break;
        }
    }
}
