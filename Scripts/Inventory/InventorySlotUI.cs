using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        public InventorySlot slot;
        public GameObject itemActionPrefab; // Assign this in the inspector
        private static GameObject currentItemActionUI;
        private Canvas parentCanvas;
        public bool isEquipmentSlot; // Indicates if this slot is for equipment

        private ItemDescriptionPanel _descPanel;

        private void Awake()
        {
            // Cache slot visuals
            slot.slotObject = gameObject;
            slot.stackText = transform.Find("DraggableItem/StackText")?.GetComponent<Text>();
            slot.itemIcon  = transform.Find("DraggableItem/ItemIcon")?.GetComponent<Image>();

            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                Debug.LogError("Parent canvas not found.");

            // Try to find the DescriptionPanel even if it's inactive
            var panels = Resources.FindObjectsOfTypeAll<ItemDescriptionPanel>();
            if (panels != null && panels.Length > 0)
                _descPanel = panels.First();

            if (slot.stackText == null)
                Debug.LogError($"StackText not found in DraggableItem for slot: {gameObject.name}");
            if (slot.itemIcon == null)
                Debug.LogError($"ItemIcon not found in DraggableItem for slot: {gameObject.name}");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Slot clicked: {gameObject.name}, Button: {eventData.button}, Item: {(slot.item != null ? slot.item.itemName : "None")}");

            // Close any existing action UI
            if (currentItemActionUI != null)
            {
                Destroy(currentItemActionUI);
                currentItemActionUI = null;
            }

            // LEFT click → double-click?
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Unity's clickCount resets unless the two clicks happen within the OS/system double-click interval
                if (eventData.clickCount == 2 && slot.item != null)
                    HandleDoubleClick();
            }
            // RIGHT click → context menu
            else if (eventData.button == PointerEventData.InputButton.Right && slot.item != null)
            {
                ShowItemActionUI(slot.item);
            }

            // Show or hide description panel
            InventoryItem clickedItem = slot.item;
            if (_descPanel == null)
            {
                var panels = Resources.FindObjectsOfTypeAll<ItemDescriptionPanel>();
                if (panels != null && panels.Length > 0)
                    _descPanel = panels.First();
            }

            if (_descPanel != null)
            {
                if (clickedItem != null)
                    _descPanel.Show(clickedItem);
                else
                    _descPanel.Hide();
            }
        }

        private void HandleDoubleClick()
        {
            // This only fires on a true “double-click” within Unity’s threshold
            if (slot != null && slot.item != null)
                slot.UseItem();
        }

        private void Update()
        {
            if (currentItemActionUI != null && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                RectTransform rectTransform = currentItemActionUI.GetComponent<RectTransform>();
                if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, parentCanvas.worldCamera))
                {
                    Destroy(currentItemActionUI);
                    currentItemActionUI = null;
                }
            }
        }

        private void ShowItemActionUI(InventoryItem item)
        {
            if (currentItemActionUI != null)
                Destroy(currentItemActionUI);

            currentItemActionUI = Instantiate(itemActionPrefab, parentCanvas.transform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out Vector2 anchoredPosition);

            RectTransform rectTransform = currentItemActionUI.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(100, 100);

            var itemActionUI = currentItemActionUI.GetComponent<ItemActionUI>();
            if (itemActionUI != null)
            {
                var playerEquipment = GameObject.FindWithTag("Player")?.GetComponent<Equipment>();
                itemActionUI.ConfigureButtons(item, this, playerEquipment);
            }
        }

        public void CloseItemActionUI()
        {
            if (currentItemActionUI != null)
            {
                Destroy(currentItemActionUI);
                currentItemActionUI = null;
            }
        }

        private void OnEnable()
        {
            slot.SetItem(slot.item, slot.quantity);
        }
    }
}
