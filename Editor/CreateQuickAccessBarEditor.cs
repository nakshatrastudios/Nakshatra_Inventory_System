using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Nakshatra.Plugins;  // Access Inventory, QuickAccessBar, SaveLoadUIManager

namespace Nakshatra.Plugins.Editor
{
    public class CreateQuickAccessBarEditor : EditorWindow
    {
        private int quickAccessSlots;
        private Sprite quickAccessBackgroundSprite;
        private GameObject quickAccessSlotPrefab;
        private GameObject buttonNumberTextPrefab;
        private ItemDB itemDB;                        // ← New field for selecting your ItemDB
        private int quickAccessPaddingLeft;
        private int quickAccessPaddingRight;
        private int quickAccessPaddingTop;
        private int quickAccessPaddingBottom;
        private Vector2 quickAccessGridSpacing;
        private Vector2 quickAccessCellSize;
        private float quickAccessBackgroundPaddingPercentage = 17f;

        //[MenuItem("Tools/Nakshatra Studios/Nakshatra Inventory System/Create Quick Access Bar")]
        public static void ShowWindow()
        {
            GetWindow<CreateQuickAccessBarEditor>("Create Quick Access Bar");
        }

        public void OnGUI()
        {
            GUILayout.Label("Quick Access Bar Settings", EditorStyles.boldLabel);

            quickAccessSlots = EditorGUILayout.IntField("Number of Slots", quickAccessSlots);
            quickAccessSlots = Mathf.Clamp(quickAccessSlots, 1, 10);

            quickAccessBackgroundSprite = (Sprite)EditorGUILayout.ObjectField(
                "Background Sprite",
                quickAccessBackgroundSprite,
                typeof(Sprite),
                false
            );

            quickAccessSlotPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Slot Prefab",
                quickAccessSlotPrefab,
                typeof(GameObject),
                false
            );

            buttonNumberTextPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Button Number Text Prefab",
                buttonNumberTextPrefab,
                typeof(GameObject),
                false
            );

            // ——— NEW: ItemDB selector ———
            itemDB = (ItemDB)EditorGUILayout.ObjectField(
                "Item Database",
                itemDB,
                typeof(ItemDB),
                false
            );
            if (itemDB == null)
            {
                EditorGUILayout.HelpBox("Assign an ItemDB here or it will default to the Player’s Inventory.itemDB.", MessageType.Info);
            }

            GUILayout.Space(10);
            GUILayout.Label("Grid Layout Settings", EditorStyles.boldLabel);

            quickAccessPaddingLeft = EditorGUILayout.IntField("Padding Left", quickAccessPaddingLeft);
            quickAccessPaddingRight = EditorGUILayout.IntField("Padding Right", quickAccessPaddingRight);
            quickAccessPaddingTop = EditorGUILayout.IntField("Padding Top", quickAccessPaddingTop);
            quickAccessPaddingBottom = EditorGUILayout.IntField("Padding Bottom", quickAccessPaddingBottom);

            quickAccessGridSpacing = EditorGUILayout.Vector2Field("Grid Spacing", quickAccessGridSpacing);
            quickAccessCellSize   = EditorGUILayout.Vector2Field("Cell Size", quickAccessCellSize);

            GUILayout.Space(10);
            GUILayout.Label("Background Padding Percentage", EditorStyles.boldLabel);

            quickAccessBackgroundPaddingPercentage = EditorGUILayout.Slider(
                "Padding Percentage",
                quickAccessBackgroundPaddingPercentage,
                0f,
                20f
            );

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create Quick Access Bar"))
            {
                CreateQuickAccessBar();
            }
        }

        private void CreateQuickAccessBar()
        {
            // 1) Player & Inventory
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject with tag 'Player' not found.");
                return;
            }
            var inv = player.GetComponent<Inventory>();
            if (inv == null)
            {
                Debug.LogError("Inventory component not found on Player.");
                return;
            }

            // 2) Attach QuickAccessBar
            QuickAccessBar quickAccessBarComponent = Undo.AddComponent<QuickAccessBar>(player);

            // 3) Wire up the ItemDB reference
            quickAccessBarComponent.itemDB = itemDB != null ? itemDB : inv.itemDB;

            // 4) Find existing InventorySystem Canvas
            GameObject canvasObject = GameObject.Find("InventorySystem");
            if (canvasObject == null)
            {
                Debug.LogError("InventorySystem Canvas not found. Please create the Inventory first.");
                return;
            }
            var canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("GameObject 'InventorySystem' does not have a Canvas component.");
                return;
            }

            // 5) Create QuickAccessBar root under Canvas
            GameObject quickAccessBarObject = new GameObject("QuickAccessBar", typeof(RectTransform));
            quickAccessBarObject.transform.SetParent(canvasObject.transform, false);

            if (quickAccessBackgroundSprite != null)
            {
                var bg = quickAccessBarObject.AddComponent<Image>();
                bg.sprite = quickAccessBackgroundSprite;
            }

            // 6) Grid container
            GameObject gridGO = new GameObject("QuickAccessGrid", typeof(RectTransform));
            gridGO.transform.SetParent(quickAccessBarObject.transform, false);
            var gridRect = gridGO.GetComponent<RectTransform>();
            gridRect.anchoredPosition = new Vector2(0, -quickAccessPaddingTop);

            var gridLayout = gridGO.AddComponent<GridLayoutGroup>();
            gridLayout.padding         = new RectOffset(quickAccessPaddingLeft, quickAccessPaddingRight, quickAccessPaddingTop, quickAccessPaddingBottom);
            gridLayout.spacing         = quickAccessGridSpacing;
            gridLayout.cellSize        = quickAccessCellSize;
            gridLayout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = quickAccessSlots;

            float gridW = quickAccessCellSize.x * quickAccessSlots + quickAccessGridSpacing.x * (quickAccessSlots - 1) + quickAccessPaddingLeft + quickAccessPaddingRight;
            float gridH = quickAccessCellSize.y + quickAccessGridSpacing.y + quickAccessPaddingTop + quickAccessPaddingBottom;
            gridRect.sizeDelta = new Vector2(gridW, gridH);

            var barRect = quickAccessBarObject.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(
                gridW * (1 + quickAccessBackgroundPaddingPercentage / 100f),
                gridH * (1 + quickAccessBackgroundPaddingPercentage / 100f)
            );

            // 7) Assign quickAccessBarComponent fields
            quickAccessBarComponent.totalSlots             = quickAccessSlots;
            quickAccessBarComponent.slotPrefab             = quickAccessSlotPrefab;
            quickAccessBarComponent.quickAccessGrid        = gridGO.transform;
            quickAccessBarComponent.buttonNumberTextPrefab = buttonNumberTextPrefab;

            // 8) Register with SaveLoadUIManager
            var slu = Object.FindObjectOfType<SaveLoadUIManager>();
            if (slu != null)
                slu.quickAccessBar = quickAccessBarComponent;
            else
                Debug.LogWarning("SaveLoadUIManager not found—quickAccessBar reference not set.");

            Debug.Log("Quick Access Bar created under InventorySystem Canvas.");
        }
    }
}
