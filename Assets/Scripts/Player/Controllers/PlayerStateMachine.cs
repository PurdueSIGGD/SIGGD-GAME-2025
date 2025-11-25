using System;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
using static UnityEngine.UI.Image;

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
        //Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.2f, groundLayer);
    
    private float lastTimeGrounded, lastTimeJumpPressed;
    
    public Vector3 LastGroundedPosition { get; private set; }
    
    public bool IsFalling => playerID.rb.linearVelocity.y < -0.1f && !IsGrounded && !IsClimbing;

    public bool IsSprinting;

    public bool HasStamina => PlayerID.Instance.GetComponent<PlayerStamina>().CurrentStamina>0;
    
    #endregion
    
    #region Movement Attributes
    public bool IsMoving => PlayerInput.Instance.movementInput.magnitude > 0.1f && !IsClimbing; // Whether the player is currently moving.
    public bool IsClimbing => PlayerID.Instance.gameObject.GetComponent<ClimbAction>().IsClimbing();
    public bool IsCrouched = false;

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
        UpdateCrouchState();
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
        animator.SetBool(Animator.StringToHash("isCrouched"), IsCrouched);
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
        animator.SetBool(Animator.StringToHash("isSprinting"), PlayerInput.Instance.sprintInput);
        animator.SetBool(Animator.StringToHash("hasStamina"), HasStamina);


        if (lastTimeJumpPressed > 0 && lastTimeGrounded > 0 && IsClimbing == false)
        {
            animator.SetTrigger(Animator.StringToHash("Jumping"));
            lastTimeJumpPressed = 0;
            lastTimeGrounded = 0;
        }
    }

    private void UpdateCrouchState() {
        bool crouchInput = PlayerInput.Instance.crouchInput;
        bool isclimb = IsClimbing;

        if ((crouchInput == true && CanCrouch() || isclimb == true) && IsCrouched == false) {
            ToggleCrouch(true);
        } else if ((crouchInput == false || IsGrounded == false) && IsCrouched == true && isclimb == false) {
            ToggleCrouch(false);
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
     * Sets the collider height of the player. 
     * This is forced to be more than the radius*2 of the collider.
     * Returns false and does not set the height if there is something in the way.
     * </summary>
     */
    public bool SetColliderHeight(float colliderHeight)
    {
        CapsuleCollider playerCollider = (CapsuleCollider)mainCol;
        colliderHeight = Mathf.Max(playerCollider.radius * 2, colliderHeight);


        float oldHeight = playerCollider.height;
        float heightDifference = colliderHeight - oldHeight; // difference in heights
        float positionDifference = 0.5f * heightDifference; // how much the player's collider shifts

        if (heightDifference > 0) {
            float radius = playerCollider.radius;
            Vector3 p1 = playerCollider.center + transform.position;

            int circlePrecision = 16;
            float angleIncrement = 360f / circlePrecision;

            // fires (circlePrecision) rays in a circle around the player, if any collide, then explode blow up kablam.
            for (int i = 0; i < circlePrecision; i++) {
                float angle = i * angleIncrement;
                float angleRad = angle * Mathf.Deg2Rad;

                Vector3 originP = p1;
                originP.x += radius * Mathf.Cos(angleRad);
                originP.z += radius * Mathf.Sin(angleRad);
                if (Physics.Raycast(originP, Vector3.up, oldHeight / 2f + heightDifference, groundLayer)) {
                    return false;
                }
            }
        }


        playerCollider.height = colliderHeight;
        playerCollider.center += new Vector3(0, positionDifference, 0);

        foreach (Collider col in allCols) {
            if (col is CapsuleCollider) {
                // only the lateral collider should be edited here, which has the same sizes as player collider
                ((CapsuleCollider)col).height = playerCollider.height;
                ((CapsuleCollider)col).center = playerCollider.center;
            }
        }

        return true;
    }


    // returns true if the player can initiate a crouch
    public bool CanCrouch() {
        return IsGrounded == true;
    }

    // if toggle == true, begin crouching. Otherwise, uncrouch. Returns false if unable to uncrouch.
    public bool ToggleCrouch(bool toggle) {
        bool returnedValue = SetColliderHeight(toggle ? moveData.crouchingPlayerHeight : moveData.standingPlayerHeight);

        if (returnedValue == true) {
            IsCrouched = toggle;
        }

        return returnedValue;
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
