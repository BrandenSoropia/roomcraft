using System.Collections.Generic;
using UnityEngine;

/*
Place this on each PlacementArea.
1 PlacementArea to 1 furniture.

Requirements:
- Each furniture piece must have:
  - a collider
  - either tagged as "Furniture" or "Marker" since we have to count all colliders present
*/
public class AreaTrigger : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform parentObject; // the root object whose children we care about

    private HashSet<Collider> inside = new HashSet<Collider>();
    private bool _hasReducedPlacementCall = false;


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
                _hasReducedPlacementCall = false; // Reset this so we can reduce the total placed counter again
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Furniture") || other.CompareTag("Marker")) && inside.Contains(other))
        {
            inside.Remove(other);

            /*
            Only allow reducing the total placement counter once per exit.
            Without this, it will decrement the progress each time any piece exits the area.
            */
            if (!_hasReducedPlacementCall)
            {
                gameManager.DecrementNumCorrectPlacementFurniture();
                _hasReducedPlacementCall = true;
            }

        }
    }
}