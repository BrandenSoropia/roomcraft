using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{

    [Header("Scene Roots")]
    public Transform piecesRoot;
    public Transform basePiecesRoot;
    public Transform placementAreasRoot;

    [Header("Levels")]
    public int nextLevelIndex;

    private int _numCorrectPlacementFurniture = 0;
    private int _numBuilt = 0;
    public int _numTotalTargets = 0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI placementProgressGUI;
    [SerializeField] Image placementProgressCheckImage;
    [SerializeField] private TextMeshProUGUI buildProgressGUI;
    [SerializeField] Image buildProgressCheckIcon;


    public GameObject winScreen;
    public Button winCloseButton;
    [SerializeField] private TextMeshProUGUI completedItemText;
    public GameObject nextLevelScreen;
    public Button nextProceedButton;
    public Button nextExitButton;
    [SerializeField] private TextMeshProUGUI commissionItemText;
    
    public GameObject controllerScreen;
    public GameObject gameProgressContainer;
    public GameObject crosshair;
    public GameObject manualUIContainer;
    public GameObject selectedPieceContainer;
    
    [SerializeField] private TextMeshProUGUI timerText;        // active timer during play
    [SerializeField] private TextMeshProUGUI timeSpentText;    // shown on Win Screen

    private float _levelStartTime = 0f;
    private bool _isTimerRunning = false;

    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private int _currentLevelIndex = -1;
    private GameObject _spawnedPieces;
    private GameObject _spawnedBasePieces;
    private GameObject _spawnedPlacementAreas;

    // ================================================================
    // Lifecycle
    // ================================================================
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (winScreen) winScreen.SetActive(false);
        if (nextLevelScreen) nextLevelScreen.SetActive(false);

        // Hook up button callbacks
        if (winCloseButton)
        {
            winCloseButton.onClick.RemoveAllListeners();
            winCloseButton.onClick.AddListener(OnWinCloseClicked);
        }
        if (nextProceedButton)
        {
            nextProceedButton.onClick.RemoveAllListeners();
            nextProceedButton.onClick.AddListener(OnProceedClicked);
        }
        if (nextExitButton)
        {
            nextExitButton.onClick.RemoveAllListeners();
            nextExitButton.onClick.AddListener(OnExitClicked);
        }

        // Set up navigation for buttons (D-Pad / keyboard Tab)
        SetupButtonNavigation();
    }

    void Start()
    {
        if (!piecesRoot || !basePiecesRoot || !placementAreasRoot)
        {
            Debug.LogError("[GameManager] Assign Pieces/BasePieces/PlacementAreas roots in the Inspector!");
            return;
        }

        UpdatePlacementProgressGUI();
        UpdateBuildProgressGUI();
    }

    // ================================================================
    // UI Navigation Setup
    // ================================================================
    void SetupButtonNavigation()
    {
        if (winCloseButton)
        {
            Navigation nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnRight = nextProceedButton,
                selectOnLeft = nextExitButton
            };
            winCloseButton.navigation = nav;
        }

        if (nextProceedButton && nextExitButton)
        {
            // Horizontal navigation between Next Level buttons
            Navigation proceedNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnRight = nextExitButton,
                selectOnLeft = nextExitButton
            };
            nextProceedButton.navigation = proceedNav;

            Navigation exitNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = nextProceedButton,
                selectOnRight = nextProceedButton
            };
            nextExitButton.navigation = exitNav;
        }
    }

    // ================================================================
    // Game State
    // ================================================================
    public void SetState(GameState newState)
    {
        if (newState == CurrentState) return;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    // ================================================================
    // Counters
    // ================================================================
    public void IncrementNumCorrectPlacementFurniture()
    {
        if (_numCorrectPlacementFurniture < _numTotalTargets)
        {
            _numCorrectPlacementFurniture++;
            UpdatePlacementProgressGUI();
            CheckLevelComplete();
        }
    }

    public void DecrementNumCorrectPlacementFurniture()
    {
        if (_numCorrectPlacementFurniture > 0)
        {
            _numCorrectPlacementFurniture--;
            UpdatePlacementProgressGUI();
        }
    }

    public void IncrementNumBuilt()
    {
        _numBuilt++;
        UpdateBuildProgressGUI();
    }

    // ================================================================
    // Completion Flow
    // ================================================================
    void CheckLevelComplete()
    {
        if (_numCorrectPlacementFurniture == _numTotalTargets && _numTotalTargets > 0)
        {
            Debug.Log("[GameManager] Level Complete!");

            // 🕒 Stop the timer
            _isTimerRunning = false;

            // 🕒 Compute final elapsed time
            float totalTime = Time.time - _levelStartTime;
            int minutes = Mathf.FloorToInt(totalTime / 60f);
            int seconds = Mathf.FloorToInt(totalTime % 60f);

            if (timeSpentText)
                timeSpentText.text = $"{minutes:00}:{seconds:00}";

            // ✅ Hide gameplay UI
            SetGameplayUIVisible(false);

            // Fill CompletedItemText from CURRENT level placement areas
            if (completedItemText && _spawnedPlacementAreas)
            {
                var names = GetItemPrefixesFromPlacementAreas(_spawnedPlacementAreas);
                completedItemText.text = names.Count > 0
                    ? string.Join("\n", names)
                    : "—";
            }

            if (winScreen)
            {
                winScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(winCloseButton.gameObject);
            }
        }
    }

    void OnWinCloseClicked()
    {
        if (winScreen) winScreen.SetActive(false);

        if (nextLevelIndex > 0)
        {
            Debug.Log("loading level at index " + nextLevelIndex);
            SceneManager.LoadScene(nextLevelIndex);
        }
    }

    void OnProceedClicked()
    {
        if (nextLevelScreen) nextLevelScreen.SetActive(false);

        // Bring gameplay UI back once you start next level
        SetGameplayUIVisible(true);

        int next = _currentLevelIndex + 1;
    }

    void OnExitClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    int CountPlacementTargets(GameObject placementAreasRootGO)
    {
        if (!placementAreasRootGO) return 0;
        var areas = placementAreasRootGO.GetComponentsInChildren<AreaTrigger>(true);
        return areas?.Length ?? 0;
    }

    // ================================================================
    // UI Updates
    // ================================================================

    /*
    Make sure the Check Image GO are enabled, and the image is set with alpha 0 initially.
    */
    void ShowProgressCheckImage(Image targetImage, bool shouldShow)
    {
        Color newAlphaColour = targetImage.color;
        newAlphaColour.a = shouldShow ? 1f : 0f;

        targetImage.color = newAlphaColour;
    }

    void UpdatePlacementProgressGUI()
    {
        if (placementProgressGUI)
            placementProgressGUI.text = $"Placed: {_numCorrectPlacementFurniture}/{_numTotalTargets}";

        ShowProgressCheckImage(placementProgressCheckImage, _numCorrectPlacementFurniture == _numTotalTargets);
    }

    void UpdateBuildProgressGUI()
    {
        if (buildProgressGUI)
            buildProgressGUI.text = $"Built: {_numBuilt}/{_numTotalTargets}";

        Debug.Log($"num built updated{_numBuilt}/{_numTotalTargets}");

        ShowProgressCheckImage(buildProgressCheckIcon, _numBuilt == _numTotalTargets);
    }

    // ================================================================
    // Utility
    // ================================================================
    static void ClearChildren(Transform root)
    {
        if (!root) return;
        for (int i = root.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(root.GetChild(i).gameObject);
            else Destroy(root.GetChild(i).gameObject);
#else
            Destroy(root.GetChild(i).gameObject);
#endif
        }
    }

    static void ResetLocal(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    void SafeDestroy(ref GameObject go)
    {
        if (!go) return;
#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(go);
        else Destroy(go);
#else
        Destroy(go);
#endif
        go = null;
    }
    
    void SafeDestroyFContainer(GameObject parent)
    {
        if (parent == null || parent.transform.childCount == 0) return;

        Transform firstChild = parent.transform.GetChild(0);
        string childName = firstChild.name;

        string[] parts = childName.Split('_');
        if (parts.Length < 3)
        {
            Debug.LogWarning($"[GameManager] Cannot extract prefix from '{childName}' (needs at least 3 parts).");
            return;
        }

        string prefix = $"{parts[0]}_{parts[1]}_{parts[2]}";
        string containerName = prefix + "_FContainer";

        Transform container = FindObjectOfType<Transform>(true);
        var all = FindObjectsOfType<Transform>(true);

        foreach (var t in all)
        {
            if (t.name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
            {
                t.gameObject.SetActive(false);
                Debug.Log($"[GameManager] Soft-deleted container: {containerName}");
                return;
            }
        }

        Debug.Log($"[GameManager] No container found for prefix '{prefix}'");
    }
    

    // Extract first three underscore-separated parts; if fewer than 3, return the whole name
    static string ExtractPrefix3(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var parts = name.Split('_');
        if (parts.Length >= 3) return $"{parts[0]}_{parts[1]}_{parts[2]}";
        return name;
    }

    // From an instantiated placement areas GameObject (current level)
    List<string> GetItemPrefixesFromPlacementAreas(GameObject placementAreasRootGO)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!placementAreasRootGO) return new List<string>();

        // Prefer AreaTrigger names if present
        var areas = placementAreasRootGO.GetComponentsInChildren<AreaTrigger>(true);
        if (areas != null && areas.Length > 0)
        {
            foreach (var a in areas)
            {
                if (!a) continue;
                var prefix = ExtractPrefix3(a.transform.name);
                if (!string.IsNullOrWhiteSpace(prefix)) result.Add(prefix);
            }
        }
        else
        {
            // Fallback: use the first child's name
            if (placementAreasRootGO.transform.childCount > 0)
            {
                var firstChild = placementAreasRootGO.transform.GetChild(0);
                var prefix = ExtractPrefix3(firstChild.name);
                if (!string.IsNullOrWhiteSpace(prefix)) result.Add(prefix);
            }
        }

        return new List<string>(result);
    }

    // From a prefab asset for the NEXT level (not instantiated yet)
    List<string> GetItemPrefixesFromPlacementPrefab(GameObject placementAreasPrefab)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!placementAreasPrefab) return new List<string>();

        // If AreaTrigger components exist on the prefab, use them
        var areas = placementAreasPrefab.GetComponentsInChildren<AreaTrigger>(true);
        if (areas != null && areas.Length > 0)
        {
            foreach (var a in areas)
            {
                if (!a) continue;
                var prefix = ExtractPrefix3(a.transform.name);
                if (!string.IsNullOrWhiteSpace(prefix)) result.Add(prefix);
            }
        }
        else
        {
            // Fallback: use the prefab's first child name
            var trs = placementAreasPrefab.GetComponentsInChildren<Transform>(true);
            // find first real child that isn't the root
            Transform firstChild = null;
            foreach (var t in trs)
            {
                if (t && t.gameObject != placementAreasPrefab)
                {
                    firstChild = t;
                    break;
                }
            }

            if (firstChild)
            {
                var prefix = ExtractPrefix3(firstChild.name);
                if (!string.IsNullOrWhiteSpace(prefix)) result.Add(prefix);
            }
        }

        return new List<string>(result);
    }
    
    private void SetGameplayUIVisible(bool visible)
    {
        if (controllerScreen) controllerScreen.SetActive(visible);
        if (gameProgressContainer) gameProgressContainer.SetActive(visible);
        if (crosshair) crosshair.SetActive(visible);
        if (manualUIContainer) manualUIContainer.SetActive(visible);
        if (selectedPieceContainer) selectedPieceContainer.SetActive(visible);
    }

    void Update()
    {
        if (_isTimerRunning && timerText)
        {
            float elapsed = Time.time - _levelStartTime;
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        // reload scene
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
