using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins.Editor
{
    public class CreateChestEditor : EditorWindow
    {
        [SerializeField] private KeyCode openKey = KeyCode.E;
        [SerializeField] private bool useSameKeyToClose = false;
        private SerializedObject so;

        private GameObject chestPrefab;
        private string chestName = "New Chest";
        private int totalSlots = 20;
        private int slotsPerRow = 5;
        private int rowsPerPage = 4;

        [Header("UI Panel Settings")]
        public Sprite chestPanelBackgroundSprite;
        public GameObject slotPrefab;
        public GameObject nextPageButtonPrefab;
        public GameObject previousPageButtonPrefab;

        [Header("Grid Layout Settings")]
        public int gridPaddingLeft = 2, gridPaddingRight = 2, gridPaddingTop = 2, gridPaddingBottom = 2;
        public Vector2 gridSpacing = new Vector2(2, 2), cellSize = new Vector2(65, 65);

        [Header("Content Settings")]
        public ItemDB itemDB;
        public bool randomizeContents = false;

        [Header("Global Randomization Settings")]
        public int minRandomDrops = 1;
        public int maxRandomDrops = 5;

        [Header("Random Loot List")]
        public List<ChestInventory.RandomItemEntry> randomItemEntries = new List<ChestInventory.RandomItemEntry>();
        public List<ChestInventory.RandomCurrencyEntry> randomCurrencyEntries = new List<ChestInventory.RandomCurrencyEntry>();

        private List<InventoryItem> manualItems = new List<InventoryItem>();
        private List<int> manualQuantities = new List<int>();

        //[MenuItem("Tools/Nakshatra Studios/Nakshatra Inventory System/Create Chest")]
        public static void ShowWindow() => GetWindow<CreateChestEditor>("Create Chest");

        private void OnEnable()
        {
            so = new SerializedObject(this);
        }

        public void OnGUI()
        {
            so.Update();

            GUILayout.Label("Chest Settings", EditorStyles.boldLabel);
            chestName      = EditorGUILayout.TextField("Chest Name", chestName);
            chestPrefab    = (GameObject)EditorGUILayout.ObjectField("Chest Prefab", chestPrefab, typeof(GameObject), false);
            totalSlots     = EditorGUILayout.IntField("Total Slots", totalSlots);
            slotsPerRow    = EditorGUILayout.IntField("Slots Per Row", slotsPerRow);
            rowsPerPage    = EditorGUILayout.IntField("Rows Per Page", rowsPerPage);

            EditorGUILayout.PropertyField(so.FindProperty("openKey"), new GUIContent("Open Key"));
            EditorGUILayout.PropertyField(so.FindProperty("useSameKeyToClose"), new GUIContent("Use Same Key To Close"));
            EditorGUILayout.Space();

            GUILayout.Label("UI Panel Settings", EditorStyles.boldLabel);
            chestPanelBackgroundSprite = (Sprite)EditorGUILayout.ObjectField("Panel Background Sprite", chestPanelBackgroundSprite, typeof(Sprite), false);
            slotPrefab                 = (GameObject)EditorGUILayout.ObjectField("Slot Prefab", slotPrefab, typeof(GameObject), false);
            nextPageButtonPrefab       = (GameObject)EditorGUILayout.ObjectField("Next Page Button Prefab", nextPageButtonPrefab, typeof(GameObject), false);
            previousPageButtonPrefab   = (GameObject)EditorGUILayout.ObjectField("Previous Page Button Prefab", previousPageButtonPrefab, typeof(GameObject), false);
            EditorGUILayout.Space();

            GUILayout.Label("Grid Layout Settings", EditorStyles.boldLabel);
            gridPaddingLeft   = EditorGUILayout.IntField("Padding Left", gridPaddingLeft);
            gridPaddingRight  = EditorGUILayout.IntField("Padding Right", gridPaddingRight);
            gridPaddingTop    = EditorGUILayout.IntField("Padding Top", gridPaddingTop);
            gridPaddingBottom = EditorGUILayout.IntField("Padding Bottom", gridPaddingBottom);
            gridSpacing       = EditorGUILayout.Vector2Field("Grid Spacing", gridSpacing);
            cellSize          = EditorGUILayout.Vector2Field("Cell Size", cellSize);
            EditorGUILayout.Space();

            GUILayout.Label("Content Settings", EditorStyles.boldLabel);
            itemDB            = (ItemDB)EditorGUILayout.ObjectField("Item Database", itemDB, typeof(ItemDB), false);
            randomizeContents = EditorGUILayout.Toggle("Randomize Contents", randomizeContents);
            EditorGUILayout.Space();

            if (randomizeContents)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("Global Random Settings", EditorStyles.boldLabel);
                    minRandomDrops = EditorGUILayout.IntField("Min Items To Drop", minRandomDrops);
                    maxRandomDrops = EditorGUILayout.IntField("Max Items To Drop", maxRandomDrops);
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal("box");
                            GUILayout.Label("Random Items", EditorStyles.boldLabel);
                            if (GUILayout.Button("+", GUILayout.Width(40)))
                                randomItemEntries.Add(new ChestInventory.RandomItemEntry { item = null, quantity = 1, dropProbability = 1f });
                        EditorGUILayout.EndHorizontal();

                        for (int i = randomItemEntries.Count - 1; i >= 0; i--)
                        {
                            var e = randomItemEntries[i];
                            bool remove = false;
                            EditorGUILayout.BeginHorizontal("box");
                                e.item            = (InventoryItem)EditorGUILayout.ObjectField(e.item, typeof(InventoryItem), false, GUILayout.Width(200));
                                e.quantity        = EditorGUILayout.IntField("Qty", e.quantity, GUILayout.Width(200));
                                e.dropProbability = EditorGUILayout.Slider("Prob", e.dropProbability, 0f, 1f, GUILayout.Width(300));
                                if (GUILayout.Button("x", GUILayout.Width(40)))
                                    remove = true;
                            EditorGUILayout.EndHorizontal();

                            if (remove)
                                randomItemEntries.RemoveAt(i);
                            else
                                randomItemEntries[i] = e;
                        }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal("box");
                            GUILayout.Label("Random Currency", EditorStyles.boldLabel);
                            if (GUILayout.Button("+", GUILayout.Width(40)))
                                randomCurrencyEntries.Add(new ChestInventory.RandomCurrencyEntry { currencyName = "", minAmount = 1, maxAmount = 1 });
                        EditorGUILayout.EndHorizontal();

                        for (int i = randomCurrencyEntries.Count - 1; i >= 0; i--)
                        {
                            var c = randomCurrencyEntries[i];
                            bool remove = false;
                            EditorGUILayout.BeginHorizontal();
                                c.currencyName = EditorGUILayout.TextField("Currency", c.currencyName, GUILayout.Width(200));
                                c.minAmount    = EditorGUILayout.IntField("Min Amt", c.minAmount, GUILayout.Width(200));
                                c.maxAmount    = EditorGUILayout.IntField("Max Amt", c.maxAmount, GUILayout.Width(300));
                                if (GUILayout.Button("x", GUILayout.Width(40)))
                                    remove = true;
                            EditorGUILayout.EndHorizontal();

                            if (remove)
                                randomCurrencyEntries.RemoveAt(i);
                            else
                                randomCurrencyEntries[i] = c;
                        }
                    EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal("box");
                        GUILayout.Label("Manual Contents", EditorStyles.boldLabel);
                        if (GUILayout.Button("+", GUILayout.Width(40)))
                        {
                            manualItems.Add(null);
                            manualQuantities.Add(1);
                        }
                    EditorGUILayout.EndHorizontal();

                    for (int i = manualItems.Count - 1; i >= 0; i--)
                    {
                        bool remove = false;
                        EditorGUILayout.BeginHorizontal();
                            manualItems[i]      = (InventoryItem)EditorGUILayout.ObjectField($"Item #{i + 1}", manualItems[i], typeof(InventoryItem), false, GUILayout.Width(300));
                            manualQuantities[i] = EditorGUILayout.IntField("Qty", manualQuantities[i], GUILayout.Width(300));
                            if (GUILayout.Button("x", GUILayout.Width(40)))
                                remove = true;
                        EditorGUILayout.EndHorizontal();

                        if (remove)
                        {
                            manualItems.RemoveAt(i);
                            manualQuantities.RemoveAt(i);
                        }
                    }
                EditorGUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create Chest", GUILayout.Height(30)))
                CreateChest();

            so.ApplyModifiedProperties();
        }

        private void CreateChest()
        {
            if (chestPrefab == null)
            {
                Debug.LogError("Assign a Chest Prefab first.");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(chestPrefab);
            Undo.RegisterCreatedObjectUndo(instance, "Create Chest");
            instance.name = chestName;

            var inv = instance.GetComponent<ChestInventory>() ?? instance.AddComponent<ChestInventory>();
            inv.chestName             = chestName;
            inv.totalSlots            = totalSlots;
            inv.columns               = slotsPerRow;
            inv.rows                  = rowsPerPage;
            inv.slotPrefab            = slotPrefab;
            inv.itemDB                = itemDB;
            inv.randomizeContents     = randomizeContents;
            inv.minRandomDrops        = minRandomDrops;
            inv.maxRandomDrops        = maxRandomDrops;
            inv.randomItemEntries     = new List<ChestInventory.RandomItemEntry>(randomItemEntries);
            inv.randomCurrencyEntries = new List<ChestInventory.RandomCurrencyEntry>(randomCurrencyEntries);

            if (!randomizeContents)
            {
                inv.manualItems = new List<ChestInventory.ChestItemEntry>();
                for (int i = 0; i < manualItems.Count; i++)
                {
                    if (manualItems[i] != null && manualQuantities[i] > 0)
                        inv.manualItems.Add(new ChestInventory.ChestItemEntry { item = manualItems[i], quantity = manualQuantities[i] });
                }
            }

            // setup Canvas and Panel...
            var canvasGO = new GameObject("ChestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(instance.transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            var panelGO = new GameObject("ChestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelImage = panelGO.GetComponent<Image>();
            if (chestPanelBackgroundSprite != null)
                panelImage.sprite = chestPanelBackgroundSprite;

            var gridGO = new GameObject("ChestGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGO.transform.SetParent(panelGO.transform, false);
            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.padding         = new RectOffset(gridPaddingLeft, gridPaddingRight, gridPaddingTop, gridPaddingBottom);
            grid.spacing         = gridSpacing;
            grid.cellSize        = cellSize;
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = slotsPerRow;
            inv.chestGrid        = gridGO.transform;

            var gridRT = gridGO.GetComponent<RectTransform>();
            float w = cellSize.x * slotsPerRow + gridSpacing.x * (slotsPerRow - 1) + gridPaddingLeft + gridPaddingRight;
            float h = cellSize.y * inv.rows + gridSpacing.y * (inv.rows - 1) + gridPaddingTop + gridPaddingBottom;
            gridRT.sizeDelta = new Vector2(w, h);
            const float margin = 20f;
            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(w + margin, h + margin);

            var btnContainer = new GameObject("ButtonContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            btnContainer.transform.SetParent(panelGO.transform, false);
            var hl = btnContainer.GetComponent<HorizontalLayoutGroup>();
            hl.spacing               = 5;
            hl.childAlignment        = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            var cf = btnContainer.GetComponent<ContentSizeFitter>();
            cf.horizontalFit = cf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rt = btnContainer.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-margin/2, margin/2);

            if (previousPageButtonPrefab != null)
            {
                var prev = (GameObject)PrefabUtility.InstantiatePrefab(previousPageButtonPrefab);
                prev.name = "PreviousPageButton";
                prev.transform.SetParent(btnContainer.transform, false);
                inv.previousPageButton = prev.GetComponent<Button>();
            }
            if (nextPageButtonPrefab != null)
            {
                var next = (GameObject)PrefabUtility.InstantiatePrefab(nextPageButtonPrefab);
                next.name = "NextPageButton";
                next.transform.SetParent(btnContainer.transform, false);
                inv.nextPageButton = next.GetComponent<Button>();
            }

            var ctrl = instance.GetComponent<ChestController>();
            ctrl.openKey           = openKey;
            ctrl.useSameKeyToClose = useSameKeyToClose;
            ctrl.chestPanel        = panelGO;

            var slu = Object.FindObjectOfType<SaveLoadUIManager>();
            if (slu != null)
            {
                if (slu.chests == null) slu.chests = new List<ChestInventory>();
                slu.chests.Add(inv);
            }
            else Debug.LogWarning("SaveLoadUIManager not found; chest won't persist.");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Chest '{chestName}' created successfully.");
        }
    }
}
