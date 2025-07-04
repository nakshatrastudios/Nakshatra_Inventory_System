using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    /// <summary>
    /// Controls the item description panel which shows item details when an inventory slot is clicked.
    /// </summary>
    public class ItemDescriptionPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text itemTypeText;
        [SerializeField] private Text descriptionText;
        [Tooltip("Parent for dynamic stat lines (optional)")]
        [SerializeField] private Transform statsContainer;
        [Tooltip("Prefab for a single stat line (optional)")]
        [SerializeField] private GameObject statLinePrefab;

        private void Awake()
        {
            // Hide on start
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the panel and populates UI with the given item's data.
        /// </summary>
        public void Show(InventoryItem item)
        {
            if (item == null)
                return;

            gameObject.SetActive(true);

            // Icon
            if (iconImage != null && item.itemIcon != null)
                iconImage.sprite = item.itemIcon;

            // Name
            if (nameText != null)
                nameText.text = item.itemName;

            // Item type and categories
            if (itemTypeText != null)
            {
                var descriptors = new List<string> { item.itemType.ToString() };

                if (item.itemType == ItemType.Equipment)
                {
                    descriptors.Add(item.equipmentCategory.ToString());

                    if (item.equipmentCategory == EquipmentCategory.Weapon)
                    {
                        descriptors.Add(item.weaponType.ToString());

                        if (item.weaponType == WeaponType.OneHand)
                        {
                            if (item.isMainHand)
                                descriptors.Add("Main Hand");
                            if (item.isOffHand)
                                descriptors.Add("Off Hand");
                        }
                        else if (item.weaponType == WeaponType.TwoHand)
                        {
                            descriptors.Add("Two-Handed");
                        }
                    }
                }

                itemTypeText.text = string.Join(", ", descriptors);
            }

            // Description
            if (descriptionText != null)
                descriptionText.text = item.itemDescription;

            // Optional: Populate stats
            if (statsContainer != null && statLinePrefab != null && item.stats != null)
            {
                // Clear previous
                foreach (Transform child in statsContainer)
                    Destroy(child.gameObject);

                // Create new stat lines
                foreach (var stat in item.stats)
                {
                    var line = Instantiate(statLinePrefab, statsContainer);
                    var texts = line.GetComponentsInChildren<Text>();
                    if (texts.Length >= 2)
                    {
                        texts[0].text = stat.statType.ToString();
                        texts[1].text = stat.value.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Hides the description panel.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}