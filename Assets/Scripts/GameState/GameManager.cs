using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool _isBuildingEnabled = true;
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

    public bool GetIsBuildingEnabled()
    {
        return _isBuildingEnabled;
    }

    public void SetIsBuildingEnabled(bool newState)
    {
        _isBuildingEnabled = newState;
    }
}
