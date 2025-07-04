using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nakshatra.Plugins
{
    public class NakshatraSceneLoader : MonoBehaviour
    {
        [Header("Scene Settings")]
        [Tooltip("Name of the scene to load when New Game is clicked.")]
        public string sceneToLoad;

        public void LoadScene()
        {
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("NakshatraSceneLoader: No scene name specified.");
                return;
            }

            NakshatraLoadGameButton.ClearLoadFlag(); // 🧹 prevent loading save
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
