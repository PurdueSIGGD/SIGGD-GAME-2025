using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDecayRate = 2f;
    [SerializeField] float staminaRegenRate = 1f;
    
    private float currentStamina;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    public float currentHunger => GetComponent<PlayerHunger>().CurrentHunger;
    public float maxHunger => GetComponent<PlayerHunger>().MaxHunger;

    private PlayerStateMachine psm;

    private bool isSprinting;
    private bool isClimbing;


    void Start()
    {
        currentStamina = maxStamina;
        //playerStamina = GetComponent<EntityHealthManager>(); //entitystaminamanager?
        psm = PlayerID.Instance.stateMachine;
    }

    void Update()
    {
        isSprinting = psm.IsSprinting;
        isClimbing = psm.IsClimbing;

        // stamina decays while exerting effort (climb, sprint; jump triggers once?)
        Debug.Log("Stamina: " + currentStamina);
        if (currentStamina <= 0) // if player runs out of stamina, stop the action they're doing
        {
            if (isSprinting)
            {
                Debug.Log("Ran out of stamina, stopped sprinting");
            }
            if (isClimbing)
            {
                Debug.Log("Ran out of stamina, stopped climbing");
            }
            // jumping should trigger once, don't need to keep checking
        }
        else if (isClimbing || isSprinting)
        {
            currentStamina -= staminaDecayRate * Time.deltaTime;
        }
        
        else if (currentStamina < maxStamina) // stamina regens while not exerting effort, but can't go over max
        {
            currentStamina += staminaRegenRate * (currentHunger / maxHunger) * Time.deltaTime; // regen stamina slower as hunger goes down
            Debug.Log("Stamina rate: " + staminaRegenRate * (currentHunger / maxHunger));
        }
    }

    public void UpdateStamina(float amount)
    {
        currentStamina += amount;
    }
}



