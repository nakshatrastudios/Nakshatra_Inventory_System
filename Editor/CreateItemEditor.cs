// Assets/NakshatraStudios/InventorySystem/Editor/CreateItemEditor.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nakshatra.Plugins;

namespace Nakshatra.Plugins.Editor
{
    public class CreateItemEditor : EditorWindow
    {
        [Header("Basic Item Info")]
        private string  itemName        = "New Item";
        private string  itemDescription = "Item Description";
        private ItemType itemType       = ItemType.Other;
        private Sprite  itemIcon;
        private bool    isStackable;
        private int     maxStackSize    = 1;

        [Header("Consumable / Currency Amount")]
        private int amount = 1;

        [Header("Default Base Price (for shop fallback)")]
        private int      priceAmount        = 1;
        private int      priceCurrencyIndex = 0;
        private string[] currencyNames      = new string[0];

        [Header("Equipment Settings")]
        private EquipmentCategory equipmentCategory;
        private WeaponType        weaponType;
        private bool              isMainHand;
        private bool              isOffHand;

        [Header("Item Stats (Consumables & Others)")]
        private List<ItemStat> itemStats = new List<ItemStat>();

        [Header("Pickup Prefabs & Colliders")]
        private GameObject       itemPickupPrefab;
        private GameObject       pickupTextPrefab;
        private List<GameObject> extraPrefabs = new List<GameObject>();
        private float            sphereRadius = 1f;
        private float            sphereHeight = -0.5f;

        [Header("Bone Attachment (Optional)")]
        private bool             setBone;
        private List<string>     selectedBoneNames = new List<string>();
        private List<Vector3>    itemPositions     = new List<Vector3>();
        private List<Vector3>    itemRotations     = new List<Vector3>();
        private List<Vector3>    itemScale         = new List<Vector3>();

        [Header("Modular Sibling Toggles (Optional)")]
        private bool                   toggleSiblings;
        private List<ParentToggleData> parentToggles    = new List<ParentToggleData>();

        [Header("Sounds (Optional)")]
        private AudioClip pickupSound;
        private AudioClip onEquipSound;
        private AudioClip onUnequipSound;

        [MenuItem("Tools/Nakshatra Studios/Inventory System/Create Item")]
        public static void ShowWindow()
        {
            GetWindow<CreateItemEditor>("Create Item");
        }

        private void OnEnable()
        {
            RefreshCurrencyList();
        }

        private void RefreshCurrencyList()
        {
            var cm = FindObjectOfType<CurrencyManager>();
            if (cm != null && cm.currencies != null && cm.currencies.Count > 0)
                currencyNames = cm.currencies.Select(c => c.name).ToArray();
            else
                currencyNames = new[] { "Copper" };

            priceCurrencyIndex = Mathf.Clamp(priceCurrencyIndex, 0, currencyNames.Length - 1);
        }

        public void OnGUI()
        {
            // Keep currencies up to date
            RefreshCurrencyList();

            GUILayout.Label("Basic Item Info", EditorStyles.boldLabel);
            itemName        = EditorGUILayout.TextField("Item Name",        itemName);
            itemDescription = EditorGUILayout.TextField("Description",      itemDescription);
            itemType        = (ItemType)EditorGUILayout.EnumPopup("Item Type", itemType);

            if (itemType != ItemType.Currency)
            {
                itemIcon    = (Sprite)EditorGUILayout.ObjectField("Icon",       itemIcon, typeof(Sprite), false);
                isStackable = EditorGUILayout.Toggle("Is Stackable", isStackable);
                if (isStackable)
                    maxStackSize = EditorGUILayout.IntField("Max Stack Size", maxStackSize);
            }

            if (itemType == ItemType.Consumable)
            {
                amount = EditorGUILayout.IntField("Consumable Amount", amount);
            }
            else if (itemType == ItemType.Currency)
            {
                amount = EditorGUILayout.IntField("Currency Amount (pickup)", amount);
            }
            else if (itemType == ItemType.Equipment)
            {
                equipmentCategory = (EquipmentCategory)EditorGUILayout.EnumPopup("Category", equipmentCategory);
                if (equipmentCategory == EquipmentCategory.Weapon)
                {
                    weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);
                    if (weaponType == WeaponType.OneHand)
                    {
                        isMainHand = EditorGUILayout.Toggle("Main Hand", isMainHand);
                        isOffHand  = EditorGUILayout.Toggle("Off Hand",  isOffHand);
                    }
                }
            }

