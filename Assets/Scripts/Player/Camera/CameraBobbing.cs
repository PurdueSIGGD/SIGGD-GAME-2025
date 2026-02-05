using System.Numerics;
using UnityEngine;

// place this script on an empty gameobject with the main camera as a child

// When walking along a surface, expect the transform to bob up and down.
// The strength of the bobbing is determined by speed.
// Sway is like bobbing but its horizontal only.
// When airborne, bobbing will pause updating based on vertical speed.
public class CameraBobbing : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] public bool enableBobbing = true;

    #region Attributes for Bobbing
    [Header("//// Bob Attributes /////")]
    [Tooltip("The curve that the bob follows when walking. You could emulate a sin curve on this if you wanted to.")]
    [SerializeField] private AnimationCurve bobWalkingPattern;
    [Tooltip("How fast the bob will update to where it should be placed.")]
    [SerializeField] private float bobUpdateSpeed = 75.0f;

    [Header("Bob Period")]
    [Tooltip("Bob Period when moving beyond max speed.")]
    [SerializeField] private float bobPeriodRealMax = 0.2f;
    [Tooltip("Bob Period when moving at max speed. Higher value = slow bob")]
    [SerializeField] private float bobPeriodMax = 0.3f;
    [Tooltip("Bob Period when moving at zero speed.")]
    [SerializeField] private float bobPeriodMin = 0.7f;

    [Tooltip("Whatever the period of bob is, take that divided by this divisor to get the sway period")]
    [SerializeField] private float swayPeriodDivisor = 4f;

    [Header("Bob Amplitude")]
    [Tooltip("Bob Amplitude when moving at max speed. Higher value = higher bob")]
    [SerializeField] private float bobWalkingAmplitude = 0.1f;
    [Tooltip("How far the camera swings left to right")]
    [SerializeField] private float swayWalkingAmplitude = 0.2f;

    [Header("Bob Speed Detection Attributes")]
    [Tooltip("Speed needed to reach the max bob period")]
    [SerializeField] private float rigidBodyMaxSpeed = 10f;

    [Tooltip("Value after rigidbodyMaxSpeed needed to reach PeriodRealMax. This value is added onto rigidBodyMaxSpeed.")]
    [SerializeField] private float rigidBodySpeedOverflowSpeed = 5f; // DOES NOT NEED TO BE GREATER THAN RIGIDBODY MAX SPEED.

    [Header("//// Airborne Attributes ////")]
    [Tooltip("If the rigid body's vertical speed is greater than this, then bobbing will no longer apply")]
    [SerializeField] private float airborneSpeedCutoff = 0.2f;

    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDecayRate = 1.0f;
    private float currentShakeIntensity = 0f;
    private Vector3 shakeOffset;
    #endregion

    // important variables
    private float bobTimer = 0.0f;
    private float targetBobPositionY = 0;
    private float targetBobPositionX = 0;
    private Rigidbody playerRigidbody;

    #region Start and Update Methods
    private void Start()
    {
        playerRigidbody = PlayerID.Instance.rb;
    }
    private void Update()
    {
        float verticalSpeed = GetVerticalSpeed();
        if (PlayerID.Instance.stateMachine.IsGrounded) {
            GroundedBobbingUpdate();
        } else {
            targetBobPositionX = targetBobPositionY = 0;
        }
        ScreenShakePhysics();
        UpdateBobPosition();
    }
    #endregion

    #region Bobbing Methods
    // UpdateBobPosition() interpolates transform's local position to match
    private void UpdateBobPosition() {
        Vector3 targetBobPosition = new Vector3(targetBobPositionX, targetBobPositionY, 0f) + shakeOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetBobPosition, Time.deltaTime * bobUpdateSpeed);
    }

    // GroundedBobbingUpdate() Call when "grounded". Sets the target bob position to incorperate basic bobbing and swaying based on rigidbody velocity
    private void GroundedBobbingUpdate() { 
        // get speed values and stuff
        float horizontalSpeed = GetHorizontalSpeed();
        float horizontalSpeedPrecentage = horizontalSpeed / rigidBodyMaxSpeed;
        float horizontalSpeedPrecentageClamped = Mathf.Clamp(horizontalSpeedPrecentage, 0f, 1f);

        // get bob amplitude
        float bobAmplitude = bobWalkingAmplitude * horizontalSpeedPrecentageClamped;

        // get bob period
        float bobPeriod = bobPeriodMax;
        if (horizontalSpeedPrecentage > 1.0f) { // when going beyond max speed
            float overflowHorizontalSpeedPrecentage = Mathf.Min(0, (horizontalSpeed - rigidBodyMaxSpeed) / rigidBodySpeedOverflowSpeed);
            bobPeriod = (bobPeriodRealMax - bobPeriodMax) * overflowHorizontalSpeedPrecentage + bobPeriodMax;
        } else { // when going under max speed
            bobPeriod = (bobPeriodMax - bobPeriodMin) * horizontalSpeedPrecentageClamped + bobPeriodMin;
        }

        bobTimer += Time.deltaTime / bobPeriod;

        // get sway amplitude
        float swayAmplitude = swayWalkingAmplitude * horizontalSpeedPrecentageClamped;

        // putting it all together
        float bobTimeEvaluated = bobWalkingPattern.Evaluate(bobTimer % 1);
        float targetAmplitude = bobAmplitude * bobTimeEvaluated;

        float swayTimeEvaluated = Mathf.Sin(((bobTimer / swayPeriodDivisor) % 1) * (2 * Mathf.PI));
        float targetSwayAmpltitude = swayAmplitude * swayTimeEvaluated;

        // set target bob positions
        targetBobPositionX = targetSwayAmpltitude;
        targetBobPositionY = targetAmplitude;

        // reset bob timer if I haven't bobbed in awhile
        if (horizontalSpeed <= 0.001f) {
            bobTimer = 0f;
        }
    }
    #endregion

    #region Camera Shake Methods
    //Change the current shake intensity
    public void ApplyShake(float intensity)
    {
        currentShakeIntensity = intensity;
    }

    //Triggers a small screen shake
    public void TriggerSmallShake()
    {
        ApplyShake(0.05f);
    }
    
    //Triggers a medium screen shake
    public void TriggerMediumShake()
    {
        ApplyShake(0.15f);
    }
    
    //Triggers a large screen shake
    public void TriggerLargeShake()
    {
        ApplyShake(0.4f);
    }

    //Handles the screen shake physics
    private ScreenShakePhysics(){
        if (currentShakeIntensity > 0)
        {
            //Shake offset for X, Y, and Z variables
            shakeOffset = new Vector3 (
                Random.Range(-1f, 1f) * currentShakeIntensity,
                Random.Range(-1f, 1f) * currentShakeIntensity,
                0f
            );
            currentShakeIntensity = currentShakeIntensity - Time.deltaTime * shakeDecayRate;
        } else
        {
            shakeOffset = Vector3.Zero;
            currentShakeIntensity = 0f;
        }
    }

    #endregion

    #region Helper Functions
    // speed functions for vertical and horizontal speeds
    private float GetHorizontalSpeed() {
        return (new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z)).magnitude;
    }
    private float GetVerticalSpeed() {
        return playerRigidbody.linearVelocity.y;
    }
    #endregion
}