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
    
    public bool IsFalling => playerID.rb.linearVelocity.y < -0.1f && !IsGrounded;
    
    #endregion
    
    #region Movement Attributes
    
    [HideInInspector] public Vector3 moveDirection; // The 3D direction the player is currently moving in.
    public bool IsMoving => PlayerInput.Instance.movementInput.magnitude > 0.1f; // Whether the player is currently moving.
    
    #endregion
    
    #region MonoBehaviour Callbacks
    
    private void Start()
    {
        playerID = PlayerID.Instance; // Running this in Start to ensure PlayerID is initialized first
        
        mainCol = GetComponent<Collider>();
        allCols = GetComponentsInChildren<Collider>();
    }

    private void Update()
    {
        UpdateAnimatorParams();
        CalculateGravity();
        
    }

    private void FixedUpdate()
    {
        ApplyGravity();
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
        
        if (lastTimeJumpPressed > 0 && lastTimeGrounded > 0)
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
        return Inventory.Instance.GetSelectedItem();
    }

    #endregion

    // This region contains public methods used to move the player. This can be refactored into 
    // each individual state if this gets too cumbersome, but I am leaving it here for now because multiple
    // states might use similar movement logic.
    #region State Methods

    /**
     * <summary>
     * Updates the player's movement based on the current moveDirection and camera orientation.
     * </summary>
     *
     * <param name="moveInput">The input vector representing the desired movement direction (x for right/left, y for forward/backward).</param>
     * <param name="speed">The speed at which the player should move.</param>
     */
    public void Run(Vector2 moveInput, float speed)
    {
        Transform cam = playerID.cam.transform;
        Vector3 direction = moveInput.x * cam.right.SetY(0).normalized + 
                               moveInput.y * cam.forward.SetY(0).normalized;
        
        MoveInDirectionWithSpeed(direction, speed, moveData.movementInterpolation);
    }
    
    /**
     * <summary>
     * Moves the player in the specified direction using its rigidbody with optional smoothing.
     * </summary>
     *
     * <param name="direction">The direction to move the player in.</param>
     * <param name="speed">The target speed to move the player at.</param>
     * <param name="lerpAmount">The amount to lerp the player's velocity towards the target speed. Default is 1 (no smoothing).</param>
     */
    public void MoveInDirectionWithSpeed(Vector3 direction, float speed, float lerpAmount = 1)
    {
        moveDirection = direction;
        
        Vector3 targetSpeed = direction * speed;
        targetSpeed = Vector3.Lerp(playerID.rb.linearVelocity, targetSpeed, lerpAmount);


        float accelRate;
        if (IsGrounded)
            accelRate = (Mathf.Abs(targetSpeed.magnitude) > 0.01f) ? moveData.runAccelAmount : moveData.runDecelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed.magnitude) > 0.01f) ? moveData.runAccelAmount * moveData.accelInAir : 
                moveData.runDecelAmount * moveData.decelInAir;
        
        if (IsFalling && Mathf.Abs(playerID.rb.linearVelocity.y) < moveData.jumpHangSpeedThreshold)
        {
            accelRate *= moveData.jumpHangAccelerationMult;
            targetSpeed *= moveData.jumpHangMaxSpeedMult;
        }
		
        Vector3 speedDiff = targetSpeed - playerID.rb.linearVelocity.SetY(0);
		
        Vector3 movementForce = speedDiff * accelRate;
        
		
        playerID.rb.AddForce(movementForce, ForceMode.Acceleration);
    }

    /**
     * <summary>
     * Makes the player jump by applying an upward force based on the parameters in the scriptable object.
     * </summary>
     *
     * <param name="force">The force to apply for the jump.</param>
     */
    public void Jump(float force)
    {
        playerID.rb.linearVelocity = playerID.rb.linearVelocity.SetY(0);
        playerID.rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
    }
    
    /**
     * <summary>
     * Calculates the appropriate gravity scale based on the player's vertical velocity and grounded state.
     * </summary>
     */
    private void CalculateGravity()
    {
        if (!IsGrounded && Mathf.Abs(playerID.rb.linearVelocity.y) < moveData.jumpHangSpeedThreshold)
        {
            gravityScale = moveData.GravityScale * moveData.jumpHangGravityMult;
        }
        else if (playerID.rb.linearVelocity.y < 0)
        {
            gravityScale = moveData.GravityScale * moveData.fallGravityMult;
            playerID.rb.linearVelocity = new Vector3(playerID.rb.linearVelocity.x, 
                Mathf.Max(playerID.rb.linearVelocity.y, -moveData.maxFallSpeed), playerID.rb.linearVelocity.z);
        }
        else
        {
            gravityScale = moveData.GravityScale;
        }
    }
    
    /**
     * <summary>
     * Applies gravity to the player's rigidbody based on the calculated gravity scale.
     * </summary>
     */
    private void ApplyGravity()
    {
        Vector3 gravity = moveData.globalGravity * gravityScale * Vector3.up;
        playerID.rb.AddForce(gravity, ForceMode.Acceleration);
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
