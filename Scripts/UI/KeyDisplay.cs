using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    [AddComponentMenu("Tools/Key Display")]
    public class KeyDisplay : MonoBehaviour
    {
        [Header("Key Selection")]
        [Tooltip("Select the key whose name you want to display")]
        public KeyCode selectedKey = KeyCode.Space;

        [Header("UI Reference")]
        [Tooltip("Drag your UI Text component here")]
        public Text targetText;

        [Header("Display Format")]
        [Tooltip("Use '{0}' as a placeholder for the key name.\nE.g. 'Press {0} to Pickup' or 'Use {0} to Jump'")]
        [TextArea]
        public string displayFormat = "{0}";

        void Reset()
        {
            // Try to auto-find a Text on this GameObject
            targetText = GetComponent<Text>();
        }

        void Update()
        {
            if (targetText == null)
            {
                Debug.LogWarning("KeyDisplay: targetText is not assigned.", this);
                return;
            }

            // Always show the formatted string with the selected key
            targetText.text = string.Format(displayFormat, selectedKey.ToString());

            // If you only want to update on press, uncomment:
            // if (Input.GetKeyDown(selectedKey))
            //     targetText.text = string.Format(displayFormat, selectedKey.ToString());
        }
    }
}