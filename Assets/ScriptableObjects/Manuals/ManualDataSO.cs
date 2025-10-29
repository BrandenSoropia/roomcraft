using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ManualDataSO", menuName = "Scriptable Objects/ManualDataSO")]
public class ManualDataSO : ScriptableObject
{
    [SerializeField] Sprite[] manualPages;
}
