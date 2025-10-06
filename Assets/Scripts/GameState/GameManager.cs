using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Stats")]
    [SerializeField] int _numCorrectPlacementFurniture = 0;
    [SerializeField] int _numTotalFurnitureToBuild = 0;

    [Header("Controlled UI")]
    [SerializeField] TextMeshProUGUI placementProgressText;

    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }

    // Maintain singleton
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        UpdatePlacementProgressGUI(); // Make placement UI is properly matching text to level configuration in GameManager 
    }

    public void IncrementNumCorrectPlacementFurniture()
    {
        _numCorrectPlacementFurniture += 1;
        UpdatePlacementProgressGUI();
    }

    public void DecrementNumCorrectPlacementFurniture()
    {
        _numCorrectPlacementFurniture -= 1;
        UpdatePlacementProgressGUI();
    }

    void UpdatePlacementProgressGUI()
    {
        placementProgressText.text = $"Furniture Placed: {_numCorrectPlacementFurniture}/{_numTotalFurnitureToBuild}";
    }
}
