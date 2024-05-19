using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEditor : EditorWindow
{
    string inventoryName = "New Inventory";
    int numberOfSlots = 0;
    int slotsPerRow = 0;
    Sprite slotUISprite;
    Sprite inventoryBackground;

    [MenuItem("Inventory System/Create Inventory")]
    public static void ShowWindow()
    {
        GetWindow<InventoryEditor>("Create Inventory");
    }

    void OnGUI()
    {
        GUILayout.Label("Create a new inventory", EditorStyles.boldLabel);

        inventoryName = EditorGUILayout.TextField("Inventory Name", inventoryName);
        numberOfSlots = EditorGUILayout.IntField("Number of Slots", numberOfSlots);
        slotsPerRow = EditorGUILayout.IntField("Slots Per Row", slotsPerRow);
        slotUISprite = (Sprite)EditorGUILayout.ObjectField("Slot UI Sprite", slotUISprite, typeof(Sprite), false);
        inventoryBackground = (Sprite)EditorGUILayout.ObjectField("Inventory Background", inventoryBackground, typeof(Sprite), false);

        if (GUILayout.Button("Create Inventory"))
        {
            Inventory newInventory = ScriptableObject.CreateInstance<Inventory>();
            newInventory.inventoryName = inventoryName;
            newInventory.numberOfSlots = numberOfSlots;
            newInventory.slotsPerRow = slotsPerRow;
            newInventory.slotUISprite = slotUISprite;
            newInventory.inventoryBackground = inventoryBackground;

            GameObject inventoryGO = new GameObject(inventoryName);
            Canvas canvas = inventoryGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = inventoryGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1.0f;

            GameObject bgGO = new GameObject("Inventory ViewPort");
            bgGO.transform.SetParent(inventoryGO.transform);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.pivot = new Vector2(1, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;

            int numberOfRows = Mathf.CeilToInt((float)numberOfSlots / slotsPerRow);
            Vector2 backgroundSize = new Vector2((100 + 10) * slotsPerRow + 20 + 20, (100 + 10) * numberOfRows + 20 + 20);
            bgRect.sizeDelta = backgroundSize;

            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.sprite = inventoryBackground;

            GridLayoutGroup grid = bgGO.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(100, 100);
            grid.spacing = new Vector2(10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = slotsPerRow;
            grid.padding = new RectOffset(20, 20, 20, 20);

            for (int i = 0; i < numberOfSlots; i++)
            {
                GameObject newSlot = new GameObject("Slot_" + i);
                newSlot.transform.SetParent(bgGO.transform);
                RectTransform slotRect = newSlot.AddComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0, 1);
                slotRect.anchorMax = new Vector2(0, 1);
                slotRect.pivot = new Vector2(0.5f, 0.5f);
                slotRect.sizeDelta = new Vector2(100, 100);

                Image slotImageComponent = newSlot.AddComponent<Image>();
                slotImageComponent.sprite = slotUISprite;

                Button slotButton = newSlot.AddComponent<Button>();
                slotButton.onClick.AddListener(() => Debug.Log("Slot clicked"));

                GameObject itemIcon = new GameObject("Item Icon");
                itemIcon.transform.SetParent(newSlot.transform);
                RectTransform itemIconRect = itemIcon.AddComponent<RectTransform>();
                itemIconRect.anchorMin = Vector2.zero;
                itemIconRect.anchorMax = Vector2.one;
                itemIconRect.offsetMin = new Vector2(10, 10); // Add an offset to make the item icon smaller than the slot
                itemIconRect.offsetMax = new Vector2(-10, -10);
                Image itemIconImage = itemIcon.AddComponent<Image>();
                itemIconImage.enabled = false; // Disable the item icon by default

                GameObject itemQuantity = new GameObject("Item Quantity");
                itemQuantity.transform.SetParent(newSlot.transform);
                RectTransform itemQuantityRect = itemQuantity.AddComponent<RectTransform>();
                itemQuantityRect.anchorMin = new Vector2(1, 0); // Anchor to the bottom right corner
                itemQuantityRect.anchorMax = new Vector2(1, 0);
                itemQuantityRect.pivot = new Vector2(1, 0);
                itemQuantityRect.anchoredPosition = new Vector2(-10, 10); // Position it 10 units from the right and bottom edges
                Text itemQuantityText = itemQuantity.AddComponent<Text>();
                //itemQuantityText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                itemQuantityText.fontSize = 25;
                itemQuantityText.alignment = TextAnchor.LowerRight;
                itemQuantityText.enabled = false; // Disable the quantity text by default
            }

            string prefabPath = "Assets/InventorySystem/Inventories/" + inventoryName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(inventoryGO, prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Could not create prefab at path: " + prefabPath);
                return;
            }

            DestroyImmediate(inventoryGO);

            AssetDatabase.CreateAsset(newInventory, "Assets/InventorySystem/InventoryScriptableObjects/" + inventoryName + ".asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = newInventory;
        }
    }
}
