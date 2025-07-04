using UnityEngine;

namespace Nakshatra.Plugins
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item & UI")]
        [Tooltip("The item to add when the player presses the pickup key")]
        public InventoryItem item;  // Assign this in the Inspector

        [Tooltip("A prefab for the floating \"Press [Key] to Pickup\" text")]
        public GameObject pickupTextPrefab;  // Assign the PickupText prefab in the Inspector

        [Header("Pickup Settings")]
        [Tooltip("Key used to pick up the item when in range")]
        public KeyCode pickupKey = KeyCode.F;
        private GameObject pickupTextInstance;
        private bool playerInRange;

        private void OnTriggerEnter(Collider other)         => HandleCollisionEnter(other.gameObject);
        private void OnTriggerEnter2D(Collider2D other)     => HandleCollisionEnter(other.gameObject);
        private void OnTriggerExit(Collider other)          => HandleCollisionExit(other.gameObject);
        private void OnTriggerExit2D(Collider2D other)      => HandleCollisionExit(other.gameObject);

        private void HandleCollisionEnter(GameObject other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                if (pickupTextPrefab != null && pickupTextInstance == null)
                {
                    pickupTextInstance = Instantiate(pickupTextPrefab, transform.position, Quaternion.identity);
                    pickupTextInstance.SetActive(true);
                }
            }
        }

        private void HandleCollisionExit(GameObject other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                if (pickupTextInstance != null)
                {
                    Destroy(pickupTextInstance);
                    pickupTextInstance = null;
                }
            }
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(pickupKey))
            {
                // 1) Play pickup sound
                if (item.pickupSound != null)
                    AudioSource.PlayClipAtPoint(
                        item.pickupSound,
                        Camera.main.transform.position
                    );
                // Access the player's inventory (assumes player has an Inventory component)
                Inventory playerInventory = GameObject.FindWithTag("Player").GetComponent<Inventory>();
                if (playerInventory != null)
                {
                    // Add the item to the player's inventory
                    playerInventory.AddItem(item, 1);
                    // Destroy the pickup text and the item pickup GameObject
                    if (pickupTextInstance != null)
                    {
                        Destroy(pickupTextInstance);
                    }
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning("Player does not have an Inventory component");
                }
            }
        }
    }
}