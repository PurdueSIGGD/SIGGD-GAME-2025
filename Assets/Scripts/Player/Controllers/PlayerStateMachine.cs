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

    #endregion
    
    #region Parameter SOs
    
    [Header("Parameter SOs")]
    public MoveData moveData; // ScriptableObject containing movement parameters.
    public ItemInfo defaultItem; // The default item the player starts with.

    #endregion

    #region Check Attributes

    public float gravityScale { get; private set; } = 1f; // A scale on gravity when applied to the player,
                                                          // relevant when the player is in situations like falling
                                                          // or hanging on a jump at the apex
    
    [Header("Checks")] 
    [SerializeField] public Transform groundCheckPoint;
    [SerializeField] public Vector3 groundCheckSize = new Vector3(0.49f, 0.3f, 0.49f);
    public LayerMask groundLayer;
    
    public bool IsGrounded =>
        Physics.CheckBox(groundCheckPoint.position, groundCheckSize, Quaternion.identity, groundLayer);
    
    private float lastTimeGrounded, lastTimeJumpPressed;
    
    public Vector3 LastGroundedPosition { get; private set; }
    
    public bool IsFalling => playerID.rb.linearVelocity.y < -0.1f && !IsGrounded && !IsClimbing;
    
    #endregion
    
    #region Movement Attributes
    public bool IsMoving => PlayerInput.Instance.movementInput.magnitude > 0.1f && !IsClimbing; // Whether the player is currently moving.
    public bool IsClimbing => PlayerID.Instance.gameObject.GetComponent<ClimbAction>().IsClimbing();
    
    #endregion
    
    #region MonoBehaviour Callbacks
    
    private void Start()
    {
        playerID = PlayerID.Instance; // Running this in Start to ensure PlayerID is initialized first
        
        mainCol = GetComponent<Collider>();
        allCols = GetComponentsInChildren<Collider>();

        PlayerInput.Instance.OnJump += OnJumpAction;
        PlayerInput.Instance.OnAction += TriggerAction;
    }

    private void Update()
    {
        UpdateAnimatorParams();
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
        
        lastTimeJumpPressed = moveData.jumpInputBufferTime;
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
        animator.SetBool(Animator.StringToHash("isClimbing"), IsClimbing);
        if (IsGrounded)
        {
            lastTimeGrounded = moveData.coyoteTime;
            LastGroundedPosition = playerID.transform.position;
        }
        else
            lastTimeGrounded -= Time.deltaTime;
        
        lastTimeJumpPressed -= Time.deltaTime;
        
        animator.SetBool(Animator.StringToHash("isMoving"), IsMoving);
        animator.SetBool(Animator.StringToHash("isGrounded"), IsGrounded); 
        animator.SetBool(Animator.StringToHash("isFalling"), IsFalling);
        
        if (lastTimeJumpPressed > 0 && lastTimeGrounded > 0 && IsClimbing == false)
        {
            animator.SetTrigger(Animator.StringToHash("Jumping"));
            lastTimeJumpPressed = 0;
            lastTimeGrounded = 0;
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
