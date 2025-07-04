using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    [RequireComponent(typeof(ChestController))]
    public class ChestInventory : MonoBehaviour
    {
        [Header("Chest Identification")]
        public string chestName = "New Chest";

        [Header("Slot Configuration")]
        public int rows = 4;
        public int columns = 5;
        public int totalSlots = 20;
        public GameObject slotPrefab;
        public Transform chestGrid;
        public Button nextPageButton;
        public Button previousPageButton;

        [Header("Loot Settings")]
        public ItemDB itemDB;
        private List<InventoryItem> allItemsList;

        [Header("Randomization Settings")]
        public bool randomizeContents = false;
        public int minRandomDrops = 1;
        public int maxRandomDrops = 5;

        [System.Serializable]
        public struct RandomItemEntry
        {
            public InventoryItem item;
            public int quantity;
            [Range(0f,1f)] public float dropProbability;
        }

        [System.Serializable]
        public struct RandomCurrencyEntry
        {
            public string currencyName;
            public int minAmount;
            public int maxAmount;
        }

        [Header("Random Loot List")]
        public List<RandomItemEntry> randomItemEntries = new List<RandomItemEntry>();
        public List<RandomCurrencyEntry> randomCurrencyEntries = new List<RandomCurrencyEntry>();

        //[Header("Manual Contents (if not randomized)")]
        [System.Serializable]
        public struct ChestItemEntry
        {
            public InventoryItem item;
            public int quantity;
        }
        public List<ChestItemEntry> manualItems = new List<ChestItemEntry>();

        [Header("Runtime State")]
        public List<InventorySlot> chestSlots = new List<InventorySlot>();
        private int currentPage = 0;
        private int pages;

        private CurrencyManager currencyManager;
        private ChestController chestController;
        private bool currencyDispensed = false;

        void Awake()
        {
            if (itemDB != null)
                allItemsList = itemDB.items;
            currencyManager = FindObjectOfType<CurrencyManager>();
            chestController = GetComponent<ChestController>();
        }

        void Start()
        {
            SetupChestUI();
            if (nextPageButton != null)     nextPageButton.onClick.AddListener(NextPage);
            if (previousPageButton != null) previousPageButton.onClick.AddListener(PreviousPage);

            if (randomizeContents)
                PrepareRandomItems();
            else
                PopulateManual();
        }

        private void SetupChestUI()
        {
            foreach (Transform t in chestGrid) Destroy(t.gameObject);
            chestSlots.Clear();
            pages = Mathf.CeilToInt((float) totalSlots / (rows * columns));

            for (int i = 0; i < totalSlots; i++)
            {
                var go = Instantiate(slotPrefab, chestGrid);
                var ui = go.GetComponent<InventorySlotUI>();
                ui.slot.SetTransformProperties();
                chestSlots.Add(ui.slot);

                var drag = go.transform.Find("DraggableItem")?.GetComponent<InventoryDragHandler>();
                if (drag != null) drag.slot = ui.slot;
            }
            UpdatePage();
        }

        private void PrepareRandomItems()
        {
            var prepared = new List<InventoryItem>();
            foreach (var e in randomItemEntries)
            {
                for (int i = 0; i < e.quantity; i++)
                    if (Random.value <= e.dropProbability)
                        prepared.Add(e.item);
            }

            int target = Random.Range(minRandomDrops, maxRandomDrops + 1);
            if (prepared.Count > target)
                prepared = prepared.OrderBy(_ => Random.value).Take(target).ToList();
            else if (prepared.Count < target && randomItemEntries.Count > 0)
            {
                while (prepared.Count < target)
                {
                    var e = randomItemEntries[Random.Range(0, randomItemEntries.Count)];
                    prepared.Add(e.item);
                }
            }

            foreach (var it in prepared)
                AddItem(it, 1);
        }

        private void PopulateManual()
        {
            foreach (var e in manualItems)
                if (e.item != null && e.quantity > 0)
                    AddItem(e.item, e.quantity);
        }

        public void DispenseCurrency()
        {
            if (!randomizeContents || currencyManager == null || currencyDispensed) return;
            currencyDispensed = true;
            foreach (var c in randomCurrencyEntries)
            {
                if (!string.IsNullOrEmpty(c.currencyName) && c.maxAmount >= c.minAmount)
                {
                    int amt = Random.Range(c.minAmount, c.maxAmount + 1);
                    currencyManager.AddCurrency(c.currencyName, amt);
                }
            }
        }

        public void AddItem(InventoryItem item, int quantity = 1)
        {
            int remaining = quantity;
            foreach (var slot in chestSlots)
            {
                if (slot.item == item && slot.quantity < item.maxStackSize)
                {
                    int canAdd = Mathf.Min(remaining, item.maxStackSize - slot.quantity);
                    slot.quantity += canAdd;
                    slot.stackText.text = slot.quantity.ToString();
                    remaining -= canAdd;
                    if (remaining <= 0) return;
                }
            }
            foreach (var slot in chestSlots)
            {
                if (slot.item == null)
                {
                    int toAdd = Mathf.Min(remaining, item.maxStackSize);
                    slot.SetItem(item, toAdd);
                    slot.SetTransformProperties();
                    remaining -= toAdd;
                    if (remaining <= 0) return;
                }
            }
            if (remaining > 0)
                Debug.LogWarning($"Chest '{chestName}' full; {remaining} '{item.itemName}' dropped.");
        }

        public void ClearItems()
        {
            foreach (var slot in chestSlots)
                slot.SetItem(null, 0);
        }

        private void NextPage()
        {
            if (currentPage < pages - 1) currentPage++;
            UpdatePage();
        }

        private void PreviousPage()
        {
            if (currentPage > 0) currentPage--;
            UpdatePage();
        }

        private void UpdatePage()
        {
            int perPage = rows * columns;
            int start   = currentPage * perPage;
            int end     = Mathf.Min(start + perPage, chestSlots.Count);

            for (int i = 0; i < chestSlots.Count; i++)
                chestSlots[i].slotObject.SetActive(i >= start && i < end);

            if (nextPageButton != null)     nextPageButton.interactable     = currentPage < pages - 1;
            if (previousPageButton != null) previousPageButton.interactable = currentPage > 0;
        }

        public List<InventoryItemData> GetItems()
        {
            var list = new List<InventoryItemData>();
            foreach (var s in chestSlots)
                if (s.item != null)
                    list.Add(new InventoryItemData { itemName = s.item.itemName, quantity = s.quantity });
            return list;
        }

        public void LoadItems(List<InventoryItemData> items)
        {
            ClearItems();
            foreach (var d in items)
            {
                var it = allItemsList.Find(x => x.itemName == d.itemName);
                if (it != null) AddItem(it, d.quantity);
                else Debug.LogWarning($"Chest '{chestName}': saved '{d.itemName}' not found.");
            }
        }
    }
}
