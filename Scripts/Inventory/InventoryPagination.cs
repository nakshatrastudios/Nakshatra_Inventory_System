using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class InventoryPagination : MonoBehaviour
    {
        public Inventory inventory;
        public Button previousPageButton;
        public Button nextPageButton;

        private int currentPage = 0;

        void Start()
        {
            UpdateButtons();
        }

        public void NextPage()
        {
            if (currentPage < inventory.Pages - 1)
            {
                currentPage++;
                inventory.SetPage(currentPage);
                UpdateButtons();
            }
        }

        public void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                inventory.SetPage(currentPage);
                UpdateButtons();
            }
        }

        private void UpdateButtons()
        {
            previousPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < inventory.Pages - 1;
        }
    }
}