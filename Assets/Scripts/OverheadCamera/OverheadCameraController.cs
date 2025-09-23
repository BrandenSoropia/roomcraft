using UnityEngine;
using UnityEngine.InputSystem;

/*
Requirements:
- Attach to overhead camera
- Make sure it has an Input Action with its own generated InputAction file
- Overhead camera should be disabled initially
*/
public class OverheadCameraController : MonoBehaviour
{

    [Header("Movement Configuration")]
    [SerializeField] float panSpeed = 10f;

    [SerializeField] StarterAssets.StarterAssetsInputs _input;

    void Start()
    {
        // Make sure the overhead camera is off, even if accidentally turned on in editor. 
        gameObject.SetActive(false);
    }

    void Update()
    {
        float xValue = _input.move.x * Time.deltaTime * panSpeed;
        float yValue = _input.move.y * Time.deltaTime * panSpeed;
        float zValue = 0f;

        transform.Translate(xValue, yValue, zValue);
    }
}
