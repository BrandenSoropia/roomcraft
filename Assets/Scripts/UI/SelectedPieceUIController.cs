using UnityEngine;
using Image = UnityEngine.UI.Image;

public class SelectedPieceUIController : MonoBehaviour
{

    [SerializeField] GameObject furniturePieceGO;
    [SerializeField] Sprite placeholderPieceSprite;

    [Header("UI")]
    [SerializeField] GameObject holdThrowControlGO;

    Image myFurniturePieceImage;

    [Header("Internal State")]
    public GameObject renderedSelectedPiece;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myFurniturePieceImage = furniturePieceGO.GetComponent<Image>();

        holdThrowControlGO.SetActive(false);

        // Remove the image on start so we can remove any presets used while developing.
        ClearSelectedPieceImage();
    }

    public void SetSelectedPieceImage(Sprite newImpageSprite)
    {
        myFurniturePieceImage.sprite = newImpageSprite;
    }

    public void SetSelectedPlaceholderPiece()
    {
        myFurniturePieceImage.sprite = placeholderPieceSprite;
        holdThrowControlGO.SetActive(true);
    }

    public void ClearSelectedPieceImage()
    {
        myFurniturePieceImage.sprite = null;
        holdThrowControlGO.SetActive(false);
    }
}
