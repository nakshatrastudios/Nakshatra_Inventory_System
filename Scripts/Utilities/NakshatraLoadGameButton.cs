using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class NakshatraLoadGameButton : MonoBehaviour
    {
        [Header("UI Reference")]
        public Button loadButton;

        [Header("Save File Settings")]
        public string saveFileName = "game_save.json";

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

        private const string LoadFlagKey = "Nakshatra_LoadFromSave";

        private void Start()
        {
            if (loadButton == null)
            {
                Debug.LogError("NakshatraLoadGameButton: Load Button not assigned.");
                return;
            }

            loadButton.interactable = File.Exists(SaveFilePath);
        }

        public void LoadSavedGame()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("NakshatraLoadGameButton: No save file found.");
                return;
            }

            string json = File.ReadAllText(SaveFilePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            if (!string.IsNullOrEmpty(saveData.currentScene))
            {
                PlayerPrefs.SetInt(LoadFlagKey, 1);
                SceneManager.LoadScene(saveData.currentScene);
            }
            else
            {
                Debug.LogWarning("NakshatraLoadGameButton: Scene name not found in save file.");
            }
        }

        public static bool ShouldLoadFromSave()
        {
            return PlayerPrefs.GetInt(LoadFlagKey, 0) == 1;
        }

        public static void ClearLoadFlag()
        {
            PlayerPrefs.DeleteKey(LoadFlagKey);
        }
    }
}
