using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    [System.Serializable]
    public class InventorySlot
    {
        public InventoryItem item;
        public int quantity;
        public GameObject slotObject;
        public Text stackText;
        public Image itemIcon;

        private CanvasGroup canvasGroup;

        public void SetItem(InventoryItem newItem, int newQuantity)
        {
            // --- SAFETY GUARD: ensure the slotObject has been assigned in the editor ---
            if (slotObject == null)
            {
                Debug.LogWarning("InventorySlot.SetItem: slotObject is not assigned. Skipping SetItem.");
                return;
            }

            // --- AUTO‐WIRE: if icon/text refs are missing, try to find them under "DraggableItem" ---
            if (itemIcon == null || stackText == null)
            {
                var draggable = slotObject.transform.Find("DraggableItem");
                if (draggable != null)
                {
                    if (itemIcon == null)
                        itemIcon = draggable.Find("ItemIcon")?.GetComponent<Image>();
                    if (stackText == null)
                        stackText = draggable.Find("StackText")?.GetComponent<Text>();
                }
                if (itemIcon == null || stackText == null)
                {
                    Debug.LogWarning($"InventorySlot.SetItem: Missing UI refs in '{slotObject.name}'. Skipping.");
                    return;
                }
            }

            // --- NOW SAFE TO ASSIGN ---
            item     = newItem;
            quantity = newQuantity;

            if (item != null)
            {
                itemIcon.sprite = item.itemIcon;
                itemIcon.enabled = true;
                itemIcon.color  = new Color(itemIcon.color.r, itemIcon.color.g, itemIcon.color.b, 1);

                stackText.text    = quantity > 1 ? quantity.ToString() : "";
                stackText.enabled = true;
                Debug.Log($"Set item: {item.itemName} ({quantity}) in slot {slotObject.name}");
            }
            else
            {
                itemIcon.sprite  = null;
                itemIcon.enabled = true;
                itemIcon.color   = new Color(itemIcon.color.r, itemIcon.color.g, itemIcon.color.b, 0);

                stackText.text    = "";
                stackText.enabled = true;
                Debug.Log($"Cleared slot {slotObject.name}");
            }

            // --- PRESERVE ORIGINAL TRANSFORM RESET LOGIC ---
            SetTransformProperties();

            // Restore CanvasGroup so the slot is interactable
            if (canvasGroup == null)
                canvasGroup = slotObject.GetComponentInChildren<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = 1;
                canvasGroup.blocksRaycasts = true;
            }

            // Update drag handler’s slot reference
            var dragHandler = slotObject.transform
                                        .Find("DraggableItem")
                                        ?.GetComponent<InventoryDragHandler>();
            if (dragHandler != null)
                dragHandler.slot = this;
        }

        public void SetTransformProperties()
        {
            if (slotObject == null || itemIcon == null || stackText == null)
            {
                Debug.LogError("One or more required components are not assigned in InventorySlot.");
                return;
            }

            // DraggableItem size & anchors
            RectTransform draggableItemRect = slotObject.transform
                                                       .Find("DraggableItem")
                                                       ?.GetComponent<RectTransform>();
            if (draggableItemRect == null)
            {
                Debug.LogError($"DraggableItem RectTransform not found in {slotObject.name}");
                return;
            }
            draggableItemRect.anchorMin   = new Vector2(0.5f, 0.5f);
            draggableItemRect.anchorMax   = new Vector2(0.5f, 0.5f);
            draggableItemRect.offsetMin   = Vector2.zero;
            draggableItemRect.offsetMax   = Vector2.zero;
            draggableItemRect.sizeDelta   = new Vector2(56, 56);

            // ItemIcon sizing
            RectTransform itemIconRect = itemIcon.GetComponent<RectTransform>();
            itemIconRect.anchorMin     = new Vector2(0.5f, 0.5f);
            itemIconRect.anchorMax     = new Vector2(0.5f, 0.5f);
            itemIconRect.pivot         = new Vector2(0.5f, 0.5f);
            itemIconRect.sizeDelta     = new Vector2(80, 80);
            itemIconRect.anchoredPosition = Vector2.zero;

            // StackText positioning
            RectTransform stackTextRect = stackText.GetComponent<RectTransform>();
            stackTextRect.anchorMin     = new Vector2(0, 0.5f);
            stackTextRect.anchorMax     = new Vector2(0, 0.5f);
            stackTextRect.pivot         = new Vector2(0, 1);
            stackTextRect.sizeDelta     = new Vector2(56, 28);
            stackTextRect.anchoredPosition = new Vector2(0, 0);
        }

        public void UseItem()
        {
            if (item == null)
            {
                Debug.Log("No item to use.");
                return;
            }

            Debug.Log($"Used item: {item.itemName}");

            if (item.itemType == ItemType.Consumable)
            {
                if (item.onEquipSound != null && Camera.main != null)
                {
                    AudioSource.PlayClipAtPoint(item.onEquipSound, Camera.main.transform.position);
                }
                var playerStatus = GameObject.FindWithTag("Player")
                                             .GetComponent<PlayerStatus>();
                if (playerStatus != null)
                    playerStatus.ApplyConsumableEffects(item.stats);

                quantity--;
                if (quantity <= 0)
                    SetItem(null, 0);
                else
                    stackText.text = quantity.ToString();
            }
            else if (item.itemType == ItemType.Equipment)
            {
                var equipment = GameObject.FindWithTag("Player")
                                          .GetComponent<Equipment>();
                if (equipment == null)
                {
                    Debug.LogError("Equipment component not found on the Player GameObject.");
                    return;
                }

                if (equipment.IsItemEquipped(item))
                    equipment.UnequipItem(item);
                else
                {
                    equipment.EquipItem(item);
                    quantity--;
                    if (quantity <= 0)
                        SetItem(null, 0);
                    else
                        stackText.text = "";
                }
            }
        }
    }
}
