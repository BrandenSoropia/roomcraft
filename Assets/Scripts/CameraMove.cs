using UnityEngine;

public class CameraMoveToMouse : MonoBehaviour
{
    public float moveSpeed = 5f;             // how fast the camera moves
    public float stopDistance = 0.1f;        // distance at which we stop moving
    public LayerMask hitLayers = ~0;         // layers to raycast against (~0 = all)

    private Vector3 targetPosition;
    private bool hasTarget = false;

    void Update()
    {
        // On left mouse click, pick a new target position
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitLayers))
            {
                targetPosition = hit.point;
                hasTarget = true;
            }
        }

        // Move toward the target if we have one
        if (hasTarget)
        {
            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;

            if (distance > stopDistance)
            {
                Vector3 step = direction.normalized * moveSpeed * Time.deltaTime;
                transform.position += step;
            }
            else
            {
                hasTarget = false; // reached the point
            }
        }
    }
}
