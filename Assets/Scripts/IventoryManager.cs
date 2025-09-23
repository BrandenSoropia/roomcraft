using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Sprite> itemIcons;
    public UIController UIController;
    public Dictionary<GameObject, Sprite> inventory;

    void Start()
    {
        inventory = new Dictionary<GameObject, Sprite>();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    //This method might be changed depending on the implementation of furniture items
    public void AddToInventory(GameObject item, Sprite icon)
    {
        if (!inventory.ContainsKey(item))
        {
            inventory.Add(item, icon);
        }
    }
}
