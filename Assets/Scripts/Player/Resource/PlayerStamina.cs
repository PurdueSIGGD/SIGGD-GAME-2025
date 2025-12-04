using System.Collections;
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

    private Animator anim;

    private IEnumerator coroutine;

    void Start()
    {
        currentStamina = maxStamina;
        //playerStamina = GetComponent<EntityHealthManager>(); //entitystaminamanager?
        psm = PlayerID.Instance.stateMachine;
        anim = PlayerID.Instance.GetComponent<Animator>();
    }

    void Update()
    {
        isSprinting = psm.IsSprinting;
        isClimbing = psm.IsClimbing;

        // stamina decays while exerting effort (climb, sprint; jump triggers once?)
        Debug.Log("Stamina: " + currentStamina);
        //if (currentStamina <= 0) // if player runs out of stamina, stop the action they're doing
        //{
        //    if (isSprinting)
        //    {
        //        Debug.Log("Ran out of stamina, stopped sprinting");
        //    }
        //    if (isClimbing)
        //    {
        //        Debug.Log("Ran out of stamina, stopped climbing");
        //    }
        //    // jumping should trigger once, don't need to keep checking
        //}
        if (isSprinting && currentStamina <= 0)
        {
            Debug.Log("Ran out of stamina, stopped sprinting");
            if (coroutine == null)
            {
                coroutine = DisableStamina();
                StartCoroutine(coroutine);
            }
            
        } 
        else if (isClimbing && currentStamina <= 0)
        {
            Debug.Log("Ran out of stamina, stopped climbing");
        }
        if (isClimbing || isSprinting)
        {
            currentStamina -= staminaDecayRate * Time.deltaTime;
        }
        else if (currentStamina < maxStamina) // stamina regens while not exerting effort, but can't go over max
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(MaxStamina, CurrentStamina);
            Debug.Log("Stamina rate: " + staminaRegenRate);
            //currentStamina += staminaRegenRate * (currentHunger / maxHunger) * Time.deltaTime; // regen stamina slower as hunger goes down
            //Debug.Log("Stamina rate: " + staminaRegenRate * (currentHunger / maxHunger));
        }
    }

    public void UpdateStamina(float amount)
    {
        currentStamina += amount;
    }

    private IEnumerator DisableStamina()
    {
        anim.SetBool("hasStamina", false);
        yield return new WaitForSeconds(5);
        anim.SetBool("hasStamina", true);
        coroutine = null;
    }
}



