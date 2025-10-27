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
    [HideInInspector] public Camera cam ; // Reference to the main camera in the scene. Can be serialized, but kept it hide for now cus seeing prefab changes in scene is kind of annoying
    [HideInInspector] public Rigidbody rb; // Reference to the Rigidbody component on the same GameObject.
    [HideInInspector] public PlayerStateMachine stateMachine; // Reference to the player's state machine on the same GameObject.
    [HideInInspector] public PlayerMovement playerMovement; // Reference to the PlayerMovement component on the same GameObject.
    
    #endregion
    
    #region MonoBehaviour Callbacks

    protected override void Awake()
    {
        base.Awake();
        stateMachine = GetComponent<PlayerStateMachine>();
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    #endregion
}
