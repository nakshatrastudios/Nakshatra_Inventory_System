using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoadManager
{
    public static void SaveInventory(InventoryComponent inventory, string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + fileName);
        List<InventoryItemData> data = inventory.items.Select(item => new InventoryItemData(item)).ToList();
        bf.Serialize(file, data);
        file.Close();
    }


    public static void LoadInventory(InventoryComponent inventory, string fileName, Dictionary<string, Item> itemDictionary)
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.Open);
            List<InventoryItemData> data = (List<InventoryItemData>)bf.Deserialize(file);
            file.Close();
            inventory.items = data.Select(itemData => new InventoryItem(itemDictionary[itemData.itemName], itemData.quantity)).ToList();
        }
    }

}
