// Assets/NakshatraStudios/InventorySystem/Scripts/Shop/ShopRowUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins
{
    [RequireComponent(typeof(RectTransform))]
    public class ShopRowUI : MonoBehaviour
    {
        [Header("Prefab References")]
        public Text       nameText;
        public Image      iconImage;
        public Text       quantityText;
        public Text       priceText;
        public InputField qtyField;
        public Button     minusButton;
        public Button     plusButton;
        public Button     actionButton;

        private ShopInventory.ShopItemEntry _entry;
        private bool  _isShop;
        private int   _shopIndex;
        private int   _maxQuantity;

        public void Setup(ShopInventory.ShopItemEntry entry, bool isShop, int shopIndex)
        {
            _entry     = entry;
            _isShop    = isShop;
            _shopIndex = shopIndex;

            // Name & Icon
            nameText.text                     = entry.item.itemName;
            iconImage.sprite                  = entry.item.itemIcon;
            iconImage.rectTransform.sizeDelta = new Vector2(65, 65);

            // Quantity display
            quantityText.text = _isShop
                ? (entry.infinite ? "∞" : entry.quantity.ToString())
                : entry.quantity.ToString();

            // Price display
            priceText.gameObject.SetActive(true);
            FormatPrice(entry.price, entry.item.currencyType);

            // Determine max selectable quantity
            _maxQuantity = _isShop
                ? (entry.infinite ? int.MaxValue : entry.quantity)
                : entry.quantity;
            _maxQuantity = Mathf.Max(0, _maxQuantity);

            // Enable/disable action button
            actionButton.interactable = _isShop
                ? (entry.infinite || entry.quantity > 0)
                : true;

            // Qty selector init
            qtyField.transform.parent.gameObject.SetActive(true);
            qtyField.text = "1";

            // Wire up UI events
            minusButton.onClick.RemoveAllListeners();
            plusButton .onClick.RemoveAllListeners();
            qtyField.onEndEdit.RemoveAllListeners();
            actionButton.onClick.RemoveAllListeners();

            minusButton.onClick.AddListener(OnMinus);
            plusButton .onClick.AddListener(OnPlus);
            qtyField.onEndEdit.AddListener(OnQuantityEdited);
            actionButton.onClick.AddListener(OnAction);

            actionButton.GetComponentInChildren<Text>().text = _isShop ? "Buy" : "Sell";
        }

        private void FormatPrice(int amountInTier, string currencyType)
        {
            var cm = FindObjectOfType<CurrencyManager>();
            if (cm == null || cm.currencies == null || cm.currencies.Count == 0)
            {
                // fallback to raw
                priceText.text = $"{amountInTier} {currencyType}";
                return;
            }

            var tiers = cm.currencies; // highest-tier first
            int n = tiers.Count;

            // compute factor: lowest-unit per one of each tier
            var factor = new int[n];
            factor[n - 1] = 1;
            for (int i = n - 2; i >= 0; i--)
                factor[i] = factor[i + 1] * tiers[i + 1].conversionRate;

            // find index of selected currencyType
            int selIdx = tiers.FindIndex(c => c.name == currencyType);
            if (selIdx < 0) selIdx = n - 1;

            // compute raw lowest-unit total
            int rawLowest = amountInTier * factor[selIdx];

            // decompose into tier counts
            int rem = rawLowest;
            var parts = new List<string>();
            for (int i = 0; i < n; i++)
            {
                int cnt = rem / factor[i];
                if (cnt > 0)
                {
                    parts.Add($"{cnt} {tiers[i].name}");
                    rem %= factor[i];
                }
            }

            if (parts.Count == 0)
                parts.Add($"0 {tiers[n - 1].name}");

            priceText.text = string.Join(" ", parts);
        }

        private void OnMinus()
        {
            if (int.TryParse(qtyField.text, out int v) && v > 1)
                qtyField.text = (v - 1).ToString();
            else
                qtyField.text = "1";
        }

        private void OnPlus()
        {
            if (int.TryParse(qtyField.text, out int v))
                qtyField.text = Mathf.Min(v + 1, _maxQuantity).ToString();
            else
                qtyField.text = "1";
        }

        private void OnQuantityEdited(string input)
        {
            if (!int.TryParse(input, out int v) || v < 1) v = 1;
            if (v > _maxQuantity) v = _maxQuantity;
            qtyField.text = v.ToString();
        }

        private void OnAction()
        {
            if (!int.TryParse(qtyField.text, out int amt) || amt < 1) return;

            // clamp to available stock if not infinite
            if (!_entry.infinite && amt > _entry.quantity)
                amt = _entry.quantity;
            if (amt < 1) return;

            var cm  = FindObjectOfType<CurrencyManager>();
            var inv = FindObjectOfType<Inventory>();
            if (cm == null || inv == null) return;

            if (_isShop)
            {
                // BUY: compute cost in lowest-units
                var tiers = cm.currencies;
                int n = tiers.Count;
                var factor = new int[n];
                factor[n - 1] = 1;
                for (int i = n - 2; i >= 0; i--)
                    factor[i] = factor[i + 1] * tiers[i + 1].conversionRate;

                // find purchase tier index
                int selIdx = tiers.FindIndex(c => c.name == _entry.item.currencyType);
                if (selIdx < 0) selIdx = n - 1;

                int costLowest = Mathf.CeilToInt(_entry.price * amt * factor[selIdx]);

                // sum player funds
                int totalLowest = 0;
                for (int i = 0; i < n; i++)
                    totalLowest += tiers[i].amount * factor[i];
                if (totalLowest < costLowest) return;

                // compute remainder
                int remain = totalLowest - costLowest;

                // rebuild wallet
                var newData = new List<CurrencyData>();
                for (int i = 0; i < n; i++)
                {
                    int cnt = remain / factor[i];
                    remain -= cnt * factor[i];
                    newData.Add(new CurrencyData { name = tiers[i].name, amount = cnt });
                }
                cm.SetCurrencyData(newData);

                // grant item
                inv.AddItem(_entry.item, amt);

                // reduce shop stock
                if (!_entry.infinite)
                {
                    _entry.quantity -= amt;
                    ShopInventory.CurrentShop.UpdateEntry(_shopIndex, _entry);

                    // update UI to reflect new stock
                    quantityText.text = _entry.quantity > 0 ? _entry.quantity.ToString() : "0";
                    _maxQuantity = _entry.quantity;
                    actionButton.interactable = _entry.infinite || _entry.quantity > 0;
                    qtyField.text = "1";
                }
            }
            else
            {
                // SELL: remove items
                for (int i = 0; i < amt; i++)
                    inv.RemoveItem(_entry.item, 1);

                // refund in lowest-units
                int refundLowest = _entry.price * amt;
                cm.AddCurrency(_entry.item.currencyType, refundLowest);

                // restock shop
                ShopInventory.CurrentShop.Restock(_entry.item, amt);
            }

            // refresh only inventory panel
            ShopInventory.CurrentShop.RefreshInventoryPanel();
        }
    }
}
