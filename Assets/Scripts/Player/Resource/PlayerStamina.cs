//using UnityEngine;
//using static UnityEditor.PlayerSettings;

//public class PlayerStamina : MonoBehaviour
//{
//    [SerializeField] float maxStamina = 100f;
//    [SerializeField] float staminaDecayRate = 1f;

//    private PlayerID playerID;

//    public float MaxStamina => maxStamina;
//    public float CurrentStamina => currentStamina;
    
//    private float currentStamina;

//    private PlayerStateMachine psm;

//    private bool isSprinting;


//    void Start()
//    {
//        currentStamina = maxStamina;
//        //playerStamina = GetComponent<EntityHealthManager>(); //entitystaminamanager?
//        psm = playerID.stateMachine;
//    }

//    void Update()
//    {
//        isSprinting = PlayerInput.Instance.sprintInput;
//        // TODO make different decay rates for each type of activity
//        // TODO make a way to tell if the player is climbing/sprinting/etc

//        // stamina decays while exerting effort (climb, sprint; jump triggers once?)

//        if (currentStamina <= 0) // if the player runs out of stamina
//        {
//            if (climbing == true)
//            {
//                // fall off wall
//            }
//            else if (isSprinting)
//            {
//                // stop sprinting
//            }
//            // jumping should trigger once, don't need to keep checking
//        }
//        else if (climbing == true)
//        {
//            currentStamina -= staminaDecayRate * Time.deltaTime;
//        }
//        else if (sprinting == true)
//        {
//            currentStamina -= staminaDecayRate * Time.deltaTime;
//        }
//        // stamina regens while not exerting effort
//        else // if the player isn't doing effort (has stamina & isn't climbing or sprinting)
//        {
//            currentStamina += staminaRegenRate * Time.deltaTime;
//        }

//    }

//    public void UpdateHunger(float ammount)
//    {
//        currentHunger += ammount;
//    }
//}



