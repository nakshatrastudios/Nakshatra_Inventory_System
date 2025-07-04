using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class QuickAccessBar : MonoBehaviour
    {
        [Header("Slot Configuration")]
        public int totalSlots = 10;
        public GameObject slotPrefab;
        public Transform quickAccessGrid;
        public GameObject buttonNumberTextPrefab; // Assign a Text prefab with the necessary settings

        [Header("Item Database")]
        public ItemDB itemDB;                // ← Reference to your ItemDB
        public List<InventoryItem> allItemsList; // ← Holds all possible items

        [Header("Runtime State")]
        public List<InventorySlot> quickAccessSlots = new List<InventorySlot>();
        private PlayerStatus playerStatus;

        private void Start()
        {
            // —— NEW: Auto-assign ItemDB from the Player's Inventory if not set in inspector ——
            if (itemDB == null)
            {
                var inv = GameObject.FindWithTag("Player")?.GetComponent<Inventory>();
                if (inv != null)
                    itemDB = inv.itemDB;
                else
                    Debug.LogError("QuickAccessBar: could not find Inventory on Player to assign ItemDB.");
            }

            SetupQuickAccessBar();

            playerStatus = GameObject.FindWithTag("Player")?.GetComponent<PlayerStatus>();
            if (playerStatus == null)
            {
                Debug.LogError("QuickAccessBar: PlayerStatus component not found on the Player GameObject.");
            }

            if (itemDB != null)
            {
                PopulateAllItemsList();
            }
        }

        /// <summary>
        /// Fills allItemsList from the assigned ItemDB.
        /// </summary>
        public void PopulateAllItemsList()
        {
            allItemsList = itemDB.items;
        }

        /// <summary>
        /// Instantiates slots and number labels under the quickAccessGrid.
        /// </summary>
        private void SetupQuickAccessBar()
        {
            // Clear any existing children
            foreach (Transform child in quickAccessGrid)
                Destroy(child.gameObject);
            quickAccessSlots.Clear();

            // Create slots
            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObject = Instantiate(slotPrefab, quickAccessGrid);
                InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.slot.SetTransformProperties();
                    quickAccessSlots.Add(slotUI.slot);

                    var dragHandler = slotObject.transform
                        .Find("DraggableItem")?
                        .GetComponent<InventoryDragHandler>();
                    if (dragHandler != null)
                        dragHandler.slot = slotUI.slot;

                    // Create and position the button number text
                    GameObject btnTextObj = Instantiate(buttonNumberTextPrefab, slotObject.transform);
                    Text btnText = btnTextObj.GetComponent<Text>();
                    if (btnText != null)
                    {
                        btnText.text = (i < 9) ? (i + 1).ToString() : "0";
                        var rt = btnTextObj.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0);
                        rt.anchorMax = new Vector2(0.5f, 0);
                        rt.pivot     = new Vector2(0.5f, 1);
                        rt.anchoredPosition = new Vector2(0, 8);
                    }
                }
                else
                {
                    Debug.LogError($"QuickAccessBar: InventorySlotUI component missing on slot prefab at index {i}.");
                }
            }
        }

        private void Update()
        {
            // Check number key presses
            for (int i = 0; i < totalSlots; i++)
            {
                if ((i < 9 && Input.GetKeyDown(KeyCode.Alpha1 + i)) ||
                    (i == 9 && Input.GetKeyDown(KeyCode.Alpha0)))
                {
                    UseItemInSlot(i);
                }
            }
        }

        /// <summary>
        /// Uses the item in the specified slot, if any.
        /// </summary>
        public void UseItemInSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= quickAccessSlots.Count)
            {
                Debug.LogError("QuickAccessBar: Invalid slot index");
                return;
            }

            var slot = quickAccessSlots[slotIndex];
            if (slot.item != null)
                slot.UseItem();
            else
                Debug.LogWarning($"QuickAccessBar: Slot {slotIndex} is empty.");
        }

        /// <summary>
        /// Adds an item to the first available slot.
        /// </summary>
        public void AddItemToQuickAccessBar(InventoryItem item, int quantity = 1)
        {
            foreach (var slot in quickAccessSlots)
            {
                if (slot.item == null)
                {
                    slot.SetItem(item, quantity);
                    return;
                }
            }
            Debug.LogWarning("QuickAccessBar: Quick access bar is full!");
        }

        /// <summary>
        /// Collects the current slots’ data for saving.
        /// </summary>
        public List<InventoryItemData> GetItems()
        {
            var items = new List<InventoryItemData>();
            foreach (var slot in quickAccessSlots)
            {
                if (slot.item != null)
                {
                    items.Add(new InventoryItemData {
                        itemName = slot.item.itemName,
                        quantity = slot.quantity
                    });
                    Debug.Log($"QuickAccessBar: Saved {slot.item.itemName} x{slot.quantity}");
                }
            }
            return items;
        }

        /// <summary>
        /// Loads saved data back into the slots.
        /// </summary>
        public void LoadItems(List<InventoryItemData> items)
        {
            foreach (var itemData in items)
            {
                var item = FindItemByName(itemData.itemName);
                if (item != null)
                {
                    AddItemToQuickAccessBar(item, itemData.quantity);
                    Debug.Log($"QuickAccessBar: Loaded {item.itemName} x{itemData.quantity}");
                }
                else
                {
                    Debug.LogWarning($"QuickAccessBar: Item '{itemData.itemName}' not found in allItemsList.");
                }
            }
        }

        /// <summary>
        /// Clears all slots.
        /// </summary>
        public void ClearItems()
        {
            foreach (var slot in quickAccessSlots)
                slot.SetItem(null, 0);
        }

        /// <summary>
        /// Helper to find an item in allItemsList by name.
        /// </summary>
        private InventoryItem FindItemByName(string itemName)
        {
            return allItemsList.Find(item => item.itemName == itemName);
        }
    }
}
