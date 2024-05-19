using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryComponent : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    public GameObject inventoryUI; // Assign this in the inspector
    public ItemDatabase itemDatabase;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SaveLoadManager.SaveInventory(this, "inventory.save");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Dictionary<string, Item> itemDictionary = itemDatabase.items.ToDictionary(item => item.itemName, item => item);
            SaveLoadManager.LoadInventory(this, "inventory.save", itemDictionary);
            UpdateInventoryUI();
        }
    }


    public void AddItem(Item item)
    {
        InventoryItem inventoryItem = items.Find(i => i.item == item);

        if (item.stackable && inventoryItem != null)
        {
            // Increase the quantity of the existing inventory item
            inventoryItem.quantity++;
        }
        else
        {
            // Add a new inventory item to the inventory
            items.Add(new InventoryItem(item, 1));
        }

        Debug.Log("Item added: " + item.itemName);

        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem inventoryItem = items[i];
            Item item = inventoryItem.item;
            if (inventoryUI.transform.childCount > i)
            {
                GameObject slotUI = inventoryUI.transform.GetChild(i).gameObject;

                if (slotUI != null)
                {
                    // Assumes that the slot UI has a child GameObject for the item icon and item quantity
                    Image itemIcon = slotUI.transform.Find("Item Icon").GetComponent<Image>();
                    Text itemQuantity = slotUI.transform.Find("Item Quantity").GetComponent<Text>();

                    if (itemIcon != null && itemQuantity != null)
                    {
                        itemIcon.sprite = item.itemIcon;
                        itemIcon.enabled = true; // Enable the item icon
                        itemQuantity.text = inventoryItem.quantity.ToString();
                        itemQuantity.enabled = item.stackable;
                    }
                    else
                    {
                        Debug.LogError("Slot UI does not have the required components");
                    }
                }
                else
                {
                    Debug.LogError("Slot UI is null");
                }
            }
            else
            {
                Debug.LogError("Not enough slots in the inventory UI");
            }
        }
    }

    public Dictionary<string, Item> GetItemDictionary()
    {
        return itemDatabase.items.ToDictionary(item => item.itemName, item => item);
    }

    public void SaveInventory()
    {
        SaveLoadManager.SaveInventory(this, "inventory.save");
    }

    public void LoadInventory()
    {
        Dictionary<string, Item> itemDictionary = itemDatabase.items.ToDictionary(item => item.itemName, item => item);
        SaveLoadManager.LoadInventory(this, "inventory.save", itemDictionary);
        UpdateInventoryUI();
    }


}