            if (itemType != ItemType.Currency)
            {
                GUILayout.Space(6);
                GUILayout.Label("Default Base Price", EditorStyles.boldLabel);
                priceCurrencyIndex = EditorGUILayout.Popup("Currency", priceCurrencyIndex, currencyNames);
                priceAmount        = EditorGUILayout.IntField("Amount", priceAmount);
            }

            if (itemType == ItemType.Consumable || itemType == ItemType.Other)
            {
                GUILayout.Space(6);
                GUILayout.Label("Item Stats", EditorStyles.boldLabel);
                for (int i = 0; i < itemStats.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    itemStats[i].statType = (StatType)EditorGUILayout.EnumPopup(itemStats[i].statType);
                    itemStats[i].value    = EditorGUILayout.IntField(itemStats[i].value);
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        itemStats.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add Stat"))
                    itemStats.Add(new ItemStat());
            }

            GUILayout.Space(6);
            GUILayout.Label("Pickup Settings", EditorStyles.boldLabel);
            itemPickupPrefab = (GameObject)EditorGUILayout.ObjectField("Pickup Prefab",     itemPickupPrefab,     typeof(GameObject), true);
            pickupTextPrefab = (GameObject)EditorGUILayout.ObjectField("Text Prefab",       pickupTextPrefab,     typeof(GameObject), true);

            GUILayout.Space(6);
            GUILayout.Label("Extra Prefabs", EditorStyles.boldLabel);
            for (int i = 0; i < extraPrefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                extraPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab #{i+1}", extraPrefabs[i], typeof(GameObject), true);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    extraPrefabs.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Prefab"))
                extraPrefabs.Add(null);

            GUILayout.Space(6);
            GUILayout.Label("Collider Settings", EditorStyles.boldLabel);
            sphereRadius = EditorGUILayout.FloatField("Sphere Radius", sphereRadius);
            sphereHeight = EditorGUILayout.FloatField("Sphere Height", sphereHeight);

            GUILayout.Space(6);
            setBone = EditorGUILayout.Toggle("Enable Bone Attachment", setBone);
            if (setBone)
            {
                for (int i = 0; i < selectedBoneNames.Count; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    selectedBoneNames[i] = EditorGUILayout.TextField($"Bone #{i+1}", selectedBoneNames[i]);
                    itemPositions[i]     = EditorGUILayout.Vector3Field("Position", itemPositions[i]);
                    itemRotations[i]     = EditorGUILayout.Vector3Field("Rotation", itemRotations[i]);
                    itemScale[i]         = EditorGUILayout.Vector3Field("Scale",    itemScale[i]);
                    if (GUILayout.Button("Remove Bone"))
                    {
                        selectedBoneNames.RemoveAt(i);
                        itemPositions.RemoveAt(i);
                        itemRotations.RemoveAt(i);
                        itemScale.RemoveAt(i);
                    }
                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("Add Bone"))
                {
                    selectedBoneNames.Add("");
                    itemPositions.Add(Vector3.zero);
                    itemRotations.Add(Vector3.zero);
                    itemScale.Add(Vector3.one);
                }
            }

            GUILayout.Space(6);
            toggleSiblings = EditorGUILayout.Toggle("Modular Character Toggle", toggleSiblings);
            if (toggleSiblings)
            {
                // … your existing parentToggles UI …
            }

            GUILayout.Space(6);
            GUILayout.Label("Sounds (Optional)", EditorStyles.boldLabel);
            pickupSound    = (AudioClip)EditorGUILayout.ObjectField("Pickup Sound",    pickupSound,    typeof(AudioClip), false);
            onEquipSound   = (AudioClip)EditorGUILayout.ObjectField("On Equip Sound",  onEquipSound,   typeof(AudioClip), false);
            onUnequipSound = (AudioClip)EditorGUILayout.ObjectField("On Unequip Sound",onUnequipSound, typeof(AudioClip), false);

            GUILayout.Space(10);
            if (GUILayout.Button("Create Item", GUILayout.Height(30)))
                CreateAndSetupItem();
        }

        private void CreateAndSetupItem()
        {
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogError("Item name is required.");
                return;
            }
            if (itemPickupPrefab == null || pickupTextPrefab == null)
            {
                Debug.LogError("Assign both Pickup Prefab and Text Prefab.");
                return;
            }

            // Ensure Resources folder exists
            const string resRoot = "Assets/NakshatraStudios/InventorySystem/Resources";
            if (!AssetDatabase.IsValidFolder(resRoot))
                AssetDatabase.CreateFolder("Assets/NakshatraStudios/InventorySystem", "Resources");

            // Create the InventoryItem asset
            var newItem = ScriptableObject.CreateInstance<InventoryItem>();
            newItem.itemName        = itemName;
            newItem.itemDescription = itemDescription;
            newItem.itemIcon        = itemIcon;
            newItem.isStackable     = isStackable;
            newItem.maxStackSize    = maxStackSize;
            newItem.itemType        = itemType;
            newItem.amount          = amount;

            // Compute basePrice from selected currency tier
            if (itemType != ItemType.Currency)
            {
                var cm = FindObjectOfType<CurrencyManager>();
                if (cm != null && cm.currencies != null && cm.currencies.Count > 0)
                {
                    var tiers = cm.currencies;
                    int n = tiers.Count;
                    // build conversion factors from each tier to lowest
                    float[] factor = new float[n];
                    factor[n - 1] = 1f;
                    for (int i = n - 2; i >= 0; i--)
                        factor[i] = factor[i + 1] * tiers[i + 1].conversionRate;
                    int idx = Mathf.Clamp(priceCurrencyIndex, 0, n - 1);
                    newItem.basePrice = Mathf.RoundToInt(priceAmount * factor[idx]);
                }
                else
                {
                    newItem.basePrice = priceAmount;
                }
            }

            newItem.equipmentCategory = equipmentCategory;
            newItem.weaponType        = weaponType;
            newItem.isMainHand        = isMainHand;
            newItem.isOffHand         = isOffHand;
            newItem.stats             = new List<ItemStat>(itemStats);
            newItem.itemPrefabs       = new List<GameObject>(extraPrefabs);
            newItem.itemPickupPrefab  = itemPickupPrefab;
            newItem.pickupTextPrefab  = pickupTextPrefab;

            if (setBone)
            {
                newItem.setBone           = true;
                newItem.selectedBoneNames = new List<string>(selectedBoneNames);
                newItem.itemPositions     = new List<Vector3>(itemPositions);
                newItem.itemRotations     = new List<Vector3>(itemRotations);
                newItem.itemScale         = new List<Vector3>(itemScale);
            }

            newItem.toggleSiblings = toggleSiblings;
            newItem.parentToggles  = new List<ParentToggleData>(parentToggles);
            newItem.pickupSound    = pickupSound;
            newItem.onEquipSound   = onEquipSound;
            newItem.onUnequipSound = onUnequipSound;

            // Save asset
            string assetPath = $"{resRoot}/{itemName}.asset";
            AssetDatabase.CreateAsset(newItem, assetPath);
            AssetDatabase.SaveAssets();

            // Create runtime pickup prefab
            SetupItem(newItem);

            Debug.Log($"Created item '{itemName}'.");
        }

