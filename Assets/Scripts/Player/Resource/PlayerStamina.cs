using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDecayRate = 2f;
    [SerializeField] float staminaRegenRate = 1f;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    public float currentHunger => GetComponent<PlayerHunger>().CurrentHunger;
    public float maxHunger => GetComponent<PlayerHunger>().MaxHunger;

    private float currentStamina;

    private PlayerStateMachine psm;

    private bool isSprinting;


    void Start()
    {
        currentStamina = maxStamina;
        //playerStamina = GetComponent<EntityHealthManager>(); //entitystaminamanager?
        psm = PlayerID.Instance.stateMachine;
    }

    void Update()
    {
        isSprinting = psm.IsSprinting;
        // TODO make different decay rates for each type of activity
        // TODO make a way to tell if the player is climbing/sprinting/etc

        // stamina decays while exerting effort (climb, sprint; jump triggers once?)

        Debug.Log("Stamina: " + currentStamina);
        if (currentStamina <= 0) // if the player runs out of stamina
        {
            /*if (climbing == true)
            {
                // fall off wall
            }
            else */
            if (isSprinting)
            {
                Debug.Log("Ran out of stamina, stopped sprinting");
            }
            // jumping should trigger once, don't need to keep checking
        }
        /*else if (climbing == true)
        {
            currentStamina -= staminaDecayRate * Time.deltaTime;
        }
        */
        else if (isSprinting == true)
        {
            currentStamina -= staminaDecayRate * Time.deltaTime;
        }
        // stamina regens while not exerting effort
        else if (currentStamina < maxStamina)// if the player isn't doing effort (has stamina & isn't climbing or sprinting)
        {
            currentStamina += staminaRegenRate * (currentHunger / maxHunger) * Time.deltaTime;
            Debug.Log("Stamina rate: " + staminaRegenRate * (currentHunger / maxHunger));
        }
    }


    public void UpdateStamina(float ammount)
    {
        currentStamina += ammount;
    }
}



