using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    /// <summary>
    /// Manages a list of currencies with multi-tier conversion and on-screen UI.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        [System.Serializable]
        public class Currency
        {
            public string name;
            public Sprite icon;
            public int amount;
            public Text currencyText;
            public int conversionRate;
        }

        [Tooltip("Define currencies in descending order (highest value first).")]
        public List<Currency> currencies = new List<Currency>();

        private void Start()
        {
            // Initialize on-screen text
            foreach (var currency in currencies)
            {
                if (currency.currencyText != null)
                    currency.currencyText.text = currency.amount.ToString();
                else
                    Debug.LogError($"Currency text for '{currency.name}' is not assigned.");
            }
        }

        /// <summary>
        /// Add the specified amount to a currency, rolling over to higher tiers.
        /// </summary>
        public void AddCurrency(string currencyName, int amount)
        {
            var currency = currencies.Find(c => c.name == currencyName);
            if (currency == null)
            {
                Debug.LogError($"Currency '{currencyName}' not found.");
                return;
            }

            if (currency.conversionRate <= 0)
            {
                Debug.LogError($"Conversion rate for '{currency.name}' must be > 0.");
                return;
            }

            // Combine and split into remainder + converted higher-tier units
            int totalAmount     = currency.amount + amount;
            int convertedAmount = totalAmount / currency.conversionRate;
            int remainder       = totalAmount % currency.conversionRate;

            currency.amount = remainder;
            UpdateCurrencyText(currency);
            Debug.Log($"{currency.name} updated to {currency.amount}");

            // Roll over to next higher tier if any
            int idx = currencies.IndexOf(currency);
            if (convertedAmount > 0 && idx > 0)
            {
                var higher = currencies[idx - 1];
                Debug.Log($"Rolling over {convertedAmount * currency.conversionRate} {currency.name} into {convertedAmount} {higher.name}");
                AddCurrency(higher.name, convertedAmount);
            }
        }

        /// <summary>
        /// Returns the current amount of the specified currency.
        /// </summary>
        public int GetCurrencyAmount(string currencyName)
        {
            var currency = currencies.Find(c => c.name == currencyName);
            return currency != null ? currency.amount : 0;
        }

        private void UpdateCurrencyText(Currency currency)
        {
            if (currency.currencyText != null)
                currency.currencyText.text = currency.amount.ToString();
            else
                Debug.LogError($"Currency text for '{currency.name}' is not assigned.");
        }

        /// <summary>
        /// Returns a serializable snapshot of all currency amounts.
        /// </summary>
        public List<CurrencyData> GetCurrencyData()
        {
            var list = new List<CurrencyData>();
            foreach (var c in currencies)
                list.Add(new CurrencyData { name = c.name, amount = c.amount });
            return list;
        }

        /// <summary>
        /// Restores currency amounts from saved data and updates the UI.
        /// </summary>
        public void SetCurrencyData(List<CurrencyData> data)
        {
            foreach (var d in data)
            {
                var c = currencies.Find(x => x.name == d.name);
                if (c != null)
                {
                    c.amount = d.amount;
                    UpdateCurrencyText(c);
                }
            }
        }
    }

    /// <summary>
    /// Serializable structure representing a single currency's name & amount for saving/loading.
    /// </summary>
    [System.Serializable]
    public class CurrencyData
    {
        public string name;
        public int    amount;
    }

}
