using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class ReticleController : MonoBehaviour
{

    [Header("Default")]
    [SerializeField] Sprite defaultReticle;
    [SerializeField] Vector3 defaultReticleScale;


    [Header("Interact")]

    [SerializeField] Sprite interactReticle;
    [SerializeField] Vector3 interactReticleScale;

    UnityEngine.UI.Image myImage;

    void Start()
    {
        myImage = GetComponent<UnityEngine.UI.Image>();
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

    void UseInteractReticle()
    {
        myImage.sprite = interactReticle;
        transform.localScale = interactReticleScale;
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

        if (target.CompareTag("FurnitureBox"))
        {
            UseInteractReticle();
        }
        else
        {
            UseDefaultReticle();
        }
    }
}
