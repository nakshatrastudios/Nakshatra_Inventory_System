using UnityEngine;
using UnityEngine.UI;

namespace Nakshatra.Plugins
{
    public class HUD : MonoBehaviour
    {
        public Image healthBarFill;
        public Image manaBarFill;
        public Image staminaBarFill;

        private PlayerStatus playerStatus;

        void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerStatus = player.GetComponent<PlayerStatus>();
                if (playerStatus == null)
                {
                    Debug.LogError("PlayerStatus component not found on player.");
                }
            }
            else
            {
                Debug.LogError("Player GameObject with tag 'Player' not found.");
            }

            UpdateHealthBar();
            UpdateManaBar();
            UpdateStaminaBar();
        }

        void Update()
        {
            UpdateHealthBar();
            UpdateManaBar();
            UpdateStaminaBar();
        }

        public void UpdateHealthBar()
        {
            if (playerStatus != null && healthBarFill != null)
            {
                healthBarFill.fillAmount = (float)playerStatus.Health / playerStatus.MaxHealth;
            }
        }

        public void UpdateManaBar()
        {
            if (playerStatus != null && manaBarFill != null)
            {
                manaBarFill.fillAmount = (float)playerStatus.Mana / playerStatus.MaxMana;
            }
        }

        public void UpdateStaminaBar()
        {
            if (playerStatus != null && staminaBarFill != null)
            {
                staminaBarFill.fillAmount = (float)playerStatus.Stamina / playerStatus.MaxStamina;
            }
        }
    }

}