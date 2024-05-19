using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Item")]
public class Item : ScriptableObject
{
    public enum ItemType
    {
        Consumable,
        Armor,
        Gloves,
        Shoes,
        Weapon
    }

    public string itemName;
    public string itemID;
    public int itemQuantity;
    public Sprite itemIcon;
    public bool stackable;
    public GameObject pickupPrefab;
    public GameObject pickupPromptPrefab;
    public ItemType itemType;
    public List<StatModification> statModifications = new List<StatModification>();

    [System.Serializable]
    public struct StatModification
    {
        public string statName;
        public int modificationValue;
    }
}
