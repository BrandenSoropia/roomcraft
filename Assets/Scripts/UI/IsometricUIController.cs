using UnityEngine;

public class IsometricUIController : MonoBehaviour
{
    [SerializeField] GameObject requiredCompletedFurnitureMessageUI;

    [Header("Furniture")]
    [SerializeField] GameObject selectFurnitureContainerUI;
    [SerializeField] GameObject moveFurnitureContainerUI;
    [SerializeField] GameObject allRotateControlsContainerUI;

    // TODO if time
    public void DisplayRequiredCompletedFurnitureMessage(bool newState)
    {
        requiredCompletedFurnitureMessageUI.SetActive(newState);
    }

    public void DisplayAllFurnitureMoveAndRotationControls(bool newState)
    {
        moveFurnitureContainerUI.SetActive(newState);
        selectFurnitureContainerUI.SetActive(newState);

        allRotateControlsContainerUI.SetActive(newState);
    }
}
