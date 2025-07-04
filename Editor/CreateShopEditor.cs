// Assets/NakshatraStudios/InventorySystem/Editor/CreateShopEditor.cs
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins.Editor
{
    public class CreateShopEditor : EditorWindow
    {
        private string shopName = "New Shop";
        private GameObject rowPrefab;
        private Sprite    backgroundSprite;

        // Mirror of ShopInventory.ShopItemEntry for editor editing
        private struct Entry
        {
            public InventoryItem item;
            public int           quantity;
            public bool          infinite;
            public string        currencyType;
            public int           price;
        }
        private List<Entry> entries = new List<Entry>();
        private Vector2     scrollPos;

        [MenuItem("Tools/Nakshatra Studios/Inventory System/Create Shop")]
        public static void ShowWindow() => GetWindow<CreateShopEditor>("Create Shop");

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Shop Settings", EditorStyles.boldLabel);
            shopName = EditorGUILayout.TextField("Shop Name", shopName);

            GUILayout.Space(6);
            GUILayout.Label("UI Prefab & Background", EditorStyles.boldLabel);
            rowPrefab        = (GameObject)EditorGUILayout.ObjectField("Row Prefab",        rowPrefab,       typeof(GameObject), false);
            backgroundSprite = (Sprite)   EditorGUILayout.ObjectField("Background Sprite", backgroundSprite, typeof(Sprite),      false);

            GUILayout.Space(6);
            GUILayout.Label("Manual Shop Entries", EditorStyles.boldLabel);
            if (GUILayout.Button("➕ Add Entry")) 
                entries.Add(new Entry { item = null, quantity = 1, infinite = false, currencyType = "", price = 0 });

            // Header row
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
              GUILayout.Label("Item",     GUILayout.Width(150));
              GUILayout.Label("Qty",      GUILayout.Width(40));
              GUILayout.Label("∞",        GUILayout.Width(20));
              GUILayout.Label("Currency", GUILayout.Width(80));
              GUILayout.Label("Price",    GUILayout.Width(50));
              GUILayout.FlexibleSpace();
              GUILayout.Label("–",        GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            // Entries
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                EditorGUILayout.BeginHorizontal();
                  e.item         = (InventoryItem)EditorGUILayout.ObjectField(e.item,         typeof(InventoryItem), false, GUILayout.Width(150));
                  e.quantity     = EditorGUILayout.IntField(e.quantity,               GUILayout.Width(40));
                  e.infinite     = EditorGUILayout.Toggle(e.infinite,                GUILayout.Width(20));
                  e.currencyType = EditorGUILayout.TextField(e.currencyType,         GUILayout.Width(80));
                  e.price        = EditorGUILayout.IntField(e.price,                  GUILayout.Width(50));
                  if (GUILayout.Button("✖", GUILayout.Width(20)))
                  {
                      entries.RemoveAt(i);
                      i--;
                  }
                EditorGUILayout.EndHorizontal();
                entries[i] = e;
            }

            GUILayout.Space(8);
            bool canCreate = !string.IsNullOrWhiteSpace(shopName)
                         && rowPrefab != null
                         && entries.Count > 0;
            using (new EditorGUI.DisabledScope(!canCreate))
            {
                if (GUILayout.Button("🛒 Create Shop", GUILayout.Height(30)))
                    CreateShop();
            }

            EditorGUILayout.EndScrollView();
        }

        public void CreateShop()
        {
            // --- 1. Root ---
            var root = new GameObject(shopName);
            Undo.RegisterCreatedObjectUndo(root, "Create Shop");

            // --- 2. Canvas ---
            var canvasGO = new GameObject("ShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(root.transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            // --- 3. Panels & Content ---
            RectTransform shopContent = CreateScrollPanel(canvasGO.transform, "ShopPanel",     new Vector2(0f,0.1f),   new Vector2(0.45f,0.9f));
            RectTransform invContent  = CreateScrollPanel(canvasGO.transform, "InventoryPanel", new Vector2(0.55f,0.1f),  new Vector2(1f,  0.9f));

            // --- 4. ShopInventory component wiring ---
            var shopInv = root.AddComponent<ShopInventory>();
            shopInv.shopName         = shopName;
            shopInv.rowPrefab        = rowPrefab;
            shopInv.backgroundSprite = backgroundSprite;
            shopInv.shopContent      = shopContent;
            shopInv.invContent       = invContent;

            // 5. Copy over your entries
            shopInv.manualItems = new List<ShopInventory.ShopItemEntry>();
            foreach (var e in entries)
            {
                shopInv.manualItems.Add(new ShopInventory.ShopItemEntry
                {
                    item         = e.item,
                    quantity     = e.infinite ? int.MaxValue : e.quantity,
                    infinite     = e.infinite,
                    currencyType = e.currencyType,
                    price        = e.price
                });
            }

            // 6. Force editor‐time build
            EditorUtility.SetDirty(shopInv);

            // 7. Mark scene dirty & select
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            Debug.Log($"Created Shop '{shopName}' with {shopInv.manualItems.Count} entries.", root);
        }

        /// <summary>
        /// Creates a ScrollRect panel with background, masking, and Content container.
        /// Returns the RectTransform of the "Content" object.
        /// </summary>
        private RectTransform CreateScrollPanel(Transform parent, string name, Vector2 aMin, Vector2 aMax)
        {
            // Panel
            var panelGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            panelGO.transform.SetParent(parent, false);
            var rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var img = panelGO.GetComponent<Image>();
            if (backgroundSprite != null)
            {
                img.sprite = backgroundSprite;
                img.type   = Image.Type.Sliced;
            }
            else
            {
                img.color = new Color(0,0,0,0.5f);
            }

            var scroll = panelGO.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;
            scroll.scrollSensitivity = 20f;

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(CanvasRenderer), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(panelGO.transform, false);
            var ctr = contentGO.GetComponent<RectTransform>();
            ctr.anchorMin        = new Vector2(0f, 1f);
            ctr.anchorMax        = new Vector2(1f, 1f);
            ctr.pivot            = new Vector2(0.5f, 1f);
            ctr.anchoredPosition = Vector2.zero;
            ctr.sizeDelta        = Vector2.zero;

            var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing              = 8;
            vlg.childAlignment       = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.padding              = new RectOffset(8,8,100,8);

            var csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content  = ctr;
            scroll.viewport = rt;

            return ctr;
        }
    }
}
