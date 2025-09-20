using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Log the name of the clicked object
                Debug.Log("Clicked on: " + hit.collider.gameObject.name);

                // You can add more specific logic here,
                // for example, calling a method on the clicked object:
                // if (hit.collider.gameObject.CompareTag("Interactable"))
                // {
                //     hit.collider.gameObject.GetComponent<InteractableObject>().Interact();
                // }
            }
        }
    }
}