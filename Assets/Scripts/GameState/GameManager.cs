using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }


    [Header("State for Debugging")]
    public bool _isBuildingEnabled = true;
    public bool _isPlayerMovementEnabled = true;


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
}
