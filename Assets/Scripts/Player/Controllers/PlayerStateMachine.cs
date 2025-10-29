using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

/**
 * State machine for managing player states, using a StateController.
 */
public class PlayerStateMachine : MonoBehaviour
{
    #region PlayerID Reference
    private PlayerID playerID;
    #endregion

    #region Components
    [Header("Components")]
    public Animator animator; // Reference to the Animator component.
    [HideInInspector] public Collider mainCol; // Reference to the Collider component on the same GameObject.
    [HideInInspector] public Collider[] allCols;

    private PlayerMovement playerMovement;
    private FirstPersonCamera firstPersonCamera;

    #endregion
    
    #region Parameter SOs
    
    [Header("Parameter SOs")]
    public MoveData moveData; // ScriptableObject containing movement parameters.
    public ItemInfo defaultItem; // The default item the player starts with.

    #endregion
    
    #region MonoBehaviour Callbacks
    
    private void Start()
    {
        playerID = PlayerID.Instance; // Running this in Start to ensure PlayerID is initialized first
        
        mainCol = GetComponent<Collider>();
        allCols = GetComponentsInChildren<Collider>();
        playerMovement = GetComponent<PlayerMovement>();
        firstPersonCamera = playerID.cam.GetComponent<FirstPersonCamera>();
    }

    private void Update()
    {
        UpdateAnimatorParams();

        // Debug keys to test enabling/disabling movement
        if (Input.GetKeyDown(KeyCode.L))
        {
            playerMovement.SendMessage("DisableMovement");
            firstPersonCamera.SendMessage("DisableCamRotation");
            Debug.Log("Movement Disabled");
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            playerMovement.SendMessage("EnableMovement");
            firstPersonCamera.SendMessage("EnableCamRotation");
            Debug.Log("Movement Enabled");
        }
    }

    private void FixedUpdate()
    {
        
    }

    private void OnEnable()
    {
        PlayerInput.Instance.OnJump += OnJumpAction;
        PlayerInput.Instance.OnAction += TriggerAction;
    }
    
    private void OnDisable()
    {
        PlayerInput.Instance.OnJump -= OnJumpAction;
        PlayerInput.Instance.OnAction -= TriggerAction;
    }

    #region Input Callbacks

    /**
     * <summary>
     * Handles the jump input action, setting a timer for jump buffering.
     * </summary>
     *
     * <param name="context">The context of the input action.</param>
     */
    private void OnJumpAction(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        
        playerMovement.lastTimeJumpPressed = moveData.jumpInputBufferTime;
    }

    #endregion
    
    // Because the animator is our state machine, we update parameters there to control state transitions.
    #region Animator Methods
    
    /** <summary>
     * Updates the animator parameters based on the player's current state.
     * </summary>
     */
    private void UpdateAnimatorParams()
    {
        animator.SetBool(Animator.StringToHash("isClimbing"), playerMovement.IsClimbing);
        if (playerMovement.IsGrounded)
        {
            playerMovement.lastTimeGrounded = moveData.coyoteTime;
            playerMovement.LastGroundedPosition = playerID.transform.position;
        }
        else
            playerMovement.lastTimeGrounded -= Time.deltaTime;
        
        playerMovement.lastTimeJumpPressed -= Time.deltaTime;
        
        animator.SetBool(Animator.StringToHash("isMoving"), playerMovement.IsMoving);
        animator.SetBool(Animator.StringToHash("isGrounded"), playerMovement.IsGrounded); 
        animator.SetBool(Animator.StringToHash("isFalling"), playerMovement.IsFalling);
        
        if (playerMovement.lastTimeJumpPressed > 0 &&
            playerMovement.lastTimeGrounded > 0 &&
            playerMovement.IsClimbing == false)
        {
            animator.SetTrigger(Animator.StringToHash("Jumping"));
            playerMovement.lastTimeJumpPressed = 0;
            playerMovement.lastTimeGrounded = 0;
        }
    }

    #endregion

    #endregion

    #region Attack Methods

    public void TriggerAction(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        animator.SetBool(Animator.StringToHash("isPerformingAction"), true);
    }

    public ItemInfo GetEquippedItem()
    {
        if (Inventory.Instance)
        {
            return Inventory.Instance.GetSelectedItem();
        }
        return null;
    }

    #endregion

    // This region contains public methods used to move the player. This can be refactored into 
    // each individual state if this gets too cumbersome, but I am leaving it here for now because multiple
    // states might use similar movement logic.
    
    #region Collision Methods

    /**
     * <summary>
     * Disables all colliders on the player.
     * </summary>
     */
    public void DisableColliders()
    {
        foreach (var col in allCols)
            col.enabled = false;
    }
    
    /**
     * <summary>
     * Enables all colliders on the player.
     * </summary>
     */
    public void EnableColliders()
    {
        foreach (var col in allCols)
            col.enabled = true;
    }
    
    /**
     * <summary>
     * Ignores collisions between the player and a specified layer.
     * </summary>
     * <param name="layer">The layer to ignore collisions with.</param>
     * <param name="ignore">Whether to ignore (true) or re-enable (false
     */
    public void IgnoreCollisionWithLayer(int layer, bool ignore = true)
    {
        foreach (var col in allCols)
            Physics.IgnoreLayerCollision(gameObject.layer, layer, ignore);
    }
    
    /**
     * <summary>
     * Ignores collisions between the player and a specified GameObject.
     * </summary>
     * <param name="obj">The GameObject to ignore collisions with.</param>
     * <param name="ignore">Whether to ignore (true) or re-enable (false) collisions.</param>
     */
    public void IgnoreCollisionWithObject(GameObject obj, bool ignore = true)
    {
        foreach (var col in allCols)
            foreach (var objCol in obj.GetComponents<Collider>())
                Physics.IgnoreCollision(col, objCol, ignore);
    }
    
    #endregion
}
