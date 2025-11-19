using TMPro;
using UnityEngine;

public class InteractUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI myTextGui;

    [Header("Control Text")]
    [SerializeField] string pickUpText;
    [SerializeField] string buildText;


    void Start()
    {
        ShowPickUpText();
    }

    public void ShowPickUpText()
    {
        myTextGui.text = pickUpText;
    }

    public void ShowBuildText()
    {
        myTextGui.text = buildText;
    }
}
