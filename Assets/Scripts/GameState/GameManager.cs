using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Stats")]
    [SerializeField] int _numCorrectPlacementFurniture = 0;
    [SerializeField] int numBuilt = 0;
    [SerializeField] int numTotalFurniture = 1;


    [Header("Controlled UI")]
    [SerializeField] TextMeshProUGUI placementProgressGUI;
    [SerializeField] TextMeshProUGUI buildProgressGUI;

    // messy, furniture manager knows this already. maybe connect somehow in the future


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
        UpdateBuildProgressGUI();
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

    public void IncrementNumBuilt()
    {
        numBuilt += 1;
        UpdateBuildProgressGUI();
    }

    void UpdatePlacementProgressGUI()
    {
        placementProgressGUI.text = $"Furniture Placed: {_numCorrectPlacementFurniture}/{numTotalFurniture}";
    }

    void UpdateBuildProgressGUI()
    {
        buildProgressGUI.text = $"Built: {numBuilt}/{numTotalFurniture}";
    }


}
