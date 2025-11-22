using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MenuSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform highlightWood;  // the moving wood image
    [SerializeField] private List<Button> buttons = new List<Button>(); // buttons in order: top → bottom
    [SerializeField] private float moveSpeed = 8f;

    private int currentIndex = 0;
    private RectTransform targetRect;

    void Start()
    {
        if (buttons.Count == 0 || highlightWood == null)
        {
            Debug.LogError("[MenuSelector] Missing buttons or highlightWood reference.");
            return;
        }

        // Initialize
        currentIndex = 0;
        targetRect = buttons[currentIndex].GetComponent<RectTransform>();
        highlightWood.position = targetRect.position;

        // Make sure an EventSystem exists
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
    }

    void Update()
    {
        if (buttons.Count == 0 || highlightWood == null) return;

        // Smoothly move the wood towards the target button
        if (targetRect)
        {
            highlightWood.position = Vector3.Lerp(
                highlightWood.position,
                targetRect.position,
                Time.deltaTime * moveSpeed
            );
        }

        // Keyboard navigation
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }

        // Confirm
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            buttons[currentIndex].onClick.Invoke();
        }
    }

    private void MoveSelection(int direction)
    {
        int newIndex = Mathf.Clamp(currentIndex + direction, 0, buttons.Count - 1);
        if (newIndex == currentIndex) return;

        currentIndex = newIndex;
        targetRect = buttons[currentIndex].GetComponent<RectTransform>();
        EventSystem.current.SetSelectedGameObject(buttons[currentIndex].gameObject);
    }
}