        private void SetupItem(InventoryItem newItem)
        {
            const string prefabDir = "Assets/NakshatraStudios/InventorySystem/Resources/Items";
            if (!AssetDatabase.IsValidFolder(prefabDir))
                AssetDatabase.CreateFolder("Assets/NakshatraStudios/InventorySystem/Resources", "Items");

            string prefabPath = $"{prefabDir}/{newItem.itemName}.prefab";
            GameObject instance = PrefabUtility.InstantiatePrefab(newItem.itemPickupPrefab) as GameObject;
            instance.name = newItem.itemName;

            var collider = instance.AddComponent<SphereCollider>();
            collider.radius    = sphereRadius;
            collider.center    = new Vector3(0, sphereHeight, 0);
            collider.isTrigger = true;

            if (newItem.itemType == ItemType.Currency)
            {
                var currencyPickup = instance.AddComponent<CurrencyPickup>();
                currencyPickup.pickupTextPrefab = pickupTextPrefab;
                currencyPickup.currencyAmounts.Add(new CurrencyPickup.CurrencyAmount {
                    name   = newItem.itemName,
                    amount = newItem.amount
                });
            }
            else
            {
                var itemPickup = instance.AddComponent<ItemPickup>();
                itemPickup.item             = newItem;
                itemPickup.pickupTextPrefab = pickupTextPrefab;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }
    }
}
