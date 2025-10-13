using UnityEngine;

public class DeletePiece : MonoBehaviour
{
    /*
        Project a ray forward from the player's viewpoint (a.k.a the screen). This is required for aiming.
        Example: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html
        */
    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }
}
