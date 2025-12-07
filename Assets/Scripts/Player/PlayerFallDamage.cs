using UnityEngine;

public class PlayerFallDamage : MonoBehaviour
{
    private GameObject plrObject;
    private PlayerStateMachine stateMachine;
    private Rigidbody rb;
    private EntityHealthManager playerHealth;

    private float lastVerticalVelocity = 0f; // last recorded vertical velocity
    private float timeFalling = 0f; // last recorded time spent falling
    private bool wasFalling = false;

    [Header("Configurations")]
    [Tooltip("If true, use attributes from 'Precentages' when calculating fall damage instead of 'Exact Values'. Exact values will be overridden")]
    [SerializeField] private bool config_usePrecentageDamage = false;

    [Tooltip("If true, maxHurtVelocity takes the place of killVelocity and killVelocity is unused.")]
    [SerializeField] private bool config_noKillVelocity = false;

    [Tooltip("If true, fallDamageCurve is used in falldaamge calculations. If false, a linear curve is used")]
    [SerializeField] private bool config_useFallDamageCurve = true;

    [Tooltip("If true, round _UP_ to whole numbers for fall damage. If false, ugly decimals are used. For testing, it is recommended to keep this on.")]
    [SerializeField] private bool config_roundDamage = true;

    [Tooltip("If true, will print the fall speed when landing and the damage dealt from falling")]
    [SerializeField] private bool config_debugMode = false;


    [Header("Falling Attributes")]
    [Tooltip("If the player has spent no longer than the given threshold in the air, they will recieve no fall damage")]
    [SerializeField] private float timeFallingThreshold = 0.25f;

    [Tooltip("Minimum vertical speed the player needs to take minimum fall damage")]
    [SerializeField] private float minHurtVelocity = 12f;

    [Tooltip("Maximum vertical speed the player needs to take maximum fall damage")]
    [SerializeField] private float maxHurtVelocity = 17f;

    [Tooltip("Vertical speed the player needs to immediately die.")]
    [SerializeField] private float killVelocity = 17.9f;

    [Tooltip("Starts from 0 and ends at 1 describing how fall damage is registered. Lower = less damage, higher = more damage. Damage is based on attributes below.")]
    [SerializeField] private AnimationCurve fallDamageCurve;

    [Header("Fall Damage - Exact Values")]
    [SerializeField] private float minFallDamage = 1f;
    [SerializeField] private float maxFallDamage = 10f;


    [Header("Fall Damage - Precentages")]
    [SerializeField] [Range(0f, 1f)] private float minFallDamage_Precent = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float maxFallDamage_Precent = 0.8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plrObject = this.gameObject;
        PlayerID playerInstance = PlayerID.Instance;

        stateMachine = playerInstance.stateMachine;
        rb = playerInstance.rb;
        playerHealth = plrObject.GetComponent<EntityHealthManager>();
    }


    // Update is called once per frame
    void Update() {
        if (stateMachine.IsGrounded == true)
        {
            // player is grounded
            // were they falling to their death?
            if (wasFalling == true) {
                // they were? oh goodie goodie wow, they need to register this fall immediately
                wasFalling = false;

                if (config_usePrecentageDamage) {
                    minFallDamage = playerHealth.MaxHealth * minFallDamage_Precent;
                    maxFallDamage = playerHealth.MaxHealth * maxFallDamage_Precent;
                }

                // calculate damage recieved from fall
                float damageRecieved = 0f;

                // have they been falling for a long enough time?...
                if (timeFalling > timeFallingThreshold) {
                    // calculate exact fall damage
                    if (lastVerticalVelocity >= killVelocity || config_noKillVelocity && lastVerticalVelocity >= maxHurtVelocity) {
                        if (config_debugMode == true) {
                            print("DEBUG: Fall speed exceeds kill velocity: fallspeed = " + lastVerticalVelocity + ". kill velocity: " + killVelocity);
                        }

                        damageRecieved = -1f; // later in the script, this is equal to dealing player's max health as damage (death)
                    } else if (lastVerticalVelocity >= minHurtVelocity) {
                        if (config_debugMode) {
                            print("DEBUG: in damage zone for falling speed: " + lastVerticalVelocity);
                        }

                        // gets "precentage" of vertical fall speed according to hurt velocity variables.
                        float fallspeedPrecentage = Mathf.Min((lastVerticalVelocity - minHurtVelocity) / (maxHurtVelocity - minHurtVelocity), 1f);

                        if (config_useFallDamageCurve) {
                            fallspeedPrecentage = fallDamageCurve.Evaluate(fallspeedPrecentage);
                        }

                        damageRecieved = (maxFallDamage - minFallDamage) * fallspeedPrecentage + minFallDamage;
                    }
                } else if (config_debugMode) {
                    print("DEBUG: haven't been falling long enough to check for fall damage");
                }

                // if damage if negative, 
                if (damageRecieved < 0f)
                {
                    damageRecieved = playerHealth.MaxHealth;
                }

                if (damageRecieved > 0f) {
                    if (config_roundDamage) { 
                        damageRecieved = Mathf.Ceil(damageRecieved);
                    }

                    // creates whatever a 'damage context' is to deal fall damage
                    DamageContext newDamage = new DamageContext();
                    newDamage.attacker = plrObject;
                    newDamage.victim = plrObject;
                    newDamage.amount = damageRecieved;
                    newDamage.xxtraContext = "Fall Damage";

                    if (config_debugMode) {
                        print("DEBUG: Dealt fall damage: " + damageRecieved);
                    }

                    // deals the fall damage
                    playerHealth.TakeDamage(newDamage);
                }

                // TODO: put whatever sound effects are triggered from falling here.
            }
        } else
        {
            // player is in the air
            // are they falling to their death?
            if (wasFalling == false && rb.linearVelocity.y < 0)
            {
                wasFalling = true;
                timeFalling = 0;
                lastVerticalVelocity = 0;
            }
            timeFalling += Time.deltaTime;
            lastVerticalVelocity = Mathf.Abs(Mathf.Min(rb.linearVelocity.y, 0));
        }
    }
}
