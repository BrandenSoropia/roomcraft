using UnityEngine;

[ExecuteAlways] // so it shows even in edit mode
public class AreaGizmo : MonoBehaviour
{
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.25f); // semi-transparent green

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Draw cube in position, rotation, and scale of this transform
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        // Optionally: draw wireframe outline
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}