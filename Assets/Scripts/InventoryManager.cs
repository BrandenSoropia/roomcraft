using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float spawnUpModifier = 0;
    public List<Sprite> itemIcons;
    public UIController UIController;
    public FurniturePiece[] inventory;
    public Camera playerCamera;
    public bool spawning;
    private Vector3 spawnPosition;

    void Start()
    {
        inventory = new FurniturePiece[6];
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            spawnPosition = hit.point;
        }
    }

    //This method is to show the inventory's connection to the UI for the Tech Demo only
    public void SetUpExamplePicks()
    {
        UIController.couchIcons.Add(itemIcons[0]);
        UIController.couchIcons.Add(itemIcons[1]);
        UIController.couchIcons.Add(itemIcons[2]);

        UIController.shelfIcons.Add(itemIcons[3]);
        UIController.shelfIcons.Add(itemIcons[4]);
        UIController.shelfIcons.Add(itemIcons[5]);
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
            // Some pieces are spawning underground,so here's a hack to custom fix each piece
            Vector3 yOffset = (Vector3.up * item.yOffset);
            FurniturePiece obj = Instantiate(item, spawnPosition + yOffset, Quaternion.identity);
        }
        else
        {
            Debug.Log("no item");
        }
    }
}
