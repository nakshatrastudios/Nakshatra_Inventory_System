using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class Inventory : ScriptableObject
{
    public string inventoryName;
    public int numberOfSlots;
    public int slotsPerRow;
    public Sprite slotUISprite; // Changed from GameObject to Sprite
    public Sprite inventoryBackground;
}
