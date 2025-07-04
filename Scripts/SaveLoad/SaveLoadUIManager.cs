using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class SaveLoadUIManager : MonoBehaviour
    {
        [Header("References")]
        public SaveLoadManager        saveLoadManager;
        public Inventory              inventory;
        public Equipment              equipment;
        public QuickAccessBar         quickAccessBar;
        public List<ChestInventory>   chests;           
        public CurrencyManager        currencyManager;
        public PlayerStatus           playerStatus;
        public Transform              playerTransform;

        [Header("UI Buttons")]
        public Button saveButton;
        public Button loadButton;

        private void Start()
        {
            if (saveButton != null)
                saveButton.onClick.AddListener(SaveGame);
            if (loadButton != null)
                loadButton.onClick.AddListener(LoadGame);
        }

        public void SaveGame()
        {
            if (saveLoadManager == null ||
                inventory       == null ||
                equipment       == null ||
                quickAccessBar  == null ||
                chests          == null ||
                currencyManager == null)
            {
                Debug.LogError("SaveLoadUIManager: One or more references not set in Inspector.");
                return;
            }

            saveLoadManager.SaveGame(
                inventory,
                equipment,
                quickAccessBar,
                chests,            
                currencyManager,
                playerStatus,
                playerTransform
            );
        }

        public void LoadGame()
        {
            if (saveLoadManager == null ||
                inventory       == null ||
                equipment       == null ||
                quickAccessBar  == null ||
                chests          == null ||
                currencyManager == null)
            {
                Debug.LogError("SaveLoadUIManager: One or more references not set in Inspector.");
                return;
            }

            saveLoadManager.LoadGame(
                inventory,
                equipment,
                quickAccessBar,
                chests,            
                currencyManager,
                playerStatus,
                playerTransform
            );
        }
    }
}
