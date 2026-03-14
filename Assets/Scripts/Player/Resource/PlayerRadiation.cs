using UnityEngine;

public class PlayerRadiation : MonoBehaviour
{
    //TODO change builduprate, decayrate, and damageinterval to arrays with values for the 5 levels
    [SerializeField] float radiationThreshold = 100f;
    [SerializeField] float radiationDecayRate = 1f;
    [SerializeField] float radiationBuildRate = 1f;
    [SerializeField] float radiationDamageInterval = 10f; // seconds between radiation damage
    [SerializeField] DamageContext radiationDamageContext;
    [SerializeField] GenericLingeringVignette radiationVignette;
    public float RadiationThreshold => radiationThreshold;
    public float CurrentRadiation
    {
        get => currentRadiation;
        set => currentRadiation = Mathf.Clamp(value, 0, radiationThreshold);
    }
    private bool inRadiation; //TODO update this from the radiation area object?

    private float currentRadiation = 0;
    private float radiationDamageTimer;     // tracks time since last radiation tick
    private EntityHealthManager playerHealth;

    void Start()
    {
        if (currentRadiation < 0) currentRadiation = 0;
        playerHealth = GetComponent<EntityHealthManager>();
    }

    void Update()
    {
        //when not in a radiation area, decay the radiation to 0
        //when in a radiation area, buildup the radiation
        //  if the radiation gets to the threshold, start taking damage

        if (!inRadiation)
        {
            if (currentRadiation > 0)
            {
                currentRadiation = Mathf.Max(currentRadiation - radiationDecayRate * Time.deltaTime, 0); // decay the radiation, but don't go below 0
            }
            else
            {
                currentRadiation = 0; // radiation shouldn't be below 0
            }
            radiationDamageTimer = 0f; // reset timer if not taking rad damage - possible case where going in and 
        } 
        else // inside radiation area
        {
            //*doesn't take damage on the frame you reach the threshold
            if (currentRadiation < radiationThreshold) // radiation isn't at the threshold
            {
                // buildup
                currentRadiation = Mathf.Min(currentRadiation + radiationBuildRate * Time.deltaTime, radiationThreshold); // don't go above threshold
            }
            else // radiation is at the threshold -> take rad damage
            { 
                radiationDamageTimer += Time.deltaTime;

                if (radiationDamageTimer >= radiationDamageInterval)
                {
                    radiationDamageTimer = 0f; // Reset timer
                    playerHealth.TakeDamage(radiationDamageContext);
                    Debug.Log("Radiation - Took 1 damage");
                }
            }
            float radiationPercent = CurrentRadiation / RadiationThreshold;
            float targetStrength = (1 - radiationPercent) * 1.5f;
            radiationVignette?.SetStrength(targetStrength);
        }
    }

    public void UpdateRadiation(float amount)
    {
        currentRadiation += amount;
    }

    public void ResetRadiation()
    {
        currentRadiation = 0;
    }
}