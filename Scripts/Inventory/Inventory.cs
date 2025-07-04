using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class Inventory : MonoBehaviour
    {
        public int rows = 4;
        public int columns = 5;
        public int totalSlots = 20;
        public GameObject slotPrefab;
        public Transform inventoryGrid;
        public Button nextPageButton;
        public Button previousPageButton;
        public List<InventorySlot> inventorySlots = new List<InventorySlot>();
        public int currentPage = 0;
        private int pages;
        public List<InventoryItem> allItemsList; // Add this to your script to hold all possible items

        public ItemDB itemDB;

        public int Pages
        {
            get { return pages; }
        }

        void Start()
        {
            SetupInventoryUI();
            nextPageButton.onClick.AddListener(NextPage);
            previousPageButton.onClick.AddListener(PreviousPage);

            if (itemDB != null)
            {
                PopulateAllItemsList();
            }
        }

        public void PopulateAllItemsList()
        {
            allItemsList = itemDB.items;
        }

        public void SetupInventoryUI()
        {
            foreach (Transform child in inventoryGrid)
            {
                Destroy(child.gameObject);
            }
            inventorySlots.Clear();

            int slotsPerPage = rows * columns;
            pages = Mathf.CeilToInt((float)totalSlots / slotsPerPage);

            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObject = Instantiate(slotPrefab, inventoryGrid);
                InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.slot.SetTransformProperties();
                    inventorySlots.Add(slotUI.slot);

                    InventoryDragHandler dragHandler = slotObject.transform.Find("DraggableItem").GetComponent<InventoryDragHandler>();
                    if (dragHandler != null)
                    {
                        dragHandler.slot = slotUI.slot;
                    }
                }
            }

            UpdatePage();
        }

        public void AddItem(InventoryItem item, int quantity = 1, InventorySlot specificSlot = null)
        {
            if (specificSlot != null)
            {
                if (specificSlot.item == null)
                {
                    int amountToAdd = Mathf.Min(quantity, item.maxStackSize);
                    specificSlot.SetItem(item, amountToAdd);
                    specificSlot.SetTransformProperties();
                    return; // Exit the method after setting the item in the specific slot
                }
                else if (specificSlot.item == item && specificSlot.quantity < item.maxStackSize)
                {
                    int amountToAdd = Mathf.Min(quantity, item.maxStackSize - specificSlot.quantity);
                    specificSlot.quantity += amountToAdd;
                    specificSlot.stackText.text = specificSlot.quantity.ToString();
                    return; // Exit the method after adding to the stack in the specific slot
                }
                else
                {
                    Debug.LogWarning("Specific slot is full or item mismatch!");
                }
            }

            // Default behavior if no specific slot or specific slot couldn't be used
            while (quantity > 0)
            {
                InventorySlot existingSlot = FindItemSlot(item);

                if (existingSlot != null)
                {
                    int amountToAdd = Mathf.Min(quantity, item.maxStackSize - existingSlot.quantity);
                    existingSlot.quantity += amountToAdd;
                    existingSlot.stackText.text = existingSlot.quantity.ToString();
                    quantity -= amountToAdd;
                }
                else
                {
                    InventorySlot newSlot = inventorySlots.Find(slot => slot.item == null);
                    if (newSlot != null)
                    {
                        int amountToAdd = Mathf.Min(quantity, item.maxStackSize);
                        newSlot.SetItem(item, amountToAdd);
                        newSlot.SetTransformProperties();
                        quantity -= amountToAdd;
                    }
                    else
                    {
                        Debug.LogWarning("Inventory is full!");
                        break;
                    }
                }
            }
        }

        public void RemoveItem(InventoryItem item, int quantity = 1)
        {
            InventorySlot existingSlot = FindItemSlot(item);

            if (existingSlot != null)
            {
                existingSlot.quantity -= quantity;
                if (existingSlot.quantity <= 0)
                {
                    existingSlot.SetItem(null, 0);
                }
                else
                {
                    existingSlot.stackText.text = existingSlot.quantity.ToString();
                }
            }
            else
            {
                InventorySlot slot = inventorySlots.Find(s => s.item == item);
                if (slot != null)
                {
                    slot.SetItem(null, 0);
                }
                else
                {
                    Debug.LogWarning("Item not found in inventory!");
                }
            }
        }

        public void RemoveItemFromSlot(InventorySlot slot, int quantity)
        {
            if (slot != null)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                {
                    slot.SetItem(null, 0);
                }
                else
                {
                    if (slot.quantity > 1)
                    {
                        slot.stackText.text = slot.quantity.ToString();
                    }
                    else
                    {
                        slot.stackText.text = "";
                    }
                }
            }
            else
            {
                Debug.LogWarning("Slot not found in inventory!");
            }
        }

        public void ClearItems()
        {
            foreach (var slot in inventorySlots)
            {
                slot.SetItem(null, 0);
            }
        }


        private InventorySlot FindItemSlot(InventoryItem item)
        {
            return inventorySlots.Find(slot => slot.item == item && slot.quantity < item.maxStackSize);
        }

        public void SetPage(int pageIndex)
        {
            currentPage = pageIndex;
            UpdatePage();
        }

        public void NextPage()
        {
            if (currentPage < Pages - 1)
            {
                currentPage++;
                UpdatePage();
            }
        }

        public void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePage();
            }
        }

        public void UpdatePage()
        {
            int slotsPerPage = rows * columns;
            int startSlot = currentPage * slotsPerPage;
            int endSlot = Mathf.Min(startSlot + slotsPerPage, totalSlots);

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (i >= startSlot && i < endSlot)
                {
                    inventorySlots[i].slotObject.SetActive(true);
                }
                else
                {
                    inventorySlots[i].slotObject.SetActive(false);
                }
            }

            nextPageButton.interactable = currentPage < Pages - 1;
            previousPageButton.interactable = currentPage > 0;
        }

        public List<InventoryItemData> GetItems()
        {
            List<InventoryItemData> items = new List<InventoryItemData>();
            foreach (var slot in inventorySlots)
            {
                if (slot.item != null)
                {
                    items.Add(new InventoryItemData { itemName = slot.item.itemName, quantity = slot.quantity });
                    Debug.Log($"Saved Inventory Item: {slot.item.itemName} with Quantity: {slot.quantity}");
                }
            }
            return items;
        }

        public void LoadItems(List<InventoryItemData> items)
        {
            foreach (var itemData in items)
            {
                InventoryItem item = FindItemByName(itemData.itemName);
                if (item != null)
                {
                    AddItem(item, itemData.quantity);
                    Debug.Log($"Loaded Inventory Item: {item.itemName} with Quantity: {itemData.quantity}");
                }
                else
                {
                    Debug.LogWarning($"Item {itemData.itemName} not found in allItemsList.");
                }
            }
        }

        private InventoryItem FindItemByName(string itemName)
        {
            return allItemsList.Find(item => item.itemName == itemName);
        }
    } 
}

