using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nakshatra.Plugins
{
    [Serializable]
    public class SaveData
    {

        public List<SerializableInventorySlot> inventory = new List<SerializableInventorySlot>();
        public List<SerializableInventorySlot> equipment = new List<SerializableInventorySlot>();
        public List<SerializableQuickAccessSlot> quickAccessBar = new List<SerializableQuickAccessSlot>();
    }
}