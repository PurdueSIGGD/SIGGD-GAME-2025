using System;
using UnityEngine;
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

    #endregion
    
    #region Parameter SOs
    
    [Header("Parameter SOs")]
    public MoveData moveData; // ScriptableObject containing movement parameters.
    
    #endregion
    
    #region Movement Attributes
    
    [HideInInspector] public Vector3 moveDirection; // The 3D direction the player is currently moving in.
    public bool IsMoving => PlayerInput.Instance.movementInput.magnitude > 0.1f; // Whether the player is currently moving.
    
    #endregion
    
    #region MonoBehaviour Callbacks
    
    private void Start()
    {
        playerID = PlayerID.Instance; // Running this in Start to ensure PlayerID is initialized first
    }

    private void Update()
    {
        UpdateAnimatorParams();
    }
    
    // Because the animator is our state machine, we update parameters there to control state transitions.
    #region Animator Methods
    
    /** <summary>
     * Updates the animator parameters based on the player's current state.
     * </summary>
     */
    private void UpdateAnimatorParams()
    {
        animator.SetBool(Animator.StringToHash("isMoving"), IsMoving);
    }
    
    #endregion

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


        float accelRate = (Mathf.Abs(targetSpeed.magnitude) > 0.01f) ? moveData.runAccelAmount : moveData.runDecelAmount;
		
        Vector3 speedDiff = targetSpeed - playerID.rb.linearVelocity.SetY(0);
		
        Vector3 movementForce = speedDiff * accelRate;
		
        playerID.rb.AddForce(movementForce, ForceMode.Acceleration);
    }
    
    #endregion
}
