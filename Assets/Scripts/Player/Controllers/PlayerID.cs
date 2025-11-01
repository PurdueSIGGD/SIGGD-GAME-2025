using UnityEngine;

/**
 * <summary>
 * A singleton reference to the player that holds important components and references.
 * </summary>
 */
public class PlayerID : Singleton<PlayerID>
{
    #region GameObject Components 
    
    [Header("Components")]
    [HideInInspector] public FirstPersonCamera cam ; // Reference to the main camera in the scene. Can be serialized, but kept it hide for now cus seeing prefab changes in scene is kind of annoying
    [HideInInspector] public Rigidbody rb; // Reference to the Rigidbody component on the same GameObject.
    [HideInInspector] public PlayerStateMachine stateMachine; // Reference to the player's state machine on the same GameObject.
    [HideInInspector] public CameraMovement cameraMovement;
    [HideInInspector] public PlayerMovement playerMovement; // Reference to player movement script
    [HideInInspector] public EntityHealthManager playerHealth;
    [HideInInspector] public PlayerHunger playerHunger;
    public Inventory Inventory => Inventory.Instance;
    [HideInInspector] public PlayerInteractor playerInteractor;
    
    
    #endregion
    
    #region MonoBehaviour Callbacks

    protected override void Awake()
    {
        base.Awake();
        stateMachine = GetComponent<PlayerStateMachine>();
        playerInteractor = GetComponent<PlayerInteractor>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<EntityHealthManager>();
        playerHunger = GetComponent<PlayerHunger>();

        rb = GetComponent<Rigidbody>();

        if (cam == null)
        {
            cam = Camera.main.GetComponent<FirstPersonCamera>();
            if (!cam) Debug.LogError("Multiple main camera in scene or missing FirstPersonCamera script");
        }
        cameraMovement = cam.GetComponentInParent<CameraMovement>();
    }

    #endregion
}
