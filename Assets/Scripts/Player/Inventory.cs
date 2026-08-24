using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{

    List<GameObject> inventoryItems = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void addInventory(GameObject go)
    {

        bool itemFound = false;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (go.GetEntityId() == inventoryItems[i].GetEntityId())
            {
                itemFound = true;
                break;
            }
        }

        if (!itemFound) {
            inventoryItems.Add(go);        
        }
    }

}
