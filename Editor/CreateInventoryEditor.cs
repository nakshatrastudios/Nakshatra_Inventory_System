using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins.Editor
{
    public class CreateInventoryEditor : EditorWindow
    {
        private int numSlots;
        private int slotsPerRow;
        private int rowsPerPage;
        private Sprite backgroundSprite;
        private GameObject slotPrefab;
        private GameObject nextPageButtonPrefab;
        private GameObject previousPageButtonPrefab;
        private GameObject hpBarPrefab;
        private GameObject manaBarPrefab;
        private GameObject staminaBarPrefab;
        private int paddingLeft;
        private int paddingRight;
        private int paddingTop;
        private int paddingBottom;
        private Vector2 gridSpacing;
        private Vector2 cellSize;
        private float backgroundPaddingPercentage = 17f;

        private List<Currency> currencies = new List<Currency>();
        private Dictionary<string, int> currencyAmounts = new Dictionary<string, int>();

        private ItemDB itemDB;

        //[MenuItem("Nakshatra/Create Inventory")]
        public static void ShowWindow()
        {
            GetWindow<CreateInventoryEditor>("Create Inventory");
        }
        public void OnGUI()
        {
            GUILayout.Label("Inventory Settings", EditorStyles.boldLabel);
            numSlots = EditorGUILayout.IntField("Number of Slots", numSlots);
            slotsPerRow = EditorGUILayout.IntField("Slots Per Row", slotsPerRow);
            rowsPerPage = EditorGUILayout.IntField("Rows Per Page", rowsPerPage);
            backgroundSprite = (Sprite)EditorGUILayout.ObjectField("Background Sprite", backgroundSprite, typeof(Sprite), false);
            slotPrefab = (GameObject)EditorGUILayout.ObjectField("Slot Prefab", slotPrefab, typeof(GameObject), false);
            nextPageButtonPrefab = (GameObject)EditorGUILayout.ObjectField("Next Page Button Prefab", nextPageButtonPrefab, typeof(GameObject), false);
            previousPageButtonPrefab = (GameObject)EditorGUILayout.ObjectField("Previous Page Button Prefab", previousPageButtonPrefab, typeof(GameObject), false);

            GUILayout.Label("Grid Layout Settings", EditorStyles.boldLabel);
            paddingLeft = EditorGUILayout.IntField("Padding Left", paddingLeft);
            paddingRight = EditorGUILayout.IntField("Padding Right", paddingRight);
            paddingTop = EditorGUILayout.IntField("Padding Top", paddingTop);
            paddingBottom = EditorGUILayout.IntField("Padding Bottom", paddingBottom);
            gridSpacing = EditorGUILayout.Vector2Field("Grid Spacing", gridSpacing);
            cellSize = EditorGUILayout.Vector2Field("Cell Size", cellSize);

            GUILayout.Label("Background Padding Percentage", EditorStyles.boldLabel);
            backgroundPaddingPercentage = EditorGUILayout.Slider("Padding Percentage", backgroundPaddingPercentage, 0, 20);

            GUILayout.Label("HUD Settings", EditorStyles.boldLabel);
            hpBarPrefab = (GameObject)EditorGUILayout.ObjectField("HP Bar Prefab", hpBarPrefab, typeof(GameObject), false);
            manaBarPrefab = (GameObject)EditorGUILayout.ObjectField("Mana Bar Prefab", manaBarPrefab, typeof(GameObject), false);
            staminaBarPrefab = (GameObject)EditorGUILayout.ObjectField("Stamina Bar Prefab", staminaBarPrefab, typeof(GameObject), false);

            GUILayout.Label("Currency Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Currency"))
            {
                var newCurrency = new Currency("New Currency", null, 1);
                currencies.Add(newCurrency);
                currencyAmounts[newCurrency.name] = 0;
            }

            for (int i = 0; i < currencies.Count; i++)
            {
                GUILayout.BeginHorizontal();
                currencies[i].name = EditorGUILayout.TextField("Name", currencies[i].name);
                currencies[i].icon = (Sprite)EditorGUILayout.ObjectField("Icon", currencies[i].icon, typeof(Sprite), false);
                currencies[i].conversionRate = EditorGUILayout.IntField("Conversion Rate", currencies[i].conversionRate);
                if (GUILayout.Button("Remove"))
                {
                    currencyAmounts.Remove(currencies[i].name);
                    currencies.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }

            itemDB = (ItemDB)EditorGUILayout.ObjectField("Item Database", itemDB, typeof(ItemDB), false);

            if (GUILayout.Button("Create Inventory"))
            {
                CreateInventory();
            }
        }

        private void CreateInventory()
        {
            EditorUtilities.EnsureEventSystem();

            // Find or add Player
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject with tag 'Player' not found.");
                return;
            }

            // Inventory
            Inventory inventoryComponent = player.AddComponent<Inventory>();
            inventoryComponent.itemDB = itemDB;
            if (itemDB != null)
                inventoryComponent.PopulateAllItemsList();

            // Currency Manager (with conversionRate now set)
            CurrencyManager currencyManager = player.AddComponent<CurrencyManager>();
            foreach (var currency in currencies)
            {
                currencyManager.currencies.Add(new CurrencyManager.Currency
                {
                    name = currency.name,
                    icon = currency.icon,
                    amount = 0,
                    conversionRate = currency.conversionRate
                });
            }

            // Player Status
            PlayerStatus playerStatus = player.AddComponent<PlayerStatus>();

            // Canvas setup
            GameObject canvasObject = new GameObject("InventorySystem");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();

            // Inventory Panel
            GameObject inventoryObject = new GameObject("Inventory");
            inventoryObject.transform.SetParent(canvasObject.transform, false);
            if (backgroundSprite != null)
            {
                Image bg = inventoryObject.AddComponent<Image>();
                bg.sprite = backgroundSprite;
                RectTransform rt = inventoryObject.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = Vector2.zero;
            }

            // Grid
            GameObject gridGO = new GameObject("InventoryGrid");
            gridGO.transform.SetParent(inventoryObject.transform, false);
            GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
            RectTransform gridRT = gridGO.GetComponent<RectTransform>();
            grid.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            grid.spacing = gridSpacing;
            grid.cellSize = cellSize;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = slotsPerRow;

            float gridWidth = cellSize.x * slotsPerRow + gridSpacing.x * (slotsPerRow - 1) + paddingLeft + paddingRight;
            float gridHeight = cellSize.y * rowsPerPage + gridSpacing.y * (rowsPerPage - 1) + paddingTop + paddingBottom;
            gridRT.sizeDelta = new Vector2(gridWidth, gridHeight);

            RectTransform invRT = inventoryObject.GetComponent<RectTransform>();
            float padFactor = 1 + backgroundPaddingPercentage / 100f;
            invRT.sizeDelta = new Vector2(gridWidth * padFactor, gridHeight * padFactor);

            // Navigation Buttons
            GameObject nextBtn = null, prevBtn = null;
            if (nextPageButtonPrefab != null)
            {
                nextBtn = Instantiate(nextPageButtonPrefab, inventoryObject.transform);
                RectTransform rt = nextBtn.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-40, -25);
            }
            if (previousPageButtonPrefab != null)
            {
                prevBtn = Instantiate(previousPageButtonPrefab, inventoryObject.transform);
                RectTransform rt = prevBtn.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-100, -25);
            }

            // Assign Inventory references
            inventoryComponent.totalSlots = numSlots;
            inventoryComponent.columns = slotsPerRow;
            inventoryComponent.rows = rowsPerPage;
            inventoryComponent.slotPrefab = slotPrefab;
            inventoryComponent.inventoryGrid = gridGO.transform;
            if (nextBtn != null) inventoryComponent.nextPageButton = nextBtn.GetComponent<Button>();
            if (prevBtn != null) inventoryComponent.previousPageButton = prevBtn.GetComponent<Button>();

            // Pagination
            InventoryPagination pagination = inventoryObject.AddComponent<InventoryPagination>();
            pagination.inventory = inventoryComponent;
            if (nextBtn != null) pagination.nextPageButton = nextBtn.GetComponent<Button>();
            if (prevBtn != null) pagination.previousPageButton = prevBtn.GetComponent<Button>();

            // HUD
            GameObject hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(canvasObject.transform, false);
            RectTransform hudRT = hudGO.AddComponent<RectTransform>();
            hudRT.anchorMin = hudRT.anchorMax = new Vector2(0, 0);
            hudRT.pivot = new Vector2(0, 0);
            hudRT.anchoredPosition = new Vector2(10, 10);

            GameObject hpBar = null, manaBar = null, stamBar = null;
            if (hpBarPrefab != null)
            {
                hpBar = Instantiate(hpBarPrefab, hudGO.transform);
                RectTransform rt = hpBar.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(10, -10);
            }
            if (manaBarPrefab != null)
            {
                manaBar = Instantiate(manaBarPrefab, hudGO.transform);
                RectTransform rt = manaBar.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(10, -40);
            }
            if (staminaBarPrefab != null)
            {
                stamBar = Instantiate(staminaBarPrefab, hudGO.transform);
                RectTransform rt = stamBar.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(10, -70);
            }

            HUD hudComp = hudGO.AddComponent<HUD>();
            if (hpBar != null) hudComp.healthBarFill = hpBar.transform.GetChild(0).GetComponent<Image>();
            if (manaBar != null) hudComp.manaBarFill = manaBar.transform.GetChild(0).GetComponent<Image>();
            if (stamBar != null) hudComp.staminaBarFill = stamBar.transform.GetChild(0).GetComponent<Image>();

            // Currency Display
            GameObject currencyDisp = new GameObject("CurrencyDisplay");
            currencyDisp.transform.SetParent(hudGO.transform, false);
            RectTransform cdRT = currencyDisp.AddComponent<RectTransform>();
            cdRT.anchorMin = cdRT.anchorMax = new Vector2(1, 1);
            cdRT.pivot = new Vector2(1, 1);
            cdRT.anchoredPosition = new Vector2(-10, -10);
            cdRT.sizeDelta = new Vector2(200, 50);

            HorizontalLayoutGroup clg = currencyDisp.AddComponent<HorizontalLayoutGroup>();
            clg.childAlignment = TextAnchor.UpperRight;
            clg.spacing = 10;
            clg.childControlHeight = true;
            clg.childControlWidth = true;
            clg.reverseArrangement = true;

            foreach (var currency in currencies)
            {
                GameObject cObj = new GameObject(currency.name + "Icon");
                cObj.transform.SetParent(currencyDisp.transform, false);

                Image img = cObj.AddComponent<Image>();
                img.sprite = currency.icon;
                img.rectTransform.sizeDelta = new Vector2(24, 24);

                var txtGO = new GameObject(currency.name + "Text", typeof(Text));
                txtGO.transform.SetParent(cObj.transform, false);
                Text txt = txtGO.GetComponent<Text>();
                txt.text = "0";
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 18;
                txt.alignment = TextAnchor.MiddleLeft;
                RectTransform trt = txtGO.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2(1, 0);
                trt.anchorMax = new Vector2(1, 0);
                trt.pivot = new Vector2(0, 0);
                trt.anchoredPosition = Vector2.zero;

                var cm = currencyManager.currencies.Find(c => c.name == currency.name);
                if (cm != null)
                    cm.currencyText = txt;
            }

            // Save/Load UI
            GameObject saveLoadGO = new GameObject("SaveLoadUI");
            saveLoadGO.transform.SetParent(canvasObject.transform, false);
            var slu = saveLoadGO.AddComponent<SaveLoadUIManager>();
            var slm = saveLoadGO.AddComponent<SaveLoadManager>();
            var equipComp = player.GetComponent<Equipment>();
            var qaBar = Object.FindObjectOfType<QuickAccessBar>();
            slu.saveLoadManager = slm;
            slu.inventory = inventoryComponent;
            slu.equipment = equipComp;
            slu.quickAccessBar = qaBar;
            slu.currencyManager = currencyManager;
            slu.playerStatus = playerStatus;
            slu.playerTransform = player.transform;

            GameObject saveGO = new GameObject("SaveButton");
            saveGO.transform.SetParent(saveLoadGO.transform, false);
            saveGO.AddComponent<Image>();
            var saveBtn = saveGO.AddComponent<Button>();
            RectTransform srt = saveGO.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(120, 30);
            srt.anchoredPosition = new Vector2(-70, 0);

            var sTxtGO = new GameObject("SaveText", typeof(Text));
            sTxtGO.transform.SetParent(saveGO.transform, false);
            Text sTxt = sTxtGO.GetComponent<Text>();
            sTxt.text = "Save";
            sTxt.alignment = TextAnchor.MiddleCenter;
            sTxt.resizeTextForBestFit = true;
            sTxt.resizeTextMinSize = 10;
            sTxt.resizeTextMaxSize = 40;
            sTxt.color = Color.black;
            RectTransform srTxtRT = sTxtGO.GetComponent<RectTransform>();
            srTxtRT.anchorMin = Vector2.zero;
            srTxtRT.anchorMax = Vector2.one;
            srTxtRT.offsetMin = srTxtRT.offsetMax = Vector2.zero;

            GameObject loadGO = new GameObject("LoadButton");
            loadGO.transform.SetParent(saveLoadGO.transform, false);
            loadGO.AddComponent<Image>();
            var loadBtn = loadGO.AddComponent<Button>();
            RectTransform lrt = loadGO.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(120, 30);
            lrt.anchoredPosition = new Vector2(70, 0);

            var lTxtGO = new GameObject("LoadText", typeof(Text));
            lTxtGO.transform.SetParent(loadGO.transform, false);
            Text lTxt = lTxtGO.GetComponent<Text>();
            lTxt.text = "Load";
            lTxt.alignment = TextAnchor.MiddleCenter;
            lTxt.resizeTextForBestFit = true;
            lTxt.resizeTextMinSize = 10;
            lTxt.resizeTextMaxSize = 40;
            lTxt.color = Color.black;
            RectTransform lrTxtRT = lTxtGO.GetComponent<RectTransform>();
            lrTxtRT.anchorMin = Vector2.zero;
            lrTxtRT.anchorMax = Vector2.one;
            lrTxtRT.offsetMin = lrTxtRT.offsetMax = Vector2.zero;

            slu.saveButton = saveBtn;
            slu.loadButton = loadBtn;

            // UI Manager
            GameObject uiMgrGO = new GameObject("UI Manager");
            uiMgrGO.transform.SetParent(canvasObject.transform, false);
            uiMgrGO.AddComponent<UIManager>();

            Debug.Log("Inventory, HUD, Currency, Save/Load UI, and UI Manager created successfully.");
        }

        [System.Serializable]
        public class Currency
        {
            public string name;
            public Sprite icon;
            public int conversionRate;

            public Currency(string name, Sprite icon, int conversionRate)
            {
                this.name = name;
                this.icon = icon;
                this.conversionRate = conversionRate;
            }
        }
    }
}
