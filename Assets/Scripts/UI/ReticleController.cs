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
                HandleMarkerReticle(target);
                break;
            case "BasePiece":
                UseBlockedReticle();
                break;
            default:
                UseDefaultReticle();
                break;
        }
    }
    
    void HandleMarkerReticle(GameObject marker)
    {
        // Not holding anything → show default
        if (!vacuumGun.isHoldingItem)
        {
            UseDefaultReticle();
            return;
        }

        GameObject heldPiece = vacuumGun.GetHeldPreview();

        // When holdingItem is true but preview isn't spawned yet, do not revert to default.
        if (heldPiece == null)
        {
            UseBuildReticle();
            return;
        }

        // Prefix matching
        string piecePrefix = GetPiecePrefix(heldPiece.name);
        string markerPrefix = GetMarkerPrefix(marker.name);

        if (piecePrefix.Equals(markerPrefix, System.StringComparison.OrdinalIgnoreCase))
            UseBuildReticle();
        else
            UseBlockedReticle();
    }
    
    string GetPiecePrefix(string name)
    {
        int idx = name.IndexOf("(");
        if (idx > 0)
            name = name.Substring(0, idx);

        return name.Trim();
    }

    string GetMarkerPrefix(string name)
    {
        int idxSide = name.IndexOf("_side_marker");
        if (idxSide > 0)
            return name.Substring(0, idxSide);

        int idxMarker = name.IndexOf("_marker");
        if (idxMarker > 0)
            return name.Substring(0, idxMarker);

        return name.Trim();
    }
}
