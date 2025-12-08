using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDecayRate = 2f;
    [SerializeField] float staminaRegenRate = 1f;

    [SerializeField] Slider staminaSlider;
    
    private float currentStamina;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    private PlayerStateMachine psm;

    private bool isSprinting;
    private bool isClimbing;

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
        staminaSlider.value = currentStamina / maxStamina;

        isSprinting = psm.IsSprinting;
        isClimbing = psm.IsClimbing;

        // stamina decays while exerting effort (climb, sprint; jump triggers once?)

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
        else if (currentStamina < maxStamina) // stamina regens while not exerting effort, but can't go over max
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
}



