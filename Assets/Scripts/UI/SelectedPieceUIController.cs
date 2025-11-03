using UnityEngine;
using Image = UnityEngine.UI.Image;

public class SelectedPieceUIController : MonoBehaviour
{

    [SerializeField] GameObject furniturePieceGO;
    [SerializeField] Sprite placeholderPieceSprite;
    Image myFurniturePieceImage;

    [Header("Internal State")]
    public GameObject renderedSelectedPiece;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myFurniturePieceImage = furniturePieceGO.GetComponent<Image>();

        // Remove the image on start so we can remove any presets used while developing.
        ClearSelectedPieceImage();
    }

    public void RenderSelectedPiece(GameObject selectedPieceGO)
    {
        renderedSelectedPiece = Instantiate(selectedPieceGO, transform.position, Quaternion.identity);
    }

    public void ClearRenderedSelectedPiece()
    {
        Destroy(renderedSelectedPiece);
    }

    public void SetSelectedPieceImage(Sprite newImpageSprite)
    {
        myFurniturePieceImage.sprite = newImpageSprite;
    }

    public void SetSelectedPlaceholderPiece()
    {
        myFurniturePieceImage.sprite = placeholderPieceSprite;
    }

    public void ClearSelectedPieceImage()
    {
        myFurniturePieceImage.sprite = null;
    }
}
