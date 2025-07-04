using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class InventoryDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public InventorySlot slot;

        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        private Inventory inventory;
        private Equipment equipment;
        private bool isChangingPage = false;
        private GameObject dragItem;
        private RectTransform dragRectTransform;
        private Canvas dragCanvas;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            inventory = FindObjectOfType<Inventory>();
            equipment = FindObjectOfType<Equipment>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slot.item != null)
            {
                originalPosition = rectTransform.anchoredPosition;
                canvasGroup.alpha = 0.6f;
                canvasGroup.blocksRaycasts = false;

                // Create a new canvas for dragging
                dragCanvas = new GameObject("DragCanvas").AddComponent<Canvas>();
                dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                dragCanvas.sortingOrder = 1000;

                dragItem = new GameObject("DragItem");
                dragItem.transform.SetParent(dragCanvas.transform, false);

                Image dragImage = dragItem.AddComponent<Image>();
                dragImage.sprite = slot.itemIcon.sprite;
                dragImage.SetNativeSize();

                CanvasGroup tempCanvasGroup = dragItem.AddComponent<CanvasGroup>();
                tempCanvasGroup.blocksRaycasts = false;

                dragRectTransform = dragItem.GetComponent<RectTransform>();
                dragRectTransform.sizeDelta = rectTransform.sizeDelta;
                dragRectTransform.position = Input.mousePosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragRectTransform != null)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    dragCanvas.transform as RectTransform,
                    Input.mousePosition,
                    eventData.pressEventCamera,
                    out position
                );
                dragRectTransform.localPosition = position;

                // Page-changing logic
                if (inventory != null && inventory.nextPageButton != null && inventory.previousPageButton != null)
                {
                    var nextRect = inventory.nextPageButton.GetComponent<RectTransform>();
                    var prevRect = inventory.previousPageButton.GetComponent<RectTransform>();

                    if (!isChangingPage && RectTransformUtility.RectangleContainsScreenPoint(nextRect, Input.mousePosition, canvas.worldCamera))
                    {
                        isChangingPage = true;
                        inventory.NextPage();
                        Invoke(nameof(ResetPageChange), 0.5f);
                    }
                    else if (!isChangingPage && RectTransformUtility.RectangleContainsScreenPoint(prevRect, Input.mousePosition, canvas.worldCamera))
                    {
                        isChangingPage = true;
                        inventory.PreviousPage();
                        Invoke(nameof(ResetPageChange), 0.5f);
                    }
                }
            }
        }

        private void ResetPageChange()
        {
            isChangingPage = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragItem != null)
            {
                Destroy(dragItem);
                dragItem = null;
            }
            if (dragCanvas != null)
            {
                Destroy(dragCanvas.gameObject);
                dragCanvas = null;
            }
            if (slot.item != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;

                // Reset visuals
                rectTransform.anchoredPosition = Vector2.zero;
                slot.SetTransformProperties();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;

            var draggedHandler = eventData.pointerDrag.GetComponent<InventoryDragHandler>();
            if (draggedHandler == null || draggedHandler.slot == null) return;

            var draggedSlot = draggedHandler.slot;
            var targetSlot = slot;

            // Can't drop if no item
            if (draggedSlot.item == null) return;

            // EQUIPMENT → slot logic
            if (targetSlot.slotObject.GetComponent<InventorySlotUI>().isEquipmentSlot)
            {
                var equip = GameObject.FindWithTag("Player")?.GetComponent<Equipment>();
                if (equip != null)
                {
                    var correctSlot = equip.GetTargetSlot(draggedSlot.item);
                    if (draggedSlot.item.itemType == ItemType.Equipment &&
                        correctSlot != null &&
                        correctSlot.slotObject == targetSlot.slotObject)
                    {
                        equip.EquipItem(draggedSlot.item);
                        draggedSlot.SetItem(null, 0);
                    }
                    else
                    {
                        // Invalid equipment drop
                        draggedHandler.rectTransform.anchoredPosition = draggedHandler.originalPosition;
                    }
                }
            }
            else
            {
                // DRAG FROM equipment back to inventory
                if (draggedSlot.slotObject.GetComponent<InventorySlotUI>().isEquipmentSlot &&
                    !targetSlot.slotObject.GetComponent<InventorySlotUI>().isEquipmentSlot)
                {
                    var equip = GameObject.FindWithTag("Player")?.GetComponent<Equipment>();
                    equip?.UnequipItem(draggedSlot.item);
                }

                // MERGE LOGIC: same item and stackable?
                if (draggedSlot.item == targetSlot.item && draggedSlot.item.isStackable)
                {
                    int combined = draggedSlot.quantity + targetSlot.quantity;
                    int maxStack = draggedSlot.item.maxStackSize;
                    int toTarget = Mathf.Min(combined, maxStack);
                    int leftover = combined - toTarget;

                    targetSlot.SetItem(draggedSlot.item, toTarget);
                    if (leftover > 0)
                        draggedSlot.SetItem(draggedSlot.item, leftover);
                    else
                        draggedSlot.SetItem(null, 0);
                }
                else
                {
                    // FALLBACK SWAP
                    var tempItem = targetSlot.item;
                    var tempQty  = targetSlot.quantity;

                    if (draggedSlot.slotObject.GetComponent<InventorySlotUI>().isEquipmentSlot)
                    {
                        targetSlot.SetItem(tempItem, tempQty);
                        draggedSlot.SetItem(null, 0);
                    }
                    else
                    {
                        targetSlot.SetItem(draggedSlot.item, draggedSlot.quantity);
                        draggedSlot.SetItem(tempItem, tempQty);
                    }
                }

                // Reset positions & raycasts
                draggedHandler.rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.anchoredPosition               = Vector2.zero;
                draggedHandler.slot.SetTransformProperties();
                targetSlot.SetTransformProperties();
                draggedHandler.canvasGroup.blocksRaycasts = true;
                canvasGroup.blocksRaycasts               = true;
                draggedHandler.canvasGroup.alpha         = 1f;
                canvasGroup.alpha                        = 1f;

                if (draggedHandler.dragItem != null)
                {
                    Destroy(draggedHandler.dragItem);
                    draggedHandler.dragItem = null;
                }
            }
        }

        private void Update()
        {
            if (dragItem != null)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    dragCanvas.transform as RectTransform,
                    Input.mousePosition,
                    null,
                    out position
                );
                dragRectTransform.localPosition = position;
            }
        }
    }
}

