// Editor/WrapAtBoundsCenter.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class WrapAtBoundsCenter
{
    [MenuItem("Tools/Transforms/Wrap Selected At Bounds Center (Y to floor)")]
    public static void Wrap()
    {
        foreach (var go in Selection.gameObjects)
        {
            // Get world bounds from all renderers
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) { Debug.LogWarning($"No renderers under {go.name}"); continue; }
            Bounds wb = rends[0].bounds;
            foreach (var r in rends) wb.Encapsulate(r.bounds);

            // Desired pivot: center in XZ, bottom in Y (floor contact)
            Vector3 pivot = new Vector3(wb.center.x, wb.min.y, wb.center.z);

            // Create wrapper at pivot in world space
            GameObject wrapper = new GameObject(go.name + "_Root");
            Undo.RegisterCreatedObjectUndo(wrapper, "Create Wrapper");
            wrapper.transform.position = pivot;
            wrapper.transform.rotation = go.transform.rotation;
            wrapper.transform.localScale = Vector3.one;

            // Reparent
            Undo.SetTransformParent(wrapper.transform, go.transform.parent, "Reparent Wrapper");
            Undo.SetTransformParent(go.transform, wrapper.transform, "Reparent Child");

            // Preserve appearance by shifting child locally
            go.transform.position = pivot;  // first snap child to pivot
            go.transform.localPosition = Vector3.zero; // keep it centered under wrapper

            // Move Rigidbody/scripts to wrapper
            var rb = go.GetComponent<Rigidbody>();
            if (rb)
            {
                Undo.RecordObject(rb, "Move Rigidbody");
                Object.DestroyImmediate(rb, true);
                rb = Undo.AddComponent<Rigidbody>(wrapper);
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            Debug.Log($"Wrapped {go.name} -> {wrapper.name} at bounds pivot {pivot}");
            Selection.activeGameObject = wrapper;
        }
    }
}
#endif
