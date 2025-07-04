using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class ItemActionUI : MonoBehaviour
    {
        public Button useButton;
        public Button equipButton;
        public Button unequipButton;
        public Button dropButton;
        public Button splitButton;

        private InventorySlotUI parentSlotUI;
        private Inventory playerInventory;
        private Equipment playerEquipment;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
                playerEquipment = player.GetComponent<Equipment>();
                if (playerInventory == null)
                {
                    Debug.LogError("Inventory component not found on player.");
                }
                if (playerEquipment == null)
                {
                    Debug.LogError("Equipment component not found on player.");
                }
            }
            else
            {
                Debug.LogError("Player GameObject with tag 'Player' not found.");
            }
        }

        public void ConfigureButtons(InventoryItem item, InventorySlotUI slotUI, Equipment equipment)
        {
            parentSlotUI = slotUI;

            useButton.gameObject.SetActive(item.itemType == ItemType.Consumable);
            equipButton.gameObject.SetActive(item.itemType == ItemType.Equipment && !slotUI.isEquipmentSlot);
            unequipButton.gameObject.SetActive(slotUI.isEquipmentSlot);
            dropButton.gameObject.SetActive(!slotUI.isEquipmentSlot); // Drop button is always enabled
            splitButton.gameObject.SetActive(false);

            useButton.onClick.RemoveAllListeners();
            equipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.RemoveAllListeners();
            dropButton.onClick.RemoveAllListeners();

            useButton.onClick.AddListener(() => UseItem(item, slotUI.slot));
            equipButton.onClick.AddListener(() => EquipItem(item, slotUI.slot));
            unequipButton.onClick.AddListener(() => UnequipItem(item, slotUI.slot));
            dropButton.onClick.AddListener(() => DropItem(item, slotUI.slot));
        }

        private void UseItem(InventoryItem item, InventorySlot slot)
        {
            Debug.Log($"Using {item.itemName}");
            ApplyItemEffects(item);

            if (item.onEquipSound != null)
            {
                // You can choose PlayOneShot on the player AudioSource or a one‐shot
                // at the main camera. Here’s a simple PlayClipAtPoint:
                AudioSource.PlayClipAtPoint(
                    item.onEquipSound,
                    Camera.main.transform.position
                );
            }
            if (playerInventory != null)
            {
                playerInventory.RemoveItemFromSlot(slot, 1); // Remove the used item from the specific slot
            }
            CloseActionUI();
        }

        private void EquipItem(InventoryItem item, InventorySlot slot)
        {
            Debug.Log($"Equipping {item.itemName}");
            if (playerEquipment != null)
            {
                playerEquipment.EquipItem(item);
            }
            if (playerInventory != null)
            {
                playerInventory.RemoveItemFromSlot(slot, 1); // Remove the equipped item from the specific slot
            }
            CloseActionUI();
        }

        private void UnequipItem(InventoryItem item, InventorySlot slot)
        {
            Debug.Log($"Unequipping {item.itemName}");
            if (playerEquipment != null)
            {
                playerEquipment.UnequipItem(item);
            }
            CloseActionUI();
        }

        private void DropItem(InventoryItem item, InventorySlot slot)
        {
            Debug.Log($"Dropping {item.itemName}");
            // Implement item dropping logic here
            if (playerInventory != null)
            {
                playerInventory.RemoveItemFromSlot(slot, 1); // Remove the dropped item from the specific slot
            }
            CloseActionUI();
        }

        private void ApplyItemEffects(InventoryItem item)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    foreach (var stat in item.stats)
                    {
                        switch (stat.statType)
                        {
                            case StatType.Health:
                                playerStatus.Health = Mathf.Min(playerStatus.Health + stat.value, playerStatus.MaxHealth);
                                break;
                            case StatType.Mana:
                                playerStatus.Mana = Mathf.Min(playerStatus.Mana + stat.value, playerStatus.MaxMana);
                                break;
                            case StatType.Stamina:
                                playerStatus.Stamina = Mathf.Min(playerStatus.Stamina + stat.value, playerStatus.MaxStamina);
                                break;
                            default:
                                playerStatus.AddStat(stat.statType, stat.value);
                                break;
                        }
                    }
                    Debug.Log("Item effects applied.");
                }
                else
                {
                    Debug.LogError("PlayerStatus component not found on player.");
                }
            }
            else
            {
                Debug.LogError("Player GameObject with tag 'Player' not found.");
            }
        }

        public void CloseActionUI()
        {
            if (parentSlotUI != null)
            {
                parentSlotUI.CloseItemActionUI();
            }
        }
    }
}
