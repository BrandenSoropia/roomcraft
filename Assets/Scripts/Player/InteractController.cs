using UnityEngine;
using UnityEngine.InputSystem;

public class InteractController : MonoBehaviour
{
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] Animator MyAnimator;

    Ray _GetCurrentScreenCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    public void OnOpenBox(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        if (Physics.Raycast(_GetCurrentScreenCenterRay(), out RaycastHit hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("FurnitureBox"))
            {
                if (MyAnimator != null)
                {
                    MyAnimator.SetTrigger("BoxOpening");
                }

                FurnitureBox fb = clickedObject.GetComponent<FurnitureBox>();

                if (fb != null)
                {
                    inventoryManager.Unbox(fb.items);
                }

                Destroy(clickedObject, 1.5f);

                return;
            }
        }
    }
}
