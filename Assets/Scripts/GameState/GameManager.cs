using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Stats")]
    [SerializeField] int _numCorrectPlacementFurniture = 0;
    [SerializeField] int _numBuilt = 0;
    [SerializeField] int numTotalFurniture = 1;


    [Header("Controlled UI")]
    [SerializeField] TextMeshProUGUI placementProgressGUI;
    [SerializeField] TextMeshProUGUI buildProgressGUI;
    [SerializeField] TextMeshProUGUI winMessageGUI;

    [Header("Game State")]
    public GameState CurrentState { get; private set; }
    // Event Listener: gets invoked when the state changes
    public event Action<GameState> OnGameStateChanged;

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

    public void SetState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;

        OnGameStateChanged?.Invoke(newState);

        Debug.Log($"Game state changed to: {newState}");
    }

    void CheckCommissionComplete()
    {
        if (_numCorrectPlacementFurniture == numTotalFurniture)
        {
            winMessageGUI.enabled = true;
        }
    }

    public void IncrementNumCorrectPlacementFurniture()
    {
        if (_numCorrectPlacementFurniture != numTotalFurniture)
        {
            _numCorrectPlacementFurniture += 1;
            UpdatePlacementProgressGUI();

            CheckCommissionComplete();
        }

    }

    public void DecrementNumCorrectPlacementFurniture()
    {
        if (_numCorrectPlacementFurniture != 0)
        {
            _numCorrectPlacementFurniture -= 1;
            UpdatePlacementProgressGUI();
        }
    }

    public void IncrementNumBuilt()
    {
        _numBuilt += 1;
        UpdateBuildProgressGUI();
    }

    void UpdatePlacementProgressGUI()
    {
        placementProgressGUI.text = $"Furniture Placed: {_numCorrectPlacementFurniture}/{numTotalFurniture}";
    }

    void UpdateBuildProgressGUI()
    {
        buildProgressGUI.text = $"Built: {_numBuilt}/{numTotalFurniture}";
    }
}
