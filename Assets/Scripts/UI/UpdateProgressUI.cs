using TMPro;
using UnityEngine;

public class UpdateProgressUI : MonoBehaviour
{
    public TextMeshProUGUI placementProgress; // assign in Inspector
    public int score = 0;

    void Start()
    {
        placementProgress.text = "Score: " + score;
    }

    void Update()
    {
        // Example: increment score and update text
        if (Input.GetKeyDown(KeyCode.Space))
        {
            score += 10;
            placementProgress.text = "Score: " + score;
        }
    }
}
