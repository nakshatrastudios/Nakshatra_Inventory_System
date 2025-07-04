using UnityEditor;
using UnityEngine;

namespace Nakshatra.Plugins.Editor
{
    public class ItemDatabaseEditor : EditorWindow
    {
        private ItemDB itemDB;
        private Vector2 itemScrollPos;


        public void OnGUI()
        {
            GUILayout.Label("Item Database", EditorStyles.boldLabel);
            itemDB = (ItemDB)EditorGUILayout.ObjectField("Item Database", itemDB, typeof(ItemDB), false);

            if (itemDB == null)
            {
                EditorGUILayout.HelpBox("Please assign an ItemDB ScriptableObject.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Load Items to Database"))
            {
                LoadItemsToDatabase();
            }

            GUILayout.Label("Items in Database:", EditorStyles.boldLabel);
            itemScrollPos = EditorGUILayout.BeginScrollView(itemScrollPos, GUILayout.Height(400));
            foreach (var item in itemDB.items)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(item.itemName);
                EditorGUILayout.ObjectField(item.itemIcon, typeof(Sprite), false, GUILayout.Width(50), GUILayout.Height(50));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void LoadItemsToDatabase()
        {
            if (itemDB == null)
            {
                Debug.LogError("ItemDB is not assigned.");
                return;
            }

            itemDB.items.Clear();
            InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("");

            foreach (InventoryItem item in allItems)
            {
                itemDB.items.Add(item);
                Debug.Log("Added item to ItemDB: " + item.itemName);
            }

            EditorUtility.SetDirty(itemDB); // Mark the scriptable object as dirty to save changes
            Debug.Log("ItemDB loaded with all items.");
        }
    }
}