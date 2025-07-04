using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Nakshatra.Plugins
{
    [System.Serializable]
    public class ChestSaveData
    {
        public string chestName;
        public List<InventoryItemData> items;
    }

    [System.Serializable]
    public class GameSaveData
    {
        public List<InventoryItemData> inventoryItems;
        public List<InventoryItemData> equipmentItems;
        public List<InventoryItemData> quickAccessItems;
        public List<ChestSaveData>     chestItems;      
        public List<CurrencyData>      currencyAmounts;
        public Vector3                 playerPosition;
        public string                  currentScene;
        public PlayerStatusData        playerStatus;
        public List<SceneObjectManager.ChildObjectState> sceneObjectStates;
    }

    [System.Serializable]
    public class PlayerStatusData
    {
        public int health;
        public int stamina;
        public int mana;
        public int strength;
        public int agility;
        public int intelligence;
        public int attack;
        public int defense;
        public int block;
        public int maxHealth;
        public int maxMana;
        public int maxStamina;
        public int speed;
        public int dexterity;
        public int luck;
    }

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int    quantity;
    }

    public class SaveLoadManager : MonoBehaviour
    {
        private string savePath;

        private void Awake()
        {
            savePath = Application.persistentDataPath + "/game_save.json";
        }

        public void SaveGame(
            Inventory inventory,
            Equipment equipment,
            QuickAccessBar quickAccessBar,
            List<ChestInventory> chests,                  
            CurrencyManager currencyManager,
            PlayerStatus playerStatusComponent,
            Transform playerTransform)
        {
            var data = new GameSaveData
            {
                inventoryItems   = inventory.GetItems(),
                equipmentItems   = equipment.GetItems(),
                quickAccessItems = quickAccessBar.GetItems(),
                chestItems       = chests
                                     .Select(c => new ChestSaveData {
                                         chestName = c.chestName,
                                         items     = c.GetItems()
                                     }).ToList(),
                currencyAmounts  = currencyManager.GetCurrencyData(),
                playerPosition   = playerTransform.position,
                currentScene     = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                playerStatus     = new PlayerStatusData
                {
                    health       = playerStatusComponent.Health,
                    stamina      = playerStatusComponent.Stamina,
                    mana         = playerStatusComponent.Mana,
                    strength     = playerStatusComponent.Strength,
                    agility      = playerStatusComponent.Agility,
                    intelligence = playerStatusComponent.Intelligence,
                    attack       = playerStatusComponent.Attack,
                    defense      = playerStatusComponent.Defense,
                    block        = playerStatusComponent.Block,
                    maxHealth    = playerStatusComponent.MaxHealth,
                    maxMana      = playerStatusComponent.MaxMana,
                    maxStamina   = playerStatusComponent.MaxStamina,
                    speed        = playerStatusComponent.Speed,
                    dexterity    = playerStatusComponent.Dexterity,
                    luck         = playerStatusComponent.Luck
                }
            };

            var sceneMgr = FindObjectOfType<SceneObjectManager>();
            if (sceneMgr != null)
                data.sceneObjectStates = sceneMgr.CaptureState();

            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
            Debug.Log("Game saved to " + savePath);
        }

        public void LoadGame(
            Inventory inventory,
            Equipment equipment,
            QuickAccessBar quickAccessBar,
            List<ChestInventory> chests,                 
            CurrencyManager currencyManager,
            PlayerStatus playerStatusComponent,
            Transform playerTransform)
        {
            if (!File.Exists(savePath))
            {
                Debug.LogError("Save file not found: " + savePath);
                return;
            }

            string json = File.ReadAllText(savePath);
            var data  = JsonUtility.FromJson<GameSaveData>(json);

            // clear & reload main inventories
            equipment.ClearItems();
            inventory.ClearItems();
            quickAccessBar.ClearItems();

            inventory.PopulateAllItemsList();
            equipment.PopulateAllItemsList();
            quickAccessBar.PopulateAllItemsList();

            inventory.LoadItems(data.inventoryItems);
            equipment.LoadItems(data.equipmentItems);
            quickAccessBar.LoadItems(data.quickAccessItems);
            currencyManager.SetCurrencyData(data.currencyAmounts);

            // restore chests
            foreach (var cs in data.chestItems)
            {
                var chest = chests.Find(c => c.chestName == cs.chestName);
                if (chest != null)
                    chest.LoadItems(cs.items);
                else
                    Debug.LogWarning($"Saved chest '{cs.chestName}' not found in scene.");
            }

            StartCoroutine(ApplyPlayerPositionNextFrame(playerTransform, data.playerPosition));

            playerStatusComponent.Health       = data.playerStatus.health;
            playerStatusComponent.Stamina      = data.playerStatus.stamina;
            playerStatusComponent.Mana         = data.playerStatus.mana;
            playerStatusComponent.Strength     = data.playerStatus.strength;
            playerStatusComponent.Agility      = data.playerStatus.agility;
            playerStatusComponent.Intelligence = data.playerStatus.intelligence;
            playerStatusComponent.Attack       = data.playerStatus.attack;
            playerStatusComponent.Defense      = data.playerStatus.defense;
            playerStatusComponent.Block        = data.playerStatus.block;
            playerStatusComponent.MaxHealth    = data.playerStatus.maxHealth;
            playerStatusComponent.MaxMana      = data.playerStatus.maxMana;
            playerStatusComponent.MaxStamina   = data.playerStatus.maxStamina;
            playerStatusComponent.Speed        = data.playerStatus.speed;
            playerStatusComponent.Dexterity    = data.playerStatus.dexterity;
            playerStatusComponent.Luck         = data.playerStatus.luck;

            StartCoroutine(ApplySceneStateDelayed(data.sceneObjectStates));
        }

        private IEnumerator ApplyPlayerPositionNextFrame(Transform player, Vector3 target)
        {
            yield return null;
            player.position = target;
        }

        private IEnumerator ApplySceneStateDelayed(List<SceneObjectManager.ChildObjectState> states)
        {
            yield return new WaitForEndOfFrame();
            var sceneMgr = FindObjectOfType<SceneObjectManager>();
            if (sceneMgr != null && states != null)
            {
                sceneMgr.ApplyState(states);
                Debug.Log("SceneObjectManager state applied.");
            }
        }
    }
}
