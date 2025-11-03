using UnityEngine;
using Image = UnityEngine.UI.Image;

public class SelectedItemUIController : MonoBehaviour
{

    [SerializeField] GameObject furniturePieceGO;
    Image myFurniturePieceImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myFurniturePieceImage = furniturePieceGO.GetComponent<Image>();

        // Remove the image on start so we can remove any presets used while developing.
        ClearSelectedPieceImage();
    }

    public void SetSelectedFurniturePieceImage(Sprite newImpageSprite)
    {
        myFurniturePieceImage.sprite = newImpageSprite;
    }

    public void ClearSelectedPieceImage()
    {
        myFurniturePieceImage.sprite = null;
    }
}
