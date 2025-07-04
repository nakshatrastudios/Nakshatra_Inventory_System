using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class EquipmentSlot : MonoBehaviour
    {
        public GameObject draggableItem;
        public GameObject slotObject;
        public Image itemIcon;
        public Text stackText;

        public InventoryItem item;
        public int quantity;

        public void SetItem(InventoryItem newItem, int newQuantity)
        {
            item = newItem;
            quantity = newQuantity;

            if (item != null)
            {
                itemIcon.sprite = item.itemIcon;
                stackText.text = quantity > 1 ? quantity.ToString() : "";
                itemIcon.enabled = true;
                stackText.enabled = quantity > 1;
            }
            else
            {
                itemIcon.enabled = false;
                stackText.enabled = false;
            }
        }

        public void ClearSlot()
        {
            item = null;
            quantity = 0;
            itemIcon.enabled = false;
            stackText.enabled = false;
        }

        public bool HasItem()
        {
            return item != null;
        }
    }

}
