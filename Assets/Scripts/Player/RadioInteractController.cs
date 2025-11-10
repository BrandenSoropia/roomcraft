using UnityEngine;
using UnityEngine.InputSystem;

public class RadioInteractController : MonoBehaviour
{
    [SerializeField] RadioController radioController;

    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    public void OnChangeRadioSong(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        if (Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Radio"))
            {
                radioController.NextSong();
            }
        }
    }
}
