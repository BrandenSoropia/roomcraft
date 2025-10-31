using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] PlayerSFXController playerSFXController;
    [SerializeField] float spawnUpModifier = 0;
    public List<Sprite> itemIcons;
    public UIController UIController;
    public FurniturePiece[] inventory;
    public Camera playerCamera;
    public GameObject modelGhost;
    public Material ghostMaterial;
    public bool spawning = false;
    private Vector3 spawnPosition;
    private GameObject currentGhost;
    private int groundLayer;
    private Vector3 yOffset;
    private int currSlot;
    private ModelGhost ghostController;

    void Start()
    {
        inventory = new FurniturePiece[6];
        groundLayer = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            spawnPosition = hit.point;
        }

        if (spawning)
        {
            currentGhost.transform.position = spawnPosition + yOffset;

            if ((Input.GetMouseButtonDown(0) || Input.GetButtonDown("Submit")) && ghostController != null)
            {
                if (ghostController.noCollision)
                {
                    Destroy(currentGhost);
                    SpawnPiece(currSlot);
                    spawning = false;
                    ResetGhostInfo();
                }
            }
        }
    }

    public void ResetGhostInfo()
    {
        currSlot = 0;
        yOffset = Vector3.zero;
        ghostController = null;
    }

    public void Unbox(List<FurniturePiece> items)
    {
        if (items.Count <= 6)
        {
            UIController.deskIcons.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                UIController.deskIcons.Add(items[i].icon);
                inventory[i] = items[i];
            }
            UIController.DeskClick();
        }
    }

    public void SpawnPiece(int slot)
    {
        if (inventory[slot] != null)
        {
            Debug.Log("spawn a piece: " + spawnPosition);

            FurniturePiece item = inventory[slot];
            FurniturePiece obj = Instantiate(item, spawnPosition + yOffset, Quaternion.identity);

            playerSFXController.PlaySpawnPieceSFX();
        }
        else
        {
            Debug.Log("no item");
        }
    }

    public void SpawnGhostModel(int slot)
    {
        if (inventory[slot] != null)
        {
            spawning = true;
            currSlot = slot;

            FurniturePiece item = inventory[slot];

            // get the model from furniture piece to the ghost
            MeshFilter ghostFilter = modelGhost.GetComponent<MeshFilter>();
            MeshFilter itemFilter = item.GetComponent<MeshFilter>();

            if (ghostFilter != null && itemFilter != null)
            {
                ghostFilter.mesh = itemFilter.sharedMesh;
                BoxCollider ghostCollider = modelGhost.GetComponent<BoxCollider>();

                Bounds ghostBounds = ghostFilter.sharedMesh.bounds;
                yOffset = (-ghostBounds.min.y * modelGhost.transform.localScale.y) * Vector3.up;

                if (ghostCollider != null)
                {
                    ghostCollider.center = itemFilter.sharedMesh.bounds.center;
                    ghostCollider.size = itemFilter.sharedMesh.bounds.size;
                }
                else
                {
                    Debug.Log("ghost box collider is null");
                }
            }
            else
            {
                Debug.Log("ghostFilter or itemFilter is null");
            }

            // get the MeshRenderer from furniture piece to the ghost, this is for the materials
            MeshRenderer ghostRenderer = modelGhost.GetComponent<MeshRenderer>();
            MeshRenderer itemRenderer = item.GetComponent<MeshRenderer>();

            if (ghostRenderer != null && itemRenderer != null)
            {
                var mats = new List<Material>();

                foreach (var _ in itemRenderer.sharedMaterials)
                {
                    mats.Add(ghostMaterial);
                }

                ghostRenderer.sharedMaterials = mats.ToArray();
            }

            ghostFilter.transform.localScale = item.transform.localScale;
            currentGhost = Instantiate(modelGhost, spawnPosition + yOffset, Quaternion.identity);
            ghostController = currentGhost.GetComponent<ModelGhost>();

            // automatically close the UI tab to free player movements
            if (UIController.closed != true)
            {
                UIController.UItab();
            }


        }
        else
        {
            Debug.Log("no ghost item");
        }
    }
}
