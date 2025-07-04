// CurrencyPickup.cs
using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    /// <summary>
    /// Handles player proximity and uses the inspector-chosen key to pick up currency.
    /// </summary>
    public class CurrencyPickup : MonoBehaviour
    {
        [System.Serializable]
        public class CurrencyAmount
        {
            [Tooltip("Currency identifier (e.g. \"Gold\", \"Coins\")")]
            public string name;
            [Tooltip("Amount of this currency to add")]
            public int amount;
        }

        [Header("Currency Amounts")]
        [Tooltip("Configure which currencies and how much to add on pickup")]
        public List<CurrencyAmount> currencyAmounts = new List<CurrencyAmount>();

        [Header("Item & UI")]
        [Tooltip("Assign the PickupText prefab for floating prompt text")]
        public GameObject pickupTextPrefab;

        [Header("Pickup Settings")]
        [Tooltip("Key used to pick up currency when in range")]
        public KeyCode pickupKey = KeyCode.E;

        private GameObject pickupTextInstance;
        private bool playerInRange;

        private void OnTriggerEnter(Collider other)         => HandleCollisionEnter(other.gameObject);
        private void OnTriggerEnter2D(Collider2D other)     => HandleCollisionEnter(other.gameObject);
        private void OnTriggerExit(Collider other)          => HandleCollisionExit(other.gameObject);
        private void OnTriggerExit2D(Collider2D other)      => HandleCollisionExit(other.gameObject);

        private void HandleCollisionEnter(GameObject other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;

            if (pickupTextPrefab != null && pickupTextInstance == null)
            {
                pickupTextInstance = Instantiate(
                    pickupTextPrefab,
                    transform.position,
                    Quaternion.identity
                );
                pickupTextInstance.SetActive(true);
            }
        }

        private void HandleCollisionExit(GameObject other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;

            if (pickupTextInstance != null)
            {
                Destroy(pickupTextInstance);
                pickupTextInstance = null;
            }
        }

        private void Update()
        {
            if (!playerInRange) return;

            if (Input.GetKeyDown(pickupKey))
            {
                var player = GameObject.FindWithTag("Player");
                var currencyManager = player?.GetComponent<CurrencyManager>();
                if (currencyManager != null)
                {
                    foreach (var amt in currencyAmounts)
                        currencyManager.AddCurrency(amt.name, amt.amount);

                    if (pickupTextInstance != null) Destroy(pickupTextInstance);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(CurrencyPickup)}] Player lacks a CurrencyManager component.", this);
                }
            }
        }
    }
}
