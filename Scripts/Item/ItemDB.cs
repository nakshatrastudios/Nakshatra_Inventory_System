using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    [CreateAssetMenu(fileName = "ItemDB", menuName = "Inventory/ItemDB")]
    public class ItemDB : ScriptableObject
    {
        public List<InventoryItem> items = new List<InventoryItem>();
    }
}