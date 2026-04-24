using UnityEngine;
using FMOD.Studio;
using FMOD;
using Utility;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Parameter SOs")]
    public MoveData moveData; // ScriptableObject containing movement parameters.

    private EventInstance footsteps;

    private static string jumpSound = "Jump";
    private static readonly string[] labScenes = {
        "ShipScene",
        "NathanA0Scene"
    };

    public string playerLandSound = "PlayerLand";
    public string playerHeavyLandSound = "PlayerLandHeavy";

    [HideInInspector] public Rigidbody rb;

    #region Footstep Sound Attributes

    [SerializeField] private int footstepWaitCount = 5;
    private int footstepSprintWaitCount;
    private int footstepDelayCount = 0;

    #endregion

    #region Movement Attributes
    private bool IsMoving => PlayerID.Instance.stateMachine.IsMoving;
    public bool IsClimbing => PlayerID.Instance.stateMachine.IsClimbing;

    public float speedMultiplier;

    #endregion

    #region Check Attributes

    public float GravityScale { get; private set; } = 1f; // A scale on gravity when applied to the player,
                                                          // relevant when the player is in situations like falling
                                                          // or hanging on a jump at the apex

    // State params
    private PlayerStateMachine psm;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isFalling;
    public bool canMove = true;

    private int wasFalling;
    private int justJumped;

    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb = GetComponent<Rigidbody>();
        psm = PlayerID.Instance.stateMachine;
        speedMultiplier = 1f;

        footstepSprintWaitCount = 0;// use default footstep sound timing for sprinting
        // Set footstep sound to LabFootsteps if it is one of the prologue scenes
        foreach (string scene in labScenes) {
            if (SceneManager.GetActiveScene().name == scene) {
                FMODEvents.Instance.GetEventInstance("LabFootsteps", instance => { footsteps = instance; });
                jumpSound = "LabJump";
                playerLandSound = "LabLandFromJump";
                playerHeavyLandSound = "LabLandFromJump";
                footstepWaitCount *= 2; // double wait count for lab footsteps since they are shorter
                footstepSprintWaitCount = footstepWaitCount / 2;
                return;
            }
        }
        
        // Set footstep sound to regular Footsteps for all other scenes
        FMODEvents.Instance.GetEventInstance("Footsteps", instance => { footsteps = instance; });
    }

    private void Update()
    {
        CalculateGravity();
        isSprinting = psm.IsSprinting;
        isCrouching = psm.IsCrouched;
        isGrounded = psm.IsGrounded;
        isFalling = psm.IsFalling;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        UpdateFootstepSound();
        if (IsMoving && canMove)
        {
            Vector2 moveInput = PlayerInput.Instance.movementInput;

            float speed = (isSprinting && !isFalling) ? moveData.sprintSpeed : moveData.walkSpeed;
            speed *= speedMultiplier;
            if (isCrouching == true) {
                speed = moveData.crouchSpeed;
            }

            Run(moveInput, speed * Time.fixedDeltaTime);
        }
        else if (isGrounded)
        {
            Vector2 moveInput = PlayerInput.Instance.movementInput;

            float speed = 0f;
            Run(moveInput, speed * Time.fixedDeltaTime);
        }

        /*if (isGrounded && !IsMoving)
        {
            //rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            //rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, 0f);
        } */
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

        Transform cam = PlayerID.Instance.cameraMovement.transform;
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

        //TEMP FIX
        if (isGrounded && !IsMoving && rb.linearVelocity.magnitude <= 0.05f) return;

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
        GetComponent<PlayerStamina>().StaminaJump();  // decrease stamina
        SetJustJumped();
        AudioManager.Instance.PlayOneShotNoAsync(jumpSound, PlayerID.Instance.gameObject.transform.position);
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
        // TEST
        //if (isGrounded) return;

        float usedGravityScale = GravityScale;
        if (IsClimbing == true)
        { // while climbing, gravity is unaffected by gravity scale
            usedGravityScale = 1;
        }
        Vector3 gravity = moveData.globalGravity * usedGravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    private void UpdateFootstepSound()
    {
        if ((rb.linearVelocity.magnitude >= 1) && (isGrounded))
        {
            // Don't play footsteps if player jumps or lands for 5 times
            if (wasFalling > 0) {
                wasFalling--;
                return;
            }
            if (justJumped > 0) {
                justJumped--;
                return;
            }
            // NOTE: 3d attributes need to be set in order to play instances in 3d
            //ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(rb.position, rb.linearVelocity, rb.linearVelocity / rb.linearVelocity.magnitude, rb.transform.up);
            ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(rb.position, rb.linearVelocity, transform.forward, Vector3.up);
            footsteps.set3DAttributes(attr);

            PLAYBACK_STATE playbackState;
            footsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                if (footstepDelayCount > 0)
                {
                    footstepDelayCount--;
                }
                else {
                    footsteps.start();
                    if (isSprinting)
                    {
                        footstepDelayCount = footstepSprintWaitCount;
                    }
                    else
                    {
                        footstepDelayCount = footstepWaitCount;
                    }
                }
                
            }
        }
        else
        {
            footsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void SetWasFalling() {
        wasFalling = 5;
    }
    public void SetJustJumped() {
        justJumped = 5;
    }

    /**
     * <summary>
     * Switches the footstep sound to the regular footsteps audio
     * </summary>
     */
    public void SwitchFootstepSound() {
        UnityEngine.Debug.Log("Switching footstep and jump sound");
        FMODEvents.Instance.GetEventInstance("Footsteps", instance => { footsteps = instance; });
        jumpSound = "PlayerJump";
        playerLandSound = "PlayerLand";
        playerHeavyLandSound = "PlayerLandHeavy";
        footstepWaitCount /= 2; // divide by 2 to reset
        footstepSprintWaitCount = 0;
    }

    #endregion
}