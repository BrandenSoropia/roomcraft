using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Stats")]
    public int _numCorrectPlacementFurniture = 0;

    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }

    // Maintain singleton
    private void Awake()
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

    public void IncrementNumCorrectPlacementFurniture()
    {
        _numCorrectPlacementFurniture += 1;
    }

    public void DecrementNumCorrectPlacementFurniture()
    {
        _numCorrectPlacementFurniture -= 1;
    }
}
