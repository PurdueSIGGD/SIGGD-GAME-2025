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
    
    [SerializeField] Slider staminaSlider;
    [SerializeField] Image staminaSliderBar;
    
    private float currentStamina;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    public bool HasStamina => (currentStamina > 0 && anim.GetBool("hasStamina"));

    private PlayerStateMachine psm;

    private bool isSprinting;
    private bool isClimbing;
    private bool isGrounded;

    private Animator anim;

    private IEnumerator coroutine;

    void Start()
    {
        currentStamina = maxStamina;
        psm = PlayerID.Instance.stateMachine;
        anim = PlayerID.Instance.GetComponent<Animator>();
    }

    void Update()
    {
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
            if (coroutine == null)
            {
                coroutine = DisableStamina();
                StartCoroutine(coroutine);
            }
        }
        else if (isClimbing || isSprinting)
        {
            currentStamina -= staminaDecayRate * Time.deltaTime;
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
        anim.SetBool("hasStamina", false);
        yield return new WaitForSeconds(5);
        anim.SetBool("hasStamina", true);
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
}



