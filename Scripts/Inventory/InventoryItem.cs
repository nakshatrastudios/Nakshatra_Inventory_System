// Assets/NakshatraStudios/InventorySystem/Scripts/Inventory/InventoryItem.cs
using System.Collections.Generic;
using UnityEngine;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins
{
    [CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject
    {
        public string itemName = "New Item";
        public string itemDescription = "Item Description";
        public Sprite itemIcon = null;
        public bool isStackable = false;
        public int maxStackSize = 1;

        public ItemType itemType;
        public EquipmentCategory equipmentCategory;
        public int amount;

        [Header("Economy")]
        [Tooltip("Default price in lowest‐unit (Copper) for fallback sales")]
        public int basePrice = 1;
        [Tooltip("Currency tier for basePrice (matches your CurrencyManager list)")]
        public string currencyType = "Copper";

        // Weapon specific
        public WeaponType weaponType;
        public bool isMainHand;
        public bool isOffHand;

        // Dynamic stats
        public List<ItemStat> stats = new List<ItemStat>();

        // Bone attachments
        public bool setBone = false;
        public List<string> selectedBoneNames = new List<string>();
        public List<Vector3> itemPositions = new List<Vector3>();
        public List<Vector3> itemRotations = new List<Vector3>();
        public List<Vector3> itemScale = new List<Vector3>();

        // Prefabs & pickups
        public List<GameObject> itemPrefabs = new List<GameObject>();
        public GameObject itemPickupPrefab;
        public GameObject pickupTextPrefab;

        [Header("Sounds (Optional)")]
        public AudioClip pickupSound;
        public AudioClip onEquipSound;
        public AudioClip onUnequipSound;

        [Header("Sibling Toggles (Optional)")]
        public bool toggleSiblings;
        public List<ParentToggleData> parentToggles = new List<ParentToggleData>();
    }

    [System.Serializable]
    public class ParentToggleData
    {
        public string parentName;
        public List<string> enableOnEquip = new List<string>();
        public List<string> enableOnUnequip = new List<string>();
    }

    public enum ItemType { Consumable, Equipment, Currency, Other }
    public enum EquipmentCategory { Weapon, Helmet, Torso, Gloves, Shoulder, Boots, Pants, Rings, Necklace, Belt, Earrings, Back }
    public enum WeaponType { OneHand, TwoHand }

    [System.Serializable]
    public class ItemStat
    {
        public StatType statType;
        public int value;
    }
    public enum StatType { Attack, Defense, Block, Intelligence, Health, MaxHealth, Mana, MaxMana, Stamina, MaxStamina, Speed, Agility, Strength, Dexterity, Luck }
}
