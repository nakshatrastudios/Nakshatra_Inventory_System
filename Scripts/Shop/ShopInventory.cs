// Assets/NakshatraStudios/InventorySystem/Scripts/Shop/ShopInventory.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nakshatra.Plugins
{
    public class ShopInventory : MonoBehaviour
    {
        public static ShopInventory CurrentShop { get; private set; }

        [Header("Shop Settings")]
        public string shopName;
        public Sprite backgroundSprite;

        [Header("Pricing")]
        [Range(0f, 1f)]
        [Tooltip("Fraction of shop price returned when selling back")]
        public float sellPercentage = 0.75f;

        [System.Serializable]
        public struct ShopItemEntry
        {
            public InventoryItem item;
            public int           quantity;
            public bool          infinite;
            public string        currencyType; // overwritten at runtime from item.currencyType
            public int           price;        // in lowest-unit
        }

        [Header("Items the Shop Sells")]
        public List<ShopItemEntry> manualItems = new List<ShopItemEntry>();

        [Header("UI References")]
        public GameObject    rowPrefab;
        public RectTransform shopContent;
        public RectTransform invContent;

        private List<InventoryItem> _lastItems      = new List<InventoryItem>();
        private List<int>           _lastQuantities = new List<int>();
        private bool _built = false;

        void Awake()  => CurrentShop = this;
        void OnDestroy() { if (CurrentShop == this) CurrentShop = null; }

        void Start()
        {
            if (!_built)
            {
                BuildUI();
                CaptureInventorySnapshot();
                _built = true;
            }
        }

        void Update()
        {
            var inv = FindObjectOfType<Inventory>();
            if (inv != null && HasInventoryChanged(inv))
            {
                RefreshInventoryPanel();
                CaptureInventorySnapshot();
            }
        }

        /// <summary>
        /// Clears both panels and repopulates them.
        /// </summary>
        public void BuildUI()
        {
            ClearChildren(shopContent);
            ClearChildren(invContent);

            // 1) Shop side: enforce currencyType from the item asset
            var shopEntries = manualItems
                .Where(m => m.item != null)
                .Select(m => {
                    var e = m;
                    e.currencyType = m.item.currencyType;
                    return e;
                })
                .ToList();
            Populate(shopContent, shopEntries, true);

            // 2) Inventory side
            PopulateInventory();
        }

        /// <summary>
        /// Clears & rebuilds only the inventory (right) panel.
        /// </summary>
        public void RefreshInventoryPanel()
        {
            ClearChildren(invContent);
            PopulateInventory();
        }

        /// <summary>
        /// Helper to destroy all existing child rows.
        /// </summary>
        private void ClearChildren(RectTransform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroyImmediate(parent.GetChild(i).gameObject);
        }

        /// <summary>
        /// Instantiates one row prefab per entry.
        /// </summary>
        private void Populate(RectTransform parent, List<ShopItemEntry> entries, bool isShop)
        {
            if (parent == null || rowPrefab == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                var go = Instantiate(rowPrefab, parent);
                go.GetComponent<ShopRowUI>().Setup(entries[i], isShop, i);
            }
        }

        /// <summary>
        /// Gathers both curated and fallback items for the inventory panel.
        /// </summary>
        private void PopulateInventory()
        {
            var inv = FindObjectOfType<Inventory>();
            if (inv?.inventorySlots == null) return;

            var invEntries = new List<ShopItemEntry>();

            // curated items
            foreach (var m in manualItems)
            {
                if (m.item == null) continue;
                int totalQty = inv.inventorySlots.Where(s => s.item == m.item).Sum(s => s.quantity);
                if (totalQty <= 0) continue;

                int sellPrice = Mathf.RoundToInt(m.price * sellPercentage);
                invEntries.Add(new ShopItemEntry
                {
                    item         = m.item,
                    quantity     = totalQty,
                    infinite     = false,
                    currencyType = m.item.currencyType,
                    price        = sellPrice
                });
            }

            // fallback: any other item
            foreach (var slot in inv.inventorySlots)
            {
                var it = slot.item;
                if (it == null) continue;
                if (manualItems.Any(m => m.item == it)) continue;

                int totalQty = inv.inventorySlots.Where(s => s.item == it).Sum(s => s.quantity);
                if (totalQty <= 0) continue;

                int sellPrice = Mathf.RoundToInt(it.basePrice * sellPercentage);
                invEntries.Add(new ShopItemEntry
                {
                    item         = it,
                    quantity     = totalQty,
                    infinite     = false,
                    currencyType = it.currencyType,
                    price        = sellPrice
                });
            }

            Populate(invContent, invEntries, false);
        }

        /// <summary>
        /// Called after a purchase: clamps finite stock at zero.
        /// </summary>
        public void UpdateEntry(int idx, ShopItemEntry updated)
        {
            if (idx >= 0 && idx < manualItems.Count)
            {
                updated.quantity = Mathf.Max(0, updated.quantity);
                manualItems[idx] = updated;
            }
        }

        /// <summary>
        /// Called after a sale: restocks finite items.
        /// </summary>
        public void Restock(InventoryItem item, int qty)
        {
            int i = manualItems.FindIndex(e => e.item == item);
            if (i < 0) return;
            var e = manualItems[i];
            if (!e.infinite) e.quantity += qty;
            manualItems[i] = e;
        }

        private void CaptureInventorySnapshot()
        {
            _lastItems.Clear();
            _lastQuantities.Clear();
            var inv = FindObjectOfType<Inventory>();
            if (inv == null) return;
            foreach (var s in inv.inventorySlots)
            {
                _lastItems.Add(s.item);
                _lastQuantities.Add(s.quantity);
            }
        }

        private bool HasInventoryChanged(Inventory inv)
        {
            if (inv.inventorySlots.Count != _lastItems.Count) return true;
            for (int i = 0; i < inv.inventorySlots.Count; i++)
            {
                var s = inv.inventorySlots[i];
                if (s.item != _lastItems[i] || s.quantity != _lastQuantities[i])
                    return true;
            }
            return false;
        }
    }
}
