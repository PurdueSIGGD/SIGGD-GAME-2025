using UnityEngine;
using UnityEngine.UI;

public class UpdatePlayerHealthHungerBar : MonoBehaviour
{
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider hungerSlider;
    EntityHealthManager playerHealth;
    PlayerHunger playerHunger;

    void Awake()
    {
        playerHealth = PlayerID.Instance.playerHealth;
        playerHunger = PlayerID.Instance.playerHunger;
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = (playerHealth.CurrentHealth / playerHealth.MaxHealth);
        hungerSlider.value = (playerHunger.CurrentHunger / playerHunger.MaxHunger);
    }
}
