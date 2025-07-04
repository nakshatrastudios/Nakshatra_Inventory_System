using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins.Editor
{
    /// <summary>
    /// Main Inventory Manager window to create Inventory, Items, Quick Access Bar,
    /// Item Database, and Equipment Panel directly from one interface.
    /// </summary>
    public class InventoryManagerEditor : InventoryManagerBaseEditor
    {
        private enum Tab
        {
            CreateInventory,
            CreateItem,
            CreateQuickAccessBar,
            CreateChest,
            CreateShop,
            ItemDatabase,
            CreateEquipmentPanel,
            CreateDescriptionPanel
        }

        private Tab currentTab = Tab.CreateInventory;

        private CreateInventoryEditor createInventoryEditor;
        private CreateItemEditor createItemEditor;
        private CreateQuickAccessBarEditor createQuickAccessBarEditor;
        private CreateChestEditor createChestEditor;
        private CreateShopEditor createShopEditor;
        private ItemDatabaseEditor itemDatabaseEditor;
        private GameObject equipmentPanelPrefab;
        private ItemDB itemDB;
        private GameObject playerDisplayCameraPrefab;

        private GameObject descriptionPanelPrefab;

        [MenuItem("Tools/Nakshatra Studios/Nakshatra Inventory System/Inventory Manager")]
        public static void ShowWindow()
        {
            GetWindow<InventoryManagerEditor>("Inventory System");
        }

        private void OnEnable()
        {
            createInventoryEditor = new CreateInventoryEditor();
            createItemEditor = new CreateItemEditor();
            createQuickAccessBarEditor = new CreateQuickAccessBarEditor();
            createChestEditor = new CreateChestEditor();
            createShopEditor = new CreateShopEditor();
            itemDatabaseEditor = new ItemDatabaseEditor();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawTabs();

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 160), GUILayout.ExpandWidth(true));
            BeginScroll();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            switch (currentTab)
            {
                case Tab.CreateInventory:
                    createInventoryEditor.OnGUI();
                    break;
                case Tab.CreateItem:
                    createItemEditor.OnGUI();
                    break;
                case Tab.CreateQuickAccessBar:
                    createQuickAccessBarEditor.OnGUI();
                    break;
                case Tab.CreateChest:
                    createChestEditor.OnGUI();
                    break;
                case Tab.CreateShop:                 
                    createShopEditor.OnGUI();
                    break;
                case Tab.ItemDatabase:
                    itemDatabaseEditor.OnGUI();
                    break;
                case Tab.CreateEquipmentPanel:
                    DrawCreateEquipmentPanel();
                    break;
                case Tab.CreateDescriptionPanel:
                    DrawCreateDescriptionPanel();
                    break;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
            EndScroll();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.Label("Inventory Manager", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Inventory")) currentTab = Tab.CreateInventory;
            if (GUILayout.Button("Create Item")) currentTab = Tab.CreateItem;
            if (GUILayout.Button("Create Quick Access Bar")) currentTab = Tab.CreateQuickAccessBar;
            if (GUILayout.Button("Create Chest")) currentTab = Tab.CreateChest;
            if (GUILayout.Button("Create Shop")) currentTab = Tab.CreateShop;
            if (GUILayout.Button("Item Database")) currentTab = Tab.ItemDatabase;
            if (GUILayout.Button("Create Equipment Panel")) currentTab = Tab.CreateEquipmentPanel;
            if (GUILayout.Button("Create Description Panel")) currentTab = Tab.CreateDescriptionPanel;

            EditorGUILayout.EndVertical();
        }

        private void DrawCreateEquipmentPanel()
        {
            GUILayout.Label("Equipment Panel Setup", EditorStyles.boldLabel);
            equipmentPanelPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Equipment Panel Prefab",
                equipmentPanelPrefab,
                typeof(GameObject),
                false
            );

            itemDB = (ItemDB)EditorGUILayout.ObjectField(
                "Item Database",
                itemDB,
                typeof(ItemDB),
                false
            );

            playerDisplayCameraPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Player Display Camera Prefab",
                playerDisplayCameraPrefab,
                typeof(GameObject),
                false
            );

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(equipmentPanelPrefab == null || itemDB == null || playerDisplayCameraPrefab == null);
            if (GUILayout.Button("Create Equipment Panel"))
                InstantiateEquipmentPanel();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawCreateDescriptionPanel()
        {
            GUILayout.Label("Description Panel Setup", EditorStyles.boldLabel);

            descriptionPanelPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Description Panel Prefab",
                descriptionPanelPrefab,
                typeof(GameObject),
                false
            );

            EditorGUI.BeginDisabledGroup(descriptionPanelPrefab == null);
            if (GUILayout.Button("Create Description Panel"))
                InstantiateDescriptionPanel();
            EditorGUI.EndDisabledGroup();
        }

        private void InstantiateDescriptionPanel()
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene. Please add one first.");
                return;
            }

            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(
                descriptionPanelPrefab,
                canvas.gameObject.scene);

            Undo.RegisterCreatedObjectUndo(instance, "Create Description Panel");
            instance.transform.SetParent(canvas.transform, worldPositionStays: false);

            instance.name = "DescriptionPanel";
            instance.SetActive(false);

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = instance;

            Debug.Log("Description Panel instantiated from prefab (hidden by default).");
        }

        private void InstantiateEquipmentPanel()
        {
            // Instantiate the Equipment Panel prefab
            var panelInstance = PrefabUtility.InstantiatePrefab(equipmentPanelPrefab) as GameObject;
            if (panelInstance == null)
            {
                Debug.LogError("Failed to instantiate Equipment Panel prefab.");
                return;
            }
            Undo.RegisterCreatedObjectUndo(panelInstance, "Create Equipment Panel");

            // Parent panel under the existing InventorySystem Canvas
            GameObject canvasObject = GameObject.Find("InventorySystem");
            if (canvasObject == null)
            {
                Debug.LogError("InventorySystem Canvas not found. Please create the Inventory first.");
                return;
            }
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("GameObject 'InventorySystem' does not have a Canvas component.");
                return;
            }
            panelInstance.transform.SetParent(canvasObject.transform, worldPositionStays: false);
            panelInstance.name = equipmentPanelPrefab.name;

            EditorSceneManager.MarkSceneDirty(panelInstance.scene);
            Selection.activeGameObject = panelInstance;

            // Instantiate and parent the Player Display Camera Prefab under Player
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogError("Player object with tag 'Player' not found.");
                return;
            }

            var cameraInstance = PrefabUtility.InstantiatePrefab(playerDisplayCameraPrefab) as GameObject;
            if (cameraInstance != null)
            {
                Undo.RegisterCreatedObjectUndo(cameraInstance, "Create Display Camera");
                cameraInstance.transform.SetParent(playerObj.transform, worldPositionStays: false);
                cameraInstance.name = playerDisplayCameraPrefab.name;
            }
            else
            {
                Debug.LogError("Failed to instantiate Player Display Camera prefab.");
            }

            // Ensure the Player has an Equipment component
            var equip = playerObj.GetComponent<Equipment>();
            if (equip == null)
                equip = Undo.AddComponent<Equipment>(playerObj);

            equip.itemDB = itemDB;
            equip.PopulateAllItemsList();

            // Locate the Equipment root under the panel instance
            var root = panelInstance.transform.Find("Equipment");
            if (root == null)
            {
                Debug.LogError("Equipment root not found in panel prefab.");
                return;
            }

            // Assign all equipment slots
            AssignSlot(ref equip.helmetSlot, root, "HelmetSlot");
            AssignSlot(ref equip.shoulderSlot, root, "ShoulderSlot");
            AssignSlot(ref equip.torsoSlot, root, "TorsoSlot");
            AssignSlot(ref equip.pantsSlot, root, "PantSlot");
            AssignSlot(ref equip.glovesSlot, root, "GlovesSlot");
            AssignSlot(ref equip.bootsSlot, root, "BootsSlot");
            AssignSlot(ref equip.cloakSlot, root, "CloakSlot");
            AssignSlot(ref equip.neckSlot, root, "NeckSlot");
            AssignSlot(ref equip.earRingSlot, root, "EarRingSlot");
            AssignSlot(ref equip.beltSlot, root, "BeltSlot");
            AssignSlot(ref equip.ring1Slot, root, "RingSlot1");
            AssignSlot(ref equip.ring2Slot, root, "RingSlot2");
            AssignSlot(ref equip.mainHandSlot, root, "MainHandSlot");
            AssignSlot(ref equip.offHandSlot, root, "OffHandSlot");

            Debug.Log("Equipment Panel instantiated under InventorySystem Canvas and Display Camera added under Player.");

            // Assign Equipment reference to SaveLoadUIManager
            var slu = GameObject.FindObjectOfType<SaveLoadUIManager>();
            if (slu != null)
                slu.equipment = equip;
            else
                Debug.LogWarning("SaveLoadUIManager not found in scene. Equipment reference not set.");
        }

        private void AssignSlot(ref InventorySlot slotField, Transform root, string slotName)
        {
            var slotObj = root.Find(slotName)?.gameObject;
            if (slotObj == null)
            {
                Debug.LogWarning($"Slot GameObject '{slotName}' not found.");
                return;
            }
            slotField.slotObject = slotObj;

            var draggable = slotObj.transform.Find("DraggableItem");
            if (draggable != null)
            {
                var icon = draggable.Find("ItemIcon")?.GetComponent<Image>();
                var txt = draggable.Find("StackText")?.GetComponent<Text>();
                if (icon != null) slotField.itemIcon = icon;
                if (txt != null) slotField.stackText = txt;
            }
            slotField.SetItem(null, 0);
        }
    }
}
