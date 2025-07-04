using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    

    /// <summary>
    /// Attach to any GameObject.  
    /// Monitors the specified EquipmentCategory slots on your Player's Equipment
    /// and toggles two lists of GameObjects on equip/unequip.
    /// </summary>
    public class EquipmentSlotsDisabler : MonoBehaviour
    {
        [Tooltip("Your Player's Equipment component. Auto-finds tag 'Player' if empty.")]
        public Equipment equipment;

        [Tooltip("One entry per EquipmentCategory to watch.")]
        public List<SlotToggleEntry> entries = new List<SlotToggleEntry>();

        void Start()
        {
            if (equipment == null)
                equipment = GameObject.FindWithTag("Player")?.GetComponent<Equipment>();
            if (equipment == null)
                Debug.LogError("EquipmentSlotsDisabler: No Equipment component found on tagged 'Player'.");

            // Initialize each entry’s state and apply initial toggles
            foreach (var e in entries)
            {
                e.wasEquipped = IsEquipped(e.slotCategory);
                ApplyEntry(e, e.wasEquipped);
            }
        }

        void Update()
        {
            foreach (var e in entries)
            {
                bool nowEquipped = IsEquipped(e.slotCategory);
                if (nowEquipped != e.wasEquipped)
                {
                    ApplyEntry(e, nowEquipped);
                    e.wasEquipped = nowEquipped;
                }
            }
        }

        private void ApplyEntry(SlotToggleEntry entry, bool isEquipped)
        {
            // On equip: disable list1, enable list2
            // On unequip: inverse
            SetActiveList(entry.objectsToDisable,   !isEquipped);
            SetActiveList(entry.objectsToEnable,     isEquipped);
        }

        private void SetActiveList(List<GameObject> list, bool active)
        {
            foreach (var go in list)
                if (go != null && go.activeSelf != active)
                    go.SetActive(active);
        }

        private bool IsEquipped(EquipmentCategory cat)
        {
            if (equipment == null) return false;

            switch (cat)
            {
                case EquipmentCategory.Helmet:
                    return equipment.helmetSlot.item != null;
                case EquipmentCategory.Shoulder:
                    return equipment.shoulderSlot.item != null;
                case EquipmentCategory.Torso:
                    return equipment.torsoSlot.item != null;
                case EquipmentCategory.Pants:
                    return equipment.pantsSlot.item != null;
                case EquipmentCategory.Gloves:
                    return equipment.glovesSlot.item != null;
                case EquipmentCategory.Boots:
                    return equipment.bootsSlot.item != null;
                case EquipmentCategory.Back:      // Cloak slot
                    return equipment.cloakSlot.item != null;
                case EquipmentCategory.Necklace:
                    return equipment.neckSlot.item != null;
                case EquipmentCategory.Earrings:
                    return equipment.earRingSlot.item != null;
                case EquipmentCategory.Belt:
                    return equipment.beltSlot.item != null;
                case EquipmentCategory.Rings:
                    // Treat any ring as “equipped”
                    return equipment.ring1Slot.item != null
                        || equipment.ring2Slot.item != null;
                case EquipmentCategory.Weapon:
                    // Any weapon (main or off)
                    return equipment.mainHandSlot.item != null
                        || equipment.offHandSlot.item != null;
                default:
                    return false;
            }
        }
    }

    [System.Serializable]
    public class SlotToggleEntry
    {
        [Tooltip("Which equipment slot to watch")]
        public EquipmentCategory slotCategory;

        [Tooltip("GameObjects to DISABLE when this slot is equipped")]
        public List<GameObject> objectsToDisable = new List<GameObject>();

        [Tooltip("GameObjects to ENABLE when this slot is equipped")]
        public List<GameObject> objectsToEnable = new List<GameObject>();

        [HideInInspector] 
        public bool wasEquipped;
    }
}
