using SIGGD.Goap;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDecayRate = 2f;
    [SerializeField] float staminaRegenRate = 1f;
    [SerializeField] float jumpCost = 15f;  // when changing jumpcost, remember to update the number in the animator as well
                                            // (prevents jumping below a certain amount of stamina)
    [SerializeField] float climbingGlovesReduction = 0.5f;
    
    [SerializeField] Slider staminaSlider;
    [SerializeField] Image staminaSliderBar;

    private float currentStamina = -1f;
    private bool staminaDisabled = false;

    public float MaxStamina => maxStamina;
    public float CurrentStamina
    {
        get => currentStamina;
        set => currentStamina = Mathf.Clamp(value, 0, maxStamina);
    }

    public bool StaminaDisabled
    {
        get => staminaDisabled;
        set => staminaDisabled = value;
    }

    public bool HasStamina => (currentStamina > 0 && anim.GetBool("hasStamina"));

    private PlayerStateMachine psm;

    private bool isSprinting;
    private bool isClimbing;
    private bool isGrounded;

    private Animator anim;

    private IEnumerator coroutine;

    void Start()
    {
        if (currentStamina == -1f) currentStamina = maxStamina;
        
        psm = PlayerID.Instance.stateMachine;
        anim = PlayerID.Instance.GetComponent<Animator>();

        if (staminaDisabled)
        {
            if (coroutine == null)
            {
                coroutine = DisableStamina();
                StartCoroutine(coroutine);
            }
        }
    }

    void Update()
    {
        if(currentStamina > PlayerID.Instance.playerHunger.CurrentHunger)
        {
            currentStamina = PlayerID.Instance.playerHunger.CurrentHunger;
        }
        anim.SetFloat("stamina", CurrentStamina);

        staminaSlider.value = currentStamina / maxStamina;
        if (coroutine != null)
        {
            staminaSliderBar.color = Color.red;
        }
        else
        {
            staminaSliderBar.color = Color.green;
        }

        isSprinting = psm.IsSprinting;
        isClimbing = psm.IsClimbing;
        isGrounded = psm.IsGrounded;

        // stamina decays while exerting effort (climb, sprint; jump triggers once)
        if (coroutine == null && currentStamina <= 0)
        {
            if (isSprinting) Debug.Log("Ran out of stamina, stopped sprinting");
            else Debug.Log("Ran out of stamina, stopped climbing");
            coroutine = DisableStamina();
            StartCoroutine(coroutine);
        }
        else if ((isClimbing && !SaveManager.Instance.playerModule.playerData.hasGloves) || isSprinting)
        {
            currentStamina -= staminaDecayRate * Time.deltaTime;
        }
        else if (isClimbing && SaveManager.Instance.playerModule.playerData.hasGloves)
        {
            Debug.Log("Climbing stamina decay reduced by gloves");
            currentStamina -= staminaDecayRate * climbingGlovesReduction * Time.deltaTime;
        }
        else if (isGrounded && currentStamina < maxStamina) // stamina regens while on ground & not exerting effort, but can't go over max
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(MaxStamina, CurrentStamina);
        }
    }

    public void UpdateStamina(float amount)
    {
        currentStamina += amount;
    }

    private IEnumerator DisableStamina()
    {
        staminaDisabled = true;
        anim.SetBool("hasStamina", false);
        // Stamina is disabled until 50%
        // Calculate wait time based on how much current stamina is at (for when stamina is recharging when game was stopped)
        Debug.Log("Out of stamina");
        yield return new WaitUntil(() => currentStamina >= MaxStamina / 2);
        Debug.Log("Stamina at half");
        //yield return new WaitForSeconds(5 * (MaxStamina / 2 - currentStamina) / (MaxStamina / 2)); 
        anim.SetBool("hasStamina", true);
        staminaDisabled = false;
        coroutine = null;
    }

    public void StaminaJump()
    {
        UpdateStamina(-jumpCost);
        if (currentStamina < 0)
        {
            currentStamina = 0;
        }
    }
    
    public void ResetStamina()
    {
        currentStamina = MaxStamina;
    }

    public bool HasStaminaForJump() {
        return !StaminaDisabled && currentStamina >= jumpCost;
    }
}



