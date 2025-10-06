using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform parentObject; // the root object whose children we care about


    private HashSet<Collider> inside = new HashSet<Collider>();


    private Collider[] childColliders;

    void Start()
    {
        childColliders = parentObject.GetComponentsInChildren<Collider>();

    }

    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Furniture") || other.CompareTag("Marker")) && System.Array.Exists(childColliders, c => c == other))
        {
            inside.Add(other);

            if (inside.Count == childColliders.Length)
            {
                gameManager.IncrementNumCorrectPlacementFurniture();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Furniture") || other.CompareTag("Marker")) && inside.Contains(other))
        {
            inside.Remove(other);

            if (inside.Count == 0)
            {
                gameManager.DecrementNumCorrectPlacementFurniture();
            }

        }
    }
}