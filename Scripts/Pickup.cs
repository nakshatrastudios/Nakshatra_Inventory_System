using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Item item; // Ensure this is set in the Unity Editor or through code
    private bool inRange = false;
    private GameObject player = null;
    private GameObject uiPromptInstance = null;

    void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupItem();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inRange = true;
            player = other.gameObject;

            if (item != null && item.pickupPromptPrefab != null)
            {
                uiPromptInstance = Instantiate(item.pickupPromptPrefab, Vector3.zero, Quaternion.identity);
                uiPromptInstance.SetActive(true);
            }
            else
            {
                Debug.LogError("Item or item.pickupPromptPrefab is not set!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inRange = false;
            player = null;

            if (uiPromptInstance != null)
            {
                Destroy(uiPromptInstance);
                uiPromptInstance = null;
            }
        }
    }

    void PickupItem()
    {
        if (player != null)
        {
            InventoryComponent inventory = player.GetComponent<InventoryComponent>();

            if (inventory != null)
            {
                inventory.AddItem(item);
                Destroy(gameObject);

                if (uiPromptInstance != null)
                {
                    Destroy(uiPromptInstance);
                }
            }
            else
            {
                Debug.LogError("Player does not have an InventoryComponent!");
            }
        }
        else
        {
            Debug.LogError("Player reference is null!");
        }
    }
}
