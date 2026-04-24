using UnityEngine;

public class PlayerRadiation : MonoBehaviour
{
    [SerializeField] float radiationThreshold = 100f; // once threshold is reached, player starts taking damage
    // these arrays are for the 5 levels of radiation (0 is first/safest level, 4 is most dangerous)
    [SerializeField] float[] radiationDecayRate = { 9f, 8f, 7f, 6f, 5f}; 
    [SerializeField] float[] radiationBuildRate = { 5f, 7f, 9f, 11f, 13f };
    [SerializeField] float[] radiationDamageInterval = { 5f, 4.5f, 4f, 3.5f, 3f }; // seconds between radiation damage
    [SerializeField] DamageContext radiationDamageContext;
    [SerializeField] GenericLingeringVignette radiationVignette;
    [SerializeField] float[] slimePercent = { .9f, .7f, .5f, .3f, .1f}; // multiplies the builduprate so it's slower
    public float RadiationThreshold => radiationThreshold;

    public float CurrentRadiation
    {
        get => currentRadiation;
        set => currentRadiation = Mathf.Clamp(value, 0, radiationThreshold);
    }

    //these are updated from the radiation zone object
    private bool inRadiation = false;
    private int radiationZoneLevel = 0; // indicates what level the radiation zone is; use as the index for the arrays
    public bool InRadiation {
        get => inRadiation;
        set => inRadiation = value;
    }
    public int RadiationZoneLevel
    {
        get => radiationZoneLevel;
        set => radiationZoneLevel = value;
    }

    public int SlimeLevel => SaveManager.Instance.playerModule.playerData.slimeLevel;


    private float currentRadiation = 0f;
    private float radiationDamageTimer;     // tracks time since last radiation tick
    private EntityHealthManager playerHealth;

    void Start()
    {
        if (currentRadiation < 0) currentRadiation = 0f;
        playerHealth = GetComponent<EntityHealthManager>();

    }

    void Update()
    {
        //when not in a radiation area, decay the radiation to 0
        //when in a radiation area, buildup the radiation
        //  if the radiation gets to the threshold, start taking damage
        //Debug.Log("current slime: " + SlimeLevel);
        //Debug.Log("current radiation: " + currentRadiation);
        if (!inRadiation)
        {
            if (currentRadiation > 0)
            {
                currentRadiation = Mathf.Max(currentRadiation - radiationDecayRate[radiationZoneLevel] * (2 - slimePercent[SlimeLevel]) * Time.deltaTime, 0); // decay the radiation, but not below 0
            }
            else
            {
                currentRadiation = 0f; // radiation shouldn't be below 0
            }
            radiationDamageTimer = 0f; // reset timer if not taking rad damage
        } 
        else // inside radiation area
        {
            //*doesn't take damage on the frame you reach the threshold
            if (currentRadiation < radiationThreshold) // radiation isn't at the threshold
            {
                // buildup
                currentRadiation = Mathf.Min(currentRadiation + radiationBuildRate[radiationZoneLevel] * slimePercent[SlimeLevel] * Time.deltaTime, radiationThreshold); // don't go over threshold
            }
            else // radiation is at the threshold -> take rad damage
            { 
                radiationDamageTimer += Time.deltaTime;

                if (radiationDamageTimer >= radiationDamageInterval[radiationZoneLevel])
                {
                    radiationDamageTimer = 0f; // Reset timer
                    playerHealth.TakeDamage(radiationDamageContext);
                    //Debug.Log("Radiation - Took damage");
                }
            }
            float radiationPercent = GetRadiationPercent();
            float targetStrength = (1 - radiationPercent) * 1.5f;
            radiationVignette?.SetStrength(targetStrength);

        }

        // Update texture opacity
        if (RadioactiveVFXManager.Instance != null)
        {
            float radiationPercentage = GetRadiationPercent();
            if (radiationPercentage > 0f)
            {
                RadioactiveVFXManager.Instance.UpdateOpacity(radiationPercentage);
            }
            else
            {
                // Set a timer to deactivate the radiation VFX container object after the player has left for some time
                RadioactiveVFXManager.Instance.StopAfterDelay();
            }
        }
        
    }

    public float GetRadiationPercent()
    {
        return CurrentRadiation / RadiationThreshold;
    }

    public void UpdateRadiation(float amount)
    {
        currentRadiation += amount;
    }

    public void ResetRadiation()
    {
        currentRadiation = 0f;

    }

    public void IncrementSlimeLevel()
    {
        if (SaveManager.Instance.playerModule.playerData.slimeLevel < 4)
        {
            SaveManager.Instance.playerModule.playerData.slimeLevel++;
        }
    }
}