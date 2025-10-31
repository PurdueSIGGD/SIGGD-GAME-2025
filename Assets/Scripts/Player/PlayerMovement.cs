using UnityEngine;
using FMOD.Studio;
using FMOD;
using Utility;

public class PlayerMovement : MonoBehaviour
{
    private PlayerID playerID;

    [Header("Parameter SOs")]
    public MoveData moveData; // ScriptableObject containing movement parameters.

    private EventInstance footsteps;

    [HideInInspector] public Rigidbody rb;

    #region Movement Attributes

    [HideInInspector] public Vector3 moveDirection; // The 3D direction the player is currently moving in.
    public bool IsMoving { get; set; } // Whether the player is currently moving.
    public bool IsClimbing => PlayerID.Instance.gameObject.GetComponent<ClimbAction>().IsClimbing();

    #endregion

    #region Check Attributes

    public float GravityScale { get; private set; } = 1f; // A scale on gravity when applied to the player,
                                                          // relevant when the player is in situations like falling
                                                          // or hanging on a jump at the apex

    [Header("Checks")]
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] Vector3 groundCheckSize = new Vector3(0.49f, 0.3f, 0.49f);
    [SerializeField] LayerMask groundLayer;

    // State params
    private PlayerStateMachine psm;
    private bool isGrounded;
    private bool isSprinting;
    private bool isFalling;
    public bool canMove = true;

    #endregion

    private void Start()
    {
        playerID = GetComponent<PlayerID>();
        rb = GetComponent<Rigidbody>();
        psm = playerID.stateMachine;
    }

    private void Update()
    {
        CalculateGravity();
        isSprinting = PlayerInput.Instance.sprintInput;
        isGrounded = psm.IsGrounded;
        isFalling = psm.IsFalling;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        if (IsMoving && canMove)
        {
            UpdateSound();
            Vector2 moveInput = PlayerInput.Instance.movementInput;

            float speed = (isSprinting && !isFalling) ? moveData.sprintSpeed : moveData.walkSpeed;
            Run(moveInput, speed);
        }
    }

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
        if (!canMove) return;

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
        targetSpeed = Vector3.Lerp(rb.linearVelocity, targetSpeed, lerpAmount);

        float accelRate;
        if (isGrounded)
            accelRate = (Mathf.Abs(targetSpeed.magnitude) > 0.01f) ? moveData.runAccelAmount : moveData.runDecelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed.magnitude) > 0.01f) ? moveData.runAccelAmount * moveData.accelInAir :
                moveData.runDecelAmount * moveData.decelInAir;

        if (isFalling && Mathf.Abs(rb.linearVelocity.y) < moveData.jumpHangSpeedThreshold)
        {
            accelRate *= moveData.jumpHangAccelerationMult;
            targetSpeed *= moveData.jumpHangMaxSpeedMult;
        }

        Vector3 speedDiff = targetSpeed - rb.linearVelocity.SetY(0);
        Vector3 movementForce = speedDiff * accelRate;
        movementForce.y = 0; // prevent any vertical forces from being applied here

        rb.AddForce(movementForce, ForceMode.Acceleration);
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
        rb.linearVelocity = rb.linearVelocity.SetY(0);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    /**
     * <summary>
     * Calculates the appropriate gravity scale based on the player's vertical velocity and grounded state.
     * </summary>
     */
    private void CalculateGravity()
    {
        if (!isGrounded && Mathf.Abs(rb.linearVelocity.y) < moveData.jumpHangSpeedThreshold)
        {
            GravityScale = moveData.GravityScale * moveData.jumpHangGravityMult;
        }
        else if (rb.linearVelocity.y < 0)
        {
            GravityScale = moveData.GravityScale * moveData.fallGravityMult;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -moveData.maxFallSpeed), rb.linearVelocity.z);
        }
        else
        {
            GravityScale = moveData.GravityScale;
        }
    }

    /**
     * <summary>
     * Applies gravity to the player's rigidbody based on the calculated gravity scale.
     * </summary>
     */
    private void ApplyGravity()
    {
        float usedGravityScale = GravityScale;
        if (IsClimbing == true)
        { // while climbing, gravity is unaffected by gravity scale
            usedGravityScale = 1;
        }
        Vector3 gravity = moveData.globalGravity * usedGravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    private void UpdateSound()
    {
        if (rb.linearVelocity.magnitude != 0)
        {
            // NOTE: 3d attributes need to be set in order to play instances in 3d
            ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(rb.position, rb.linearVelocity, transform.forward, Vector3.up);
            footsteps.set3DAttributes(attr);

            PLAYBACK_STATE playbackState;
            footsteps.getPlaybackState(out playbackState);

            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                footsteps.start();
            }
        }
        else
        {
            footsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    #endregion
}