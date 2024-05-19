[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public int quantity;

    public InventoryItemData(InventoryItem inventoryItem)
    {
        this.itemName = inventoryItem.item.itemName;
        this.quantity = inventoryItem.quantity;
    }
}
