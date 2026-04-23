using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Diagnostics;
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

    private static readonly string radiationDamageSound = "RadiationDamage";
    private static readonly string radiationNoDamageSound = "RadiationButNoDamage";
    private EventReference radiationDamageRef = default;
    private EventReference radiationNoDamageRef = default;
    public float CurrentRadiation
    {
        get => currentRadiation;
        set => currentRadiation = Mathf.Clamp(value, 0, radiationThreshold);
    }

    //these are updated from the radiation zone object
    private bool inRadiation = false;
    private int radiationZone = 0; // indicates what level the radiation zone is; use as the index for the arrays
    public bool InRadiation {
        get => inRadiation;
        set => inRadiation = value;
    }
    public int RadiationZone
    {
        get => radiationZone;
        set => radiationZone = value;
    }

    private bool inRadiationNoDamage = false;
    private bool inRadiationDamage = false;
    private EventInstance radiationSound;

    public int SlimeLevel => SaveManager.Instance.playerModule.playerData.slimeLevel;


    private float currentRadiation = 0f;
    private float radiationDamageTimer;     // tracks time since last radiation tick
    private EntityHealthManager playerHealth;

    IEnumerator Start()
    {
        if (currentRadiation < 0) currentRadiation = 0f;
        playerHealth = GetComponent<EntityHealthManager>();
        while (!FMODEvents.Instance.Initialized)
        {
            yield return null;
        }
        radiationDamageRef = FMODEvents.Instance.GetEventReferenceNoAsync(radiationDamageSound);
        radiationNoDamageRef = FMODEvents.Instance.GetEventReferenceNoAsync(radiationNoDamageSound);
    }

    void Update()
    {
        //when not in a radiation area, decay the radiation to 0
        //when in a radiation area, buildup the radiation
        //  if the radiation gets to the threshold, start taking damage
        UnityEngine.Debug.Log("current slime: " + SlimeLevel);
        UnityEngine.Debug.Log("current radiation: " + currentRadiation);
        if (!inRadiation)
        {
            if (currentRadiation > 0)
            {
                currentRadiation = Mathf.Max(currentRadiation - radiationDecayRate[radiationZone] * (2 - slimePercent[SlimeLevel]) * Time.deltaTime, 0); // decay the radiation, but not below 0
            }
            else
            {
                currentRadiation = 0f; // radiation shouldn't be below 0
            }
            radiationDamageTimer = 0f; // reset timer if not taking rad damage
            inRadiationDamage = false;
            inRadiationNoDamage = false;
            if (radiationSound.isValid()) {
                UnityEngine.Debug.Log("Stop playing radiation sound");
                radiationSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                radiationSound.release();
                radiationSound = default;
            }

        } 
        else // inside radiation area
        {
            //*doesn't take damage on the frame you reach the threshold
            if (currentRadiation < radiationThreshold) // radiation isn't at the threshold
            {
                // buildup
                currentRadiation = Mathf.Min(currentRadiation + radiationBuildRate[radiationZone] * slimePercent[SlimeLevel] * Time.deltaTime, radiationThreshold); // don't go over threshold
                inRadiationDamage = false;
                if (!inRadiationNoDamage) {    
                    if (!radiationNoDamageRef.IsNull)
                    {
                        inRadiationNoDamage = true;
                        UnityEngine.Debug.Log("Start playing radiation no damage sound " + radiationNoDamageRef);
                        if (radiationSound.isValid())
                        {
                            radiationSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                            radiationSound.release();
                        }
                        radiationSound = AudioManager.Instance.CreateEventInstance(radiationNoDamageRef);
                        RuntimeManager.AttachInstanceToGameObject(
                            radiationSound,
                            PlayerID.Instance.transform,
                            PlayerID.Instance.GetComponent<Rigidbody>()
                        );
                        radiationSound.start();
                    }
                    else
                    {
                        UnityEngine.Debug.Log("radiationNoDamageRef not found");
                    }
                    
                }
            }
            else // radiation is at the threshold -> take rad damage
            { 
                radiationDamageTimer += Time.deltaTime;

                if (radiationDamageTimer >= radiationDamageInterval[radiationZone])
                {
                    radiationDamageTimer = 0f; // Reset timer
                    playerHealth.TakeDamage(radiationDamageContext);
                    UnityEngine.Debug.Log("Radiation - Took damage");
                }
                inRadiationNoDamage = false;
                if (!inRadiationDamage) {
                    if (!radiationDamageRef.IsNull) {
                        inRadiationDamage = true;
                        UnityEngine.Debug.Log("Start playing radiation damage sound " + radiationDamageRef);
                        if (radiationSound.isValid())
                        {
                            radiationSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                            radiationSound.release();
                        }
                        radiationSound = AudioManager.Instance.CreateEventInstance(radiationDamageRef);
                        RuntimeManager.AttachInstanceToGameObject(
                            radiationSound,
                            PlayerID.Instance.transform,
                            PlayerID.Instance.GetComponent<Rigidbody>()
                        );
                        radiationSound.start();
                    }
                    else
                    {
                        UnityEngine.Debug.Log("radiationDamageRef not found");
                    }
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