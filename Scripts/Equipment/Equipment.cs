using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    public class Equipment : MonoBehaviour
    {
        public InventorySlot helmetSlot;
        public InventorySlot shoulderSlot;
        public InventorySlot torsoSlot;
        public InventorySlot pantsSlot;
        public InventorySlot glovesSlot;
        public InventorySlot bootsSlot;
        public InventorySlot cloakSlot;
        public InventorySlot neckSlot;
        public InventorySlot earRingSlot;
        public InventorySlot ring1Slot;
        public InventorySlot ring2Slot;
        public InventorySlot beltSlot;
        public InventorySlot mainHandSlot;
        public InventorySlot offHandSlot;

        private Inventory inventory;
        private PlayerStatus playerStatus;
        private Dictionary<InventoryItem, List<GameObject>> equippedItemInstances = new Dictionary<InventoryItem, List<GameObject>>();
        public List<InventoryItem> allItemsList; // Add this to your script to hold all possible items
        public ItemDB itemDB; // Reference to ItemDB

        public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>(); // Add this line
        private AudioSource audioSource;

        private void Awake()
        {
            inventory = FindObjectOfType<Inventory>();
            playerStatus = FindObjectOfType<PlayerStatus>();

            if (inventory == null)
            {
                Debug.LogError("Inventory component not found in the scene.");
            }

            if (playerStatus == null)
            {
                Debug.LogError("PlayerStatus component not found in the scene.");
            }

            if (itemDB != null)
            {
                PopulateAllItemsList();
            }

            InitializeEquipmentSlots();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PopulateAllItemsList()
        {
            allItemsList = itemDB.items;
        }

        private void UnequipAllItems()
        {
            foreach (var slot in GetAllEquipmentSlots())
            {
                if (slot.item != null)
                {
                    UnequipItem(slot.item);
                }
            }
        }

        private void InitializeEquipmentSlots()
        {
            foreach (var slot in GetAllEquipmentSlots())
            {
                slot.SetItem(null, 0); // Initialize each slot
            }
        }

        public bool IsItemEquipped(InventoryItem item)
        {
            foreach (var slot in GetAllEquipmentSlots())
            {
                if (slot.item == item)
                {
                    return true;
                }
            }
            return false;
        }

        public void EquipItem(InventoryItem item)
        {
            InventorySlot targetSlot = GetTargetSlot(item);

            if (targetSlot != null)
            {
                if (targetSlot.item != null)
                {
                    UnequipItem(targetSlot.item);
                }

                if (item.weaponType == WeaponType.TwoHand)
                {
                    // Unequip off-hand item if equipping a two-handed weapon
                    if (offHandSlot.item != null)
                    {
                        UnequipItem(offHandSlot.item);
                    }

                    // Display the two-handed weapon in both main-hand and off-hand slots
                    mainHandSlot.SetItem(item, 1);
                    offHandSlot.SetItem(item, 1);
                }
                else if (item.isMainHand && mainHandSlot.item != null && mainHandSlot.item.weaponType == WeaponType.TwoHand)
                {
                    // Unequip two-handed weapon if equipping a one-handed weapon in the main hand
                    UnequipItem(mainHandSlot.item);
                }
                else if (item.isOffHand && mainHandSlot.item != null && mainHandSlot.item.weaponType == WeaponType.TwoHand)
                {
                    // Prevent equipping off-hand item if a two-handed weapon is equipped in the main hand
                    Debug.LogError("Cannot equip off-hand item while a two-handed weapon is equipped in the main hand.");
                    return;
                }

                if (item.weaponType != WeaponType.TwoHand)
                {
                    targetSlot.SetItem(item, 1);
                }

                if (playerStatus != null)
                {
                    playerStatus.AddStats(item.stats);
                }

                InstantiateEquippedItem(item);

                UpdateSlotUI(targetSlot);
                if (item.weaponType == WeaponType.TwoHand)
                {
                    UpdateSlotUI(offHandSlot);
                }

                Debug.Log($"Item equipped: {item.itemName} in slot: {targetSlot.slotObject.name}");
            }
            else
            {
                Debug.LogError($"No slot available for equipment category: {item.equipmentCategory}");
            }

            // SIBLING‐TOGGLE ON EQUIP
            if (item.toggleSiblings)
            {
                foreach (var pt in item.parentToggles)
                {
                    if (string.IsNullOrEmpty(pt.parentName)) continue;
                    var parentGO = GameObject.Find(pt.parentName);
                    if (parentGO == null) continue;

                    // disable all children
                    foreach (Transform child in parentGO.transform)
                        child.gameObject.SetActive(false);

                    // enable only those listed
                    foreach (var name in pt.enableOnEquip)
                    {
                        var c = parentGO.transform.Find(name);
                        if (c != null) c.gameObject.SetActive(true);
                    }
                }
            }

            if (item.onEquipSound != null)
                audioSource.PlayOneShot(item.onEquipSound);

        }

        private void InstantiateEquippedItem(InventoryItem item)
        {
            if (item.setBone)
            {
                List<GameObject> instantiatedItems = new List<GameObject>();
                for (int i = 0; i < item.selectedBoneNames.Count; i++)
                {
                    Transform bone = FindBoneByName(item.selectedBoneNames[i]);
                    if (bone != null)
                    {
                        GameObject itemInstance = Instantiate(item.itemPrefabs[i], bone);
                        itemInstance.transform.localPosition = item.itemPositions[i];
                        itemInstance.transform.localRotation = Quaternion.Euler(item.itemRotations[i]);
                        itemInstance.transform.localScale = item.itemScale[i];
                        instantiatedItems.Add(itemInstance);
                    }
                    else
                    {
                        Debug.LogError($"Bone with name {item.selectedBoneNames[i]} not found.");
                    }
                }
                equippedItemInstances[item] = instantiatedItems;
            }
        }

        public void UnequipItem(InventoryItem item)
        {
            InventorySlot targetSlot = GetEquippedSlot(item);

            if (targetSlot != null && targetSlot.item == item)
            {
                if (playerStatus != null)
                {
                    playerStatus.RemoveStats(item.stats);
                }

                targetSlot.SetItem(null, 0);
                if (item.weaponType == WeaponType.TwoHand)
                {
                    offHandSlot.SetItem(null, 0);
                }

                // Add item back to inventory
                inventory.AddItem(item, 1);

                UpdateSlotUI(targetSlot);
                if (item.weaponType == WeaponType.TwoHand)
                {
                    UpdateSlotUI(offHandSlot);
                }

                Debug.Log($"Item unequipped: {item.itemName} from slot: {targetSlot.slotObject.name}");

                // Reset the CanvasGroup properties when unequipping the item
                CanvasGroup canvasGroup = targetSlot.slotObject.GetComponentInChildren<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1;
                    canvasGroup.blocksRaycasts = true;
                }
                if (item.weaponType == WeaponType.TwoHand)
                {
                    CanvasGroup offHandCanvasGroup = offHandSlot.slotObject.GetComponentInChildren<CanvasGroup>();
                    if (offHandCanvasGroup != null)
                    {
                        offHandCanvasGroup.alpha = 1;
                        offHandCanvasGroup.blocksRaycasts = true;
                    }
                }

                // Destroy the instantiated items
                if (equippedItemInstances.ContainsKey(item))
                {
                    foreach (var instance in equippedItemInstances[item])
                    {
                        Destroy(instance);
                    }
                    equippedItemInstances.Remove(item);
                }

                // SIBLING‐TOGGLE ON UNEQUIP
                if (item.toggleSiblings)
                {
                    foreach (var pt in item.parentToggles)
                    {
                        if (string.IsNullOrEmpty(pt.parentName)) continue;
                        var parentGO = GameObject.Find(pt.parentName);
                        if (parentGO == null) continue;

                        // disable all children
                        foreach (Transform child in parentGO.transform)
                            child.gameObject.SetActive(false);

                        // enable only those listed for unequip
                        foreach (var name in pt.enableOnUnequip)
                        {
                            var c = parentGO.transform.Find(name);
                            if (c != null) c.gameObject.SetActive(true);
                        }
                    }
                }

                if (item.onUnequipSound != null)
                    audioSource.PlayOneShot(item.onUnequipSound);

            }
            else if (targetSlot != null)
            {
                Debug.LogError($"Item {item.itemName} is not equipped in the expected slot.");

            }
            else
            {
                Debug.LogError($"Item {item.itemName} is not equipped in the expected slot or no slot found.");
            }
        }

        public void ClearItems()
        {
            UnequipAllItems();
            foreach (var slot in GetAllEquipmentSlots())
            {
                slot.SetItem(null, 0);
            }
        }

        public InventorySlot GetTargetSlot(InventoryItem item)
        {
            switch (item.equipmentCategory)
            {
                case EquipmentCategory.Helmet: return helmetSlot;
                case EquipmentCategory.Shoulder: return shoulderSlot;
                case EquipmentCategory.Torso: return torsoSlot;
                case EquipmentCategory.Pants: return pantsSlot;
                case EquipmentCategory.Gloves: return glovesSlot;
                case EquipmentCategory.Boots: return bootsSlot;
                case EquipmentCategory.Back: return cloakSlot;
                case EquipmentCategory.Necklace: return neckSlot;
                case EquipmentCategory.Belt: return beltSlot;
                case EquipmentCategory.Earrings: return earRingSlot;
                case EquipmentCategory.Rings:
                    if (ring1Slot.item == null) return ring1Slot;
                    if (ring2Slot.item == null) return ring2Slot;
                    return ring1Slot; // Default to ring1Slot if both are occupied (customize as needed)
                case EquipmentCategory.Weapon:
                    if (item.weaponType == WeaponType.TwoHand) return mainHandSlot;
                    if (item.isMainHand) return mainHandSlot;
                    if (item.isOffHand) return offHandSlot;
                    break;
            }
            return null;
        }

        private InventorySlot GetEquippedSlot(InventoryItem item)
        {
            switch (item.equipmentCategory)
            {
                case EquipmentCategory.Helmet: return helmetSlot.item == item ? helmetSlot : null;
                case EquipmentCategory.Shoulder: return shoulderSlot.item == item ? shoulderSlot : null;
                case EquipmentCategory.Torso: return torsoSlot.item == item ? torsoSlot : null;
                case EquipmentCategory.Pants: return pantsSlot.item == item ? pantsSlot : null;
                case EquipmentCategory.Gloves: return glovesSlot.item == item ? glovesSlot : null;
                case EquipmentCategory.Boots: return bootsSlot.item == item ? bootsSlot : null;
                case EquipmentCategory.Back: return cloakSlot.item == item ? cloakSlot : null;
                case EquipmentCategory.Necklace: return neckSlot.item == item ? neckSlot : null;
                case EquipmentCategory.Belt: return beltSlot.item == item ? beltSlot : null;
                case EquipmentCategory.Earrings: return earRingSlot.item == item ? earRingSlot : null;
                case EquipmentCategory.Rings:
                    if (ring1Slot.item == item) return ring1Slot;
                    if (ring2Slot.item == item) return ring2Slot;
                    break;
                case EquipmentCategory.Weapon:
                    if (mainHandSlot.item == item) return mainHandSlot;
                    if (offHandSlot.item == item) return offHandSlot;
                    break;
            }
            return null;
        }

        private void UpdateSlotUI(InventorySlot slot)
        {
            InventorySlotUI slotUI = slot.slotObject.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.slot = slot;
                InventoryDragHandler dragHandler = slot.slotObject.transform.Find("DraggableItem").GetComponent<InventoryDragHandler>();
                if (dragHandler != null)
                {
                    dragHandler.slot = slot;
                }
                Debug.Log($"Updated InventorySlotUI for slot: {slot.slotObject.name}");
            }
            else
            {
                Debug.LogError("InventorySlotUI component not found on slotObject.");
            }
        }

        public IEnumerable<InventorySlot> GetAllEquipmentSlots()
        {
            yield return helmetSlot;
            yield return shoulderSlot;
            yield return torsoSlot;
            yield return pantsSlot;
            yield return glovesSlot;
            yield return bootsSlot;
            yield return cloakSlot;
            yield return neckSlot;
            yield return earRingSlot;
            yield return ring1Slot;
            yield return ring2Slot;
            yield return beltSlot;
            yield return mainHandSlot;
            yield return offHandSlot;
        }

        private Transform FindBoneByName(string boneName)
        {
            foreach (Transform bone in GetComponentsInChildren<Transform>())
            {
                if (bone.name == boneName)
                {
                    return bone;
                }
            }
            return null;
        }

        public List<InventoryItemData> GetItems()
        {
            List<InventoryItemData> items = new List<InventoryItemData>();
            foreach (var slot in GetAllEquipmentSlots())
            {
                if (slot.item != null)
                {
                    items.Add(new InventoryItemData { itemName = slot.item.itemName, quantity = 1 });
                    Debug.Log($"Saved Equipment Item: {slot.item.itemName}");
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
                    EquipItem(item);
                    Debug.Log($"Loaded Equipment Item: {item.itemName}");
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


