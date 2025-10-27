using UnityEngine;
using UnityEngine.Serialization;


/**
 * <summary>
 * A ScriptableObject to hold various parameters for rigidbody movements.
 * </summary>
 */
[CreateAssetMenu(fileName = "Move Data", menuName = "ScriptableObjects/Move Data", order = 2)]
public class MoveData : ScriptableObject
{
    [Header("Lateral Movement Parameters")]
    
    [Tooltip("Standard movement speed.")]
    public float walkSpeed = 10f;
    
    [Tooltip("Sprint movement speed.")]
    public float sprintSpeed = 15f;
    
    [Tooltip("Acceleration rate for reaching target speed.")]
    [SerializeField] private float runAcceleration = 7f;
    
    [HideInInspector]
    public float runAccelAmount; //Calculated acceleration rate for reaching target speed (currently not calculated, but could be in the future).



    [Tooltip("Deceleration rate for stopping movement.")] 
    [SerializeField] private float runDeceleration = 7;

    [HideInInspector]
    public float runDecelAmount; //Calculated deceleration rate for stopping movement (currently not calculated, but could be in the future).

    [Tooltip("How smoothly to interpolate between speeds. 1 is instant, 0 is no movement.")]
    public float movementInterpolation = 1;
    
    [Header("Air Movement Parameters")]
    
    [Header("Jump")]
    [Tooltip("Height of the player's jump.")]
    public float jumpHeight;
    [Tooltip("Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.")]
    public float jumpTimeToApex;
    
    [Header("Falling")]
    [Tooltip("Strength of Gravity")]
    public float globalGravity = -9.81f;
    [Tooltip("Multiplier to the player's gravityScale when falling.")] public float fallGravityMult;
    [Tooltip("Maximum fall speed (terminal velocity) of the player when falling.")] public float maxFallSpeed;

    public float GravityStrength => -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
    public float GravityScale => GravityStrength / globalGravity;
    public float JumpForce => Mathf.Abs(GravityStrength) * jumpTimeToApex;
    
    [Tooltip("Reduces gravity while close to the apex (desired max height) of the jump.")]
    [Range(0f, 1)] public float jumpHangGravityMult;
    [Tooltip("Speeds (close to 0) where the player will experience extra 'jump hang'. The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function).")]
    public float jumpHangSpeedThreshold;
    
    [Tooltip("Multiplier to jump hang acceleration.")]
    public float jumpHangAccelerationMult; 
    
    [Tooltip("Multiplier to jump hang max speed.")]
    public float jumpHangMaxSpeedMult; 			
    
    [Tooltip("Multiplier applied to acceleration rate when airborne.")]
    [Range(0f, 1)] public float accelInAir;
    [Tooltip("Multiplier applied to deceleration rate when airborne.")]
    [Range(0f, 1)] public float decelInAir;
    
    [Header("Assists")]
    [Tooltip("Grace period after falling off a platform, where you can still jump.")]
    [Range(0.01f, 0.5f)] public float coyoteTime;
    [Tooltip("Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.")]
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;
    
    private void OnValidate()
    {
        //Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = runAcceleration;
        runDecelAmount = runDeceleration;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, walkSpeed);
        runDeceleration = Mathf.Clamp(runDeceleration, 0.01f, walkSpeed);
        #endregion
    }

}
