// Assets/NakshatraStudios/InventorySystem/Editor/InventoryItemEditor.cs
using UnityEngine;
using UnityEditor;
using Nakshatra.Plugins;
using System;
using System.Linq;

namespace Nakshatra.Plugins.Editor
{
    [CustomEditor(typeof(InventoryItem))]
    public class InventoryItemEditor : UnityEditor.Editor
    {
        // --- all the serialized props ---
        SerializedProperty
            pItemName,
            pItemDescription,
            pItemIcon,
            pItemType,

            pIsStackable,
            pMaxStackSize,

            pEquipmentCategory,
            pWeaponType,
            pIsMainHand,
            pIsOffHand,

            pStats,

            pSetBone,
            pSelectedBoneNames,
            pItemPositions,
            pItemRotations,
            pItemScale,

            pItemPrefabs,
            pItemPickupPrefab,
            pPickupTextPrefab,

            pToggleSiblings,
            pParentToggles,

            pPickupSound,
            pOnEquipSound,
            pOnUnequipSound,

            pCurrencyType,  // our dropdown
            pBasePrice;     // our manual field

        void OnEnable()
        {
            // cache everything
            pItemName           = serializedObject.FindProperty("itemName");
            pItemDescription    = serializedObject.FindProperty("itemDescription");
            pItemIcon           = serializedObject.FindProperty("itemIcon");
            pItemType           = serializedObject.FindProperty("itemType");

            pIsStackable        = serializedObject.FindProperty("isStackable");
            pMaxStackSize       = serializedObject.FindProperty("maxStackSize");

            pEquipmentCategory  = serializedObject.FindProperty("equipmentCategory");
            pWeaponType         = serializedObject.FindProperty("weaponType");
            pIsMainHand         = serializedObject.FindProperty("isMainHand");
            pIsOffHand          = serializedObject.FindProperty("isOffHand");

            pStats              = serializedObject.FindProperty("stats");

            pSetBone            = serializedObject.FindProperty("setBone");
            pSelectedBoneNames  = serializedObject.FindProperty("selectedBoneNames");
            pItemPositions      = serializedObject.FindProperty("itemPositions");
            pItemRotations      = serializedObject.FindProperty("itemRotations");
            pItemScale          = serializedObject.FindProperty("itemScale");

            pItemPrefabs        = serializedObject.FindProperty("itemPrefabs");
            pItemPickupPrefab   = serializedObject.FindProperty("itemPickupPrefab");
            pPickupTextPrefab   = serializedObject.FindProperty("pickupTextPrefab");

            pToggleSiblings     = serializedObject.FindProperty("toggleSiblings");
            pParentToggles      = serializedObject.FindProperty("parentToggles");

            pPickupSound        = serializedObject.FindProperty("pickupSound");
            pOnEquipSound       = serializedObject.FindProperty("onEquipSound");
            pOnUnequipSound     = serializedObject.FindProperty("onUnequipSound");

            // economy
            pCurrencyType       = serializedObject.FindProperty("currencyType");
            pBasePrice          = serializedObject.FindProperty("basePrice");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // --- Core fields ---
            EditorGUILayout.PropertyField(pItemName);
            EditorGUILayout.PropertyField(pItemDescription);
            EditorGUILayout.PropertyField(pItemIcon);
            EditorGUILayout.PropertyField(pItemType);

            // stacking
            EditorGUILayout.PropertyField(pIsStackable);
            if (pIsStackable.boolValue)
                EditorGUILayout.PropertyField(pMaxStackSize);

            // equipment
            EditorGUILayout.PropertyField(pEquipmentCategory);
            if ((EquipmentCategory)pEquipmentCategory.enumValueIndex == EquipmentCategory.Weapon)
            {
                EditorGUILayout.PropertyField(pWeaponType);
                if ((WeaponType)pWeaponType.enumValueIndex == WeaponType.OneHand)
                {
                    EditorGUILayout.PropertyField(pIsMainHand);
                    EditorGUILayout.PropertyField(pIsOffHand);
                }
            }

            // stats list
            EditorGUILayout.PropertyField(pStats, true);

            // bone attachments
            EditorGUILayout.PropertyField(pSetBone, new GUIContent("Set Bone"));
            if (pSetBone.boolValue)
            {
                EditorGUILayout.LabelField("Bone Attachments", EditorStyles.boldLabel);
                int count = pSelectedBoneNames.arraySize;
                for (int i = 0; i < count; i++)
                {
                    var nameProp = pSelectedBoneNames.GetArrayElementAtIndex(i);
                    var posProp  = pItemPositions.GetArrayElementAtIndex(i);
                    var rotProp  = pItemRotations.GetArrayElementAtIndex(i);
                    var sProp    = pItemScale.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.PropertyField(nameProp, new GUIContent($"Bone Name #{i + 1}"));
                        EditorGUILayout.PropertyField(posProp,  new GUIContent("Position"));
                        EditorGUILayout.PropertyField(rotProp,  new GUIContent("Rotation"));
                        EditorGUILayout.PropertyField(sProp,    new GUIContent("Scale"));
                        if (GUILayout.Button("Remove"))
                        {
                            pSelectedBoneNames.DeleteArrayElementAtIndex(i);
                            pItemPositions.DeleteArrayElementAtIndex(i);
                            pItemRotations.DeleteArrayElementAtIndex(i);
                            pItemScale.DeleteArrayElementAtIndex(i);
                        }
                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("Add Bone"))
                {
                    pSelectedBoneNames.InsertArrayElementAtIndex(count);
                    pItemPositions.InsertArrayElementAtIndex(count);
                    pItemRotations.InsertArrayElementAtIndex(count);
                    pItemScale.InsertArrayElementAtIndex(count);
                    // init
                    pSelectedBoneNames.GetArrayElementAtIndex(count).stringValue = "";
                    pItemPositions.GetArrayElementAtIndex(count).vector3Value   = Vector3.zero;
                    pItemRotations.GetArrayElementAtIndex(count).vector3Value   = Vector3.zero;
                    pItemScale.GetArrayElementAtIndex(count).vector3Value       = Vector3.one;
                }
            }

            // item pickup & extra prefabs
            EditorGUILayout.PropertyField(pItemPrefabs, true);
            EditorGUILayout.PropertyField(pItemPickupPrefab);
            EditorGUILayout.PropertyField(pPickupTextPrefab);

            // sibling toggles
            EditorGUILayout.PropertyField(pToggleSiblings, new GUIContent("Toggle Siblings"));
            if (pToggleSiblings.boolValue)
            {
                EditorGUILayout.LabelField("Parent Toggles", EditorStyles.boldLabel);
                for (int i = 0; i < pParentToggles.arraySize; i++)
                    EditorGUILayout.PropertyField(pParentToggles.GetArrayElementAtIndex(i), new GUIContent($"Parent Toggle #{i+1}"), true);
                if (GUILayout.Button("Add Parent Toggle"))
                    pParentToggles.InsertArrayElementAtIndex(pParentToggles.arraySize);
            }

            // sounds
            EditorGUILayout.PropertyField(pPickupSound);
            EditorGUILayout.PropertyField(pOnEquipSound);
            EditorGUILayout.PropertyField(pOnUnequipSound);

            // --- Single Economy section ---
            EditorGUILayout.Space();
            GUILayout.Label("Economy", EditorStyles.boldLabel);

            // currencyType dropdown
            var cm = FindObjectOfType<CurrencyManager>();
            string[] options = (cm != null && cm.currencies != null && cm.currencies.Count > 0)
                ? cm.currencies.Select(c => c.name).ToArray()
                : new[] { "<none>" };
            int idx = Array.IndexOf(options, pCurrencyType.stringValue);
            if (idx < 0) idx = 0;
            int sel = EditorGUILayout.Popup("Currency Type", idx, options);
            pCurrencyType.stringValue = options[sel];

            // basePrice manual int field (no duplicate header)
            pBasePrice.intValue = EditorGUILayout.IntField("Base Price", pBasePrice.intValue);

            // draw remaining properties (excluding ones we've handled)
            DrawPropertiesExcluding(serializedObject,
                "itemName","itemDescription","itemIcon","itemType",
                "isStackable","maxStackSize","equipmentCategory","weaponType","isMainHand","isOffHand",
                "stats","setBone","selectedBoneNames","itemPositions","itemRotations","itemScale",
                "itemPrefabs","itemPickupPrefab","pickupTextPrefab",
                "toggleSiblings","parentToggles",
                "pickupSound","onEquipSound","onUnequipSound",
                "currencyType","basePrice"
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
