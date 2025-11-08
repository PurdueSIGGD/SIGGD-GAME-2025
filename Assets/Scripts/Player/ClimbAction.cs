using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Utility;
using static UnityEngine.UI.Image;

// KNOWN JANK HERE:
/*
 * If the player leans back and forth from a wall and then releases, they can
 * sometimes launch themselve's further than a walljump. I added 'disableWalljumpBoosting' to
 * prevent the player from combining this mechanic with the walljump to jump extremely far. But in some cases,
 * swinging back and forth launches the player further than a walljump.
 * On one hand, this is problematic. On the other hand, its really fun and awesome.
 * 
 * Also, one problematic issue is the player being able to stack their velocities
 * with chained walljumps.
 * this stinks to solve.
 */

// DESCRIPTION:
/* Script allows the player to wallclimb and climb on walls and walljump off climbable walls
 * 
 * When the player starts climbing, they will only exit the climbing state if:
 * No hands are attached to walls
 * and
 * Player is grounded <- THIS IS NOT IMPLEMENTED YET! PLEASE IMPLEMENT IT IN THE "TryToExitClimbMode()" METHOD
 * 
 * If handTransformPrefab is null, then an empty gameobject is created and used.
 * If handTransformPrefab is not null, then the prefab is used in place of the hand positions.
 * 
 * Climbable objects must have the appropriate layermask, and be static.
 */

// Non-static surfaces
/*
 * This refers to objects that can move being climbable.
 * Currently, this version does not support that.
 * If someone mad wishes to implement that, I'd recommend parenting the climbing transforms to the object they are
 * climbing on and then apply a force on that object based on the forces the player experiences.
 * But this would be hell to implement so, for now, only static climbable objects exist.
 */

// IMPORTANT METHODS:
/* 
 * bool IsClimbing() -- returns isClimbing variable. or just "is the player actively climbing?
 * void EnterClimbMode() -- tries to enter climbing mode by grabbing a nearby wall
 * void ExitClimbMode() -- forces the player to exit climbing mode. Use if the player is forced out of climbing.
 * 
 * 
 * void InputHand(bool, Hand) -- inputs a hand. bool equals if the input was "pressed down". Hand is the hand we are inputting
 * 
 * Hand is a public enum for this class.
 * 
 * 
 * bool[] GetAttachedHands() -- returns boolean array of size 2 indicating hands attached to a wall.
 * bool[] GetHeldDownHands() -- returns boolean array of size 2 indicating hands held down.
 * -- these boolean arrays of size 2: index 0 = left hand. index 1 = right hand. if attached or held down, their value is true
 * 
 * 
 * Transform[] GetHandTransforms() -- returns array of hand transform
 */

// this allows the player to wall climb
public class ClimbAction : MonoBehaviour
{
    private bool isClimbing = false;

    public bool IsClimbing() { 
        return isClimbing;
    }

    #region preset variables
    // presets
    private Rigidbody playerRigidbody; // retrieved from playerID
    private Transform cameraTransform; // retrieved from playerID
    private PlayerStateMachine stateMachine;
    private PlayerInput playerInput; // expected to be parented to the player object
    #endregion

    #region Climbable surface tagging
    [Header("Preset - Climbable Surface Tagging")]
    [SerializeField] private string climbingLayerMaskName = "Climbable";
    private LayerMask climbingLayerMask;
    #endregion

    #region Configuration Settings
    // private configurations are not meant to be based on player preference
    [Header("Public Configurations")]
    [Tooltip("If true, you must press to attach and detach hands separately")]
    [SerializeField] public bool handModeIsToggled = false;

    [Tooltip("Holding jump or crouch will move the player up or down (leaning). Will conflict with walljump")]
    // this is lowkey broken, just always set it to false
    [SerializeField] public bool influenceVerticalInputWithCrouchAndJump = false;


    [Header("Private Configurations")]
    [Tooltip("Walljump mechanic toggle for testing")]
    [SerializeField] private bool walljumpEnabled = true;

    [Tooltip("If the player leans in the direction of a walljump, they gain a boost. This will disable that")]
    [SerializeField] private bool disableWalljumpBoosting = false;

    [Tooltip("If true, the player's walljump will be in their leaning direction, and not based on the wall's normal vector")]
    [SerializeField] private bool walljumpUsesLeanDirection = true;

    [Tooltip("If true, having one hand attached will inflict one-handed penalties")]
    [SerializeField] private bool oneHandedPenalties = true;

    [Tooltip("If true, additional raycast and raycastRadius are used to make grabbing walls more generous.")]
    [SerializeField] private bool useRaycastRadius = true;

    [Tooltip("If true, the player's wall jump is diminished if they grab a wall after wall jumping. ")]
    [SerializeField] private bool enableWalljumpFatigue = true;

    [Tooltip("If true, linear velocity upon walljumping will be clamped to maxWallJumpVelocity.")]
    [SerializeField] private bool clampWalljumpVelocity = true;
    #endregion

    #region Leaning attributes
    [Header("Leaning")]
    [Tooltip("Determines the 'radius' the player can move away from climbing target")]
    [SerializeField][Range(0f, 5f)] private float leaningMagnitude = 1.25f;

    // input directions
    private Vector3 leaningInputDirection;
    private Vector3 leaningReachingDirection;
    #endregion



    #region start, awake, and update methods
    private void Start() {
        climbingLayerMask = LayerMask.GetMask(climbingLayerMaskName);

        PlayerID playerID = FindFirstObjectByType<PlayerID>();
        GameObject playerObject = playerID.gameObject;
        playerRigidbody = playerID.rb;
        cameraTransform = playerID.cam.transform;
        stateMachine = playerID.stateMachine;
        playerInput = playerObject.GetComponent<PlayerInput>();
    }

    private void Awake() {
        // create empty hand prefab is none is given
        GameObject temporaryHandPrefab = null;
        if (handTransformPrefab == null) {
            temporaryHandPrefab = new GameObject("ClimbingHand");
            handTransformPrefab = temporaryHandPrefab;
        }

        // create hands from hand prefab
        for (int i = 0; i < handTransforms.Length; i++) {
            GameObject newHand = Instantiate(handTransformPrefab);
            handTransforms[i] = newHand.transform;
            newHand.name = "Climbing Hand: (" + ((Hand)i) + ")";
        }

        // destroy temporary hand prefab if one was used
        if (temporaryHandPrefab != null) {
            Destroy(temporaryHandPrefab);
        }
    }

    private void Update() {
        if (isClimbing == true) {
            // input
            UpdateInputReadings();

            // hand input for held down hands
            UpdateHeldHands();

            // get climbing target
            bool handAttached = isHandAttached();
            if (handAttached == true) {
                UpdateClimbingMovement();
            }

            // tries to exit climbing mode if allowed
            TryToExitClimbMode();

            /*if (Input.GetKeyDown(KeyCode.R))
            {
                if (isClimbing)
                {
                    ExitClimbMode();
                }
                else
                {
                    EnterClimbMode();
                }
            }*/
        }
    }
    #endregion

    #region Input Methods
    // updates read inputs
    private void UpdateInputReadings() { 
        float verticalInput = 0;
        if (influenceVerticalInputWithCrouchAndJump == true) {
            if (playerInput.jumpInput == true) {
                verticalInput = 1;
            } else if (playerInput.crouchInput == true) {
                verticalInput = -1;
            }
        }
        if (walljumpEnabled == true) {
            TryToWallJump();
        }

        Vector2 horizontalInput = playerInput.movementInput;
        leaningInputDirection = new Vector3(horizontalInput.x, verticalInput, horizontalInput.y);
    }
    private void TryToWallJump() { 
        if (playerInput.jumpInput == true && isHandAttached()) {
            WallJump();
        }
    }
    
    // inputs to hold a hand down. If pressed down is true, the hand is held down. If false, the hand is lifted.
    // this method is called by player input
    public void InputHand(bool pressedDown, Hand handInputted) {
        if (isClimbing == true) { 
            int handIndex = (int)handInputted;
            bool pressInput = pressedDown;

            // If I had just clinged to a wall (phantom hand), then detach that hand to make way
            // for my new hand input! which is more important! it is king!.
            if (phantomHands[handIndex] == true) {
                SetPhantomHand(handIndex, false);
                DetachHand(handInputted);
                heldDownHands[handIndex] = false;
            }

            // if in toggle mode and i want to press hand down
            if (handModeIsToggled == true) {
                if (pressedDown == true) {
                    pressInput = !heldDownHands[handIndex];
                } else { // is im already pressing down, letting go of the mouse shouldn't do anything
                    pressInput = heldDownHands[handIndex];
                }
            }

            InputHand(pressInput, handInputted, true);
        }
    }

    // this function ignores toggled form. Use when it is absolutely necessary to set the pressed status of a hand
    public void InputHand(bool pressedDown, Hand handInputted, bool isForced) {
        int handIndex = (int)handInputted;
        heldDownHands[handIndex] = pressedDown;
        if (pressedDown == true) {
            if (walljumpFatigued == true) {
                walljumpFatigued = false;
                walljumpFatigueTimer = Time.time;
            }
            FireHand(handInputted);
        }
    }

    // gets direction of player's camera
    private Vector3 GetCameraDirection() { 
        return cameraTransform.forward;
    }
    #endregion

    #region Enter/Exit Climbing Methods
    // enters climbing mode if there is a valid climbable wall in front of camera
    public void EnterClimbMode() {
        InputHand(true, Hand.LeftHand, true);
        InputHand(true, Hand.RightHand, true);
        if (isHandAttached()) {
            isClimbing = true;
            stateMachine.ToggleCrouch(false);
            SetPhantomHand(true);
        } else {
            ExitClimb(); // removes held down hands
        }
    }

    // exits the climbing mode. Use 
    public void ExitClimbMode() {
        isClimbing = false;
        ExitClimb();
    }

    // tries to exitclimbmode 
    private float timeSpentGrounded = 0;
    private void TryToExitClimbMode() {
        bool handsAttached = isHandAttached();

        bool isGrounded = stateMachine.IsGrounded;
        if (isGrounded == true) {
            timeSpentGrounded += Time.deltaTime;
        } else {
            timeSpentGrounded = 0;
        }

        if (timeSpentGrounded >= groundedBufferTime && handsAttached == false) {
            ExitClimbMode();
        }
    }

    // detaches all hands. Toggles off the player's held down hand inputs
    public void ExitClimb() {
        DetachHand();
        InputHand(false, Hand.LeftHand, true);
        InputHand(false, Hand.RightHand, true);
        walljumpFatigued = false;
    }
    #endregion

    #region Hand Handling and Hand Methods
    [Header("Hand Things")]
    [Tooltip("Distance from the player head to a grabable wall")]
    [SerializeField] private float grabRange = 2.25f;
    // max grab range is hardcoded as grabrange * 3. this is the dist between two hands

    [Tooltip("Makes the raycast fatter when grabbing walls idk how else to say it.")]
    [SerializeField] [Range(0f, .25f)] private float raycastRadius = 0.1f;

    [Tooltip("Distance from the normal of a hand for target climbing position. How far from the wall the player is.")]
    [SerializeField] private float handToPlayerDistance = 1.75f;

    [Tooltip("If equal to 1, reaching can nullify inputted lean. between 0 and 1, inputted lean will overpower reaching lean. " +
        "If equal to 2, it will fully nullify inputted lean")]
    [SerializeField] [Range(0f, 2f)] private float reachingLeaningMagnitude = 0.5f;

    [Tooltip("When doing a grounded check to see if the player should exit climbing mode, " +
        "the player must be grounded for this amount of time to exit climb mode.")]
    [SerializeField] [Range(0f, 0.2f)] private float groundedBufferTime = 0.05f;

    private bool isReaching = false; // if the player attempts to grab something out of their range, they are reaching

    [SerializeField] private GameObject handTransformPrefab;
    private Transform[] handTransforms = new Transform[2];
    private bool[] heldDownHands = new bool[2]; // hands held down by the player
    private bool[] attachedHands = new bool[2]; // hands attached to a surface

    // this is used in EnterClimbMode and InputHand to let the player quickly input new hand inputs after
    // attaching themself to a surface
    private bool[] phantomHands = new bool[2]; // hands that were JUST attached to a surface.

    public enum Hand {
        LeftHand,
        RightHand
    }

    // returns hand transforms
    public Transform[] GetHandTransforms() {
        return handTransforms;
    }

    // gets the max distance between two attached hands
    private float GetMaxHandDistance() {
        return grabRange * 3;
    }

    // sets both phantom hands to given value
    private void SetPhantomHand(bool toggle) {
        SetPhantomHand(0, toggle);
        SetPhantomHand(1, toggle);
    }
    private void SetPhantomHand(int handIndex, bool toggle) {
        phantomHands[handIndex] = toggle;
    }

    // for each held down hand, fire said hand. If the hand isn't held down, detach said hand
    private void UpdateHeldHands() { 
        isReaching = false;
        for (int i = 0; i < heldDownHands.Length; i++) {
            if (heldDownHands[i] == true) {
                FireHand((Hand)i);
            } else {
                DetachHand((Hand)i);
            }
        }

        // update reaching direction
        if (isReaching == true) {
            leaningReachingDirection = GetCameraDirection();
        } else {
            leaningReachingDirection = Vector3.zero;
        }
    }
    private void AttachHand(Hand handToAttach, Vector3 handPosition, Quaternion handRotation) {
        int handIndex = (int)handToAttach;
        int otherHandIndex = 1 - handIndex;

        if (attachedHands[otherHandIndex] == true) {
            // make sure the hands aren't cartoonishly far apart
            Vector3 otherHandPosition = handTransforms[otherHandIndex].position;
            float twoHandsDistance = (handPosition - otherHandPosition).magnitude;
            if (twoHandsDistance >= GetMaxHandDistance()) {
                return;
            }
        }

        attachedHands[handIndex] = true;
        handTransforms[handIndex].position = handPosition;
        handTransforms[handIndex].rotation = handRotation;
    }
    private void AttachHand(Hand handToAttach, Transform handTransform) {
        AttachHand(handToAttach, handTransform.position, handTransform.rotation);
    }

    // detaches given hand
    private void DetachHand(Hand handToDetach) {
        int handIndex = (int)handToDetach;
        attachedHands[handIndex] = false;
        handTransforms[handIndex].position = Vector3.zero;
    }

    // detaches both hands
    private void DetachHand() {
        DetachHand(Hand.LeftHand);
        DetachHand(Hand.RightHand);
    }

    // returns if the player has any hands attached to a wall
    private bool isHandAttached() {
        for (int i = 0; i < attachedHands.Length; i++) {
            if (attachedHands[i] == true) {
                return true;
            }
        }
        return false;
    }

    // returns number of hands attached
    private int GetNumberOfHandsAttached() {
        int count = 0;
        for (int i = 0; i < attachedHands.Length; i++) {
            if (attachedHands[i] == true) {
                count += 1;
            }
        }
        return count;
    }

    // gets attached hands, a boolean array of size 2 where index 0 = left hand, index 1 = right hand
    public bool[] GetAttachedHands() {
        return attachedHands;
    }

    // gets held down hands, a boolean array of size 2 where index 0 = left hand, index 1 = right hand
    public bool[] GetHeldDownHands() {
        return heldDownHands;
    }

    // returns if the player is currently reaching out their hand
    public bool isHandReaching() {
        return isReaching;
    }

    // fire this function if a hand is held down, for each held down hand.
    private void FireHand(Hand handToFire) {
        int handIndex = (int)handToFire;
        int otherHandIndex = 1 - handIndex;

        // if my hand isn't attached, or it is attached but in 'phantom' mode, fire the hand
        if (attachedHands[handIndex] == false) {
            // the hand is reaching for an available spot
            isReaching = true;

            RaycastHit hit;
            Vector3 rayOrigin = cameraTransform.position;
            Vector3 cameraDirection = GetCameraDirection();

            bool rayHit = Physics.Raycast(rayOrigin, cameraDirection, out hit, grabRange, climbingLayerMask);
            //Debug.DrawRay(rayOrigin, (rayOrigin + cameraDirection), Color.white); // debug ray

            Vector3 perpendicular_camera = Vector3.Cross(cameraDirection, Vector3.up);

            // Handle case where direction is Vector3.up or Vector3.down
            if (perpendicular_camera == Vector3.zero) {
                perpendicular_camera = Vector3.Cross(cameraDirection, Vector3.right);
            }

            Vector3 tangent_camera = Vector3.Cross(cameraDirection, perpendicular_camera).normalized;
            perpendicular_camera.Normalize();

            int raysFired = useRaycastRadius == true ? 0 : 8; // if useRaycastRadius is false, do not do more raycasts.
            while (rayHit != true && raysFired < 8) {
                float angle = raysFired * (360f / 8f) * Mathf.Deg2Rad;
                float xOffset = Mathf.Cos(angle) * raycastRadius;
                float yOffset = Mathf.Sin(angle) * raycastRadius;

                Vector3 newRayOrigin = rayOrigin + (perpendicular_camera * xOffset) + (tangent_camera * yOffset);

                // new raycast
                rayHit = Physics.Raycast(newRayOrigin, cameraDirection, out hit, grabRange, climbingLayerMask);
                //Debug.DrawRay(newRayOrigin, (newRayOrigin + cameraDirection), Color.red); // debug ray
                raysFired += 1;
            }
            
            // Does the ray intersect any objects excluding the player layer?
            if (rayHit == false || hit.distance > grabRange) {
                // didn't hit. anything. Player is reaching for straws they are going NOWHERE.

            } else {
                // player hit an object, check if its climbable

                GameObject hitObject = hit.transform.gameObject;
                bool objectIsClimbable = IsObjectClimbable(hitObject);

                if (objectIsClimbable == true) {
                    Vector3 hitPosition = hit.point;
                    Vector3 hitOrientation = hit.normal;
                    Quaternion hitRotation = Quaternion.LookRotation(hitOrientation);
                    AttachHand(handToFire, hitPosition, hitRotation);
                } else { 
                    // player is trying to climb on something not climbable
                }
            }
        }
    }

    // returns true if the object is climbable and false otherwise
    private bool IsObjectClimbable(GameObject obj) {
        // if it has a climbable layer, and is static, it is climbable
        bool staticTest = obj.isStatic;

        return staticTest == true;
    }
    #endregion

    #region Climbing Movement
    [Header("Climbing Movement Attributes")]
    // velocities, the actual climbing speed
    [Tooltip("How fast the player tries to go")]
    [SerializeField] private float climbingVelocityGain = 1.75f;
    // basically acceleration

    [Tooltip("Max velocity the player can go")]
    [SerializeField] private float maxClimbingVelocity = 2.5f;
    // if the player gains too much speed while climbing, lower max climb velocity

    // forces, how effective the climbing script is at actually achieving desired velocity.
    [Tooltip("How strong the climbing force is")]
    [SerializeField] private float forceGain = 12f;
    [Tooltip("Max force strength")]
    [SerializeField] private float maxClimbingForce = 25f;


    [Header("One-Handed Penalties")]
    [Tooltip("When one handed, apply this attribute to climb movement speed")]
    [SerializeField] [Range(0f, 1f)] private float oneHandedSpeedPenalty = 0.8f;

    [Tooltip("When one handed, apply this attribute to climb movement force strength")]
    [SerializeField] [Range(0f, 1f)] private float oneHandedForcePenalty = 0.6f;

    [Tooltip("When one handed, apply this attribute to lean magnitude")]
    [SerializeField] [Range(0f, 1f)] private float oneHandedLeanMagnitudePenalty = 0.9f;


    [Header("Climbing Movement - Overshoot Handling")]
    [Tooltip("if 1, player does not slow down to reach target. if 0, player will not overshoot target")]
    [SerializeField] [Range(0f, 1f)] private float velocityOvershoot = 0.4f;
    [Tooltip("If the player is below the given speed, velocity overshoot is treated as 0. (removes jitter when standing still)")]
    [SerializeField][Range(0f, 2f)] private float steadyStateSpeedCap = 0.5f;
    // moves the player towards the climbing target
    private void UpdateClimbingMovement() {
        Vector3 climbingTarget = GetLeanedClimbingTarget(); // where we want to go

        float speedPenalty = 1;
        float forcePenalty = 1;
        if (oneHandedPenalties == true) {
            int handsAttached = GetNumberOfHandsAttached();
            speedPenalty = handsAttached == 2 ? 1 : oneHandedSpeedPenalty;
            forcePenalty = handsAttached == 2 ? 1 : oneHandedForcePenalty;
        }

        Vector3 direction = climbingTarget - playerRigidbody.position;
        Vector3 targetVelocity = Vector3.ClampMagnitude(climbingVelocityGain * direction, maxClimbingVelocity) * speedPenalty;

        // difference between target and current velocity
        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        float usedVelocityOvershoot = (1 - velocityOvershoot);
        if (currentVelocity.magnitude < steadyStateSpeedCap) {
            usedVelocityOvershoot = 1;
        }

        Vector3 velocityDifference = targetVelocity - playerRigidbody.linearVelocity * usedVelocityOvershoot;

        // Calculate and apply force to correct the velocity
        Vector3 climbForce = Vector3.ClampMagnitude(forceGain * velocityDifference, maxClimbingForce) * forcePenalty;
        playerRigidbody.AddForce(climbForce);
    }
    #endregion

    #region Climbing Target Methods
    // gets player's target position away from the wall while climbing
    private Vector3 GetClimbingTarget() {
        Vector3 midpointDirection = GetClimbingNormalDirection();
        Vector3 midpointPosition = GetClimbingMidpoint();

        return midpointPosition + midpointDirection.normalized * handToPlayerDistance;
    }

    // takes GetClimbingTarget(), and applys a leaning effect.
    private Vector3 GetLeanedClimbingTarget() {
        Vector3 leanVector = GetLeanVector(); // where we want to lean

        // get the leaning magnitude
        float leanMagnitude = this.leaningMagnitude;
        
        if (oneHandedPenalties == true) {
            int handsAttached = GetNumberOfHandsAttached();
            leanMagnitude *= handsAttached == 2 ? 1 : oneHandedLeanMagnitudePenalty;
        }

        // create leaning vector * magnitude of lean
        leanVector = leanVector * leanMagnitude;

        // returns leaned position added to original climbing target
        return GetClimbingTarget() + leanVector;
    }

    // returns unit vector describing direction the player wants to lean
    private Vector3 GetLeanVector() {
        // lean input based on wasd input
        Vector3 leaningInputDirection_OnCamera = cameraTransform.TransformDirection(leaningInputDirection).normalized;

        // combine previous vector with 'reaching' direction
        Vector3 leanVector = (leaningInputDirection_OnCamera + leaningReachingDirection * reachingLeaningMagnitude).normalized;
        return leanVector;
    }

    // climbing point midpoint functions
    #region climbing midpoint methods
    // returns the midpoint of attached hands
    private Vector3 GetClimbingMidpoint() { 
        Vector3 midpointPosition = Vector3.zero;
        for (int i = 0; i < attachedHands.Length; i++) { 
            if (attachedHands[i] == true) {
                if (midpointPosition.magnitude == 0) {
                    midpointPosition = handTransforms[i].position;
                } else { 
                    midpointPosition = (midpointPosition + handTransforms[i].position) / 2f;
                }
            }
        }

        return midpointPosition;
    }
    // returns the combination vector of attached hands
    private Vector3 GetClimbingNormalDirection() { 
        Vector3 midpointNormal = Vector3.zero;
        for (int i = 0; i < attachedHands.Length; i++) { 
            if (attachedHands[i] == true) {
                midpointNormal += handTransforms[i].forward;
            }
        }

        return midpointNormal.normalized;
    }
    #endregion
    #endregion

    #region Wall Jumping
    [Header("Wall Jump Attributes")]
    [Tooltip("Strength to which the player walljumps")]
    [SerializeField] private float wallJumpMagnitude = 4f;

    [Tooltip("How many 'vertical' degrees are added onto the walljump. On a flat wall: 1 = 45 degrees, 0 = 0 deg, 2 = 67.5 deg")]
    [SerializeField] [Range(0f, 3f)] private float verticalWallJumpFactor = 1f;

    [Tooltip("When jumping off a wall, the player's is clamped to this value")]
    [SerializeField] private float maxWallJumpVelocity = 7f;


    [Header("Wall Jump Fatigue")]
    [Tooltip("How long it takes for the walljump to fully recover its power after jumping from wall and grabbing another")]
    [SerializeField] [Range(0f, 5f)] private float fatigueRecoveryTime = 1.5f;
    private bool walljumpFatigued = false; // set to true when jumping. set to false when grabbing a wall along with fatigue timer being set.

    // if negative, there is no fatigue. If positive or zero, that is the time the player has "regrabbed" a wall
    private float walljumpFatigueTimer = -5;


    // launched the player rigidbody away from the wall.
    private void WallJump() { 
        if (isHandAttached()) {
            // get jump direction
            // targetjumpdirection is where the player tries to go
            Vector3 targetJumpDirection = GetClimbingNormalDirection();

            // if true, walljump direction will be based on leaning direction
            if (walljumpUsesLeanDirection == true) {
                Vector3 leanVector = GetLeanVector();

                // if the player is not leaning, then use the wall's normal for jump direction
                if (leanVector.magnitude > 0f) {
                    targetJumpDirection = leanVector;
                }
            }

            // calculates fatigue
            float jumpFatigue = 1; // 1 is no fatigue, 0 is max fatigue
            if (enableWalljumpFatigue == true && walljumpFatigueTimer > 0) {
                jumpFatigue = Mathf.Min((Time.time - walljumpFatigueTimer) / fatigueRecoveryTime, 1);
            }
            walljumpFatigued = true;

            // calculates target jump direction with an added vertical component
            Vector3 jumpDirection = (Vector3.up * verticalWallJumpFactor + targetJumpDirection).normalized;
            float jumpMagnitude = wallJumpMagnitude;
            Vector3 jumpVector = jumpDirection * (jumpMagnitude * jumpFatigue);

            // detach all hands, stop player input
            ExitClimb();

            // jump that player! jump that player right now!
            // disables boosting via leaning back and forth prior to walljumping.
            if (disableWalljumpBoosting == true) {
                Vector3 currentVelocity = playerRigidbody.linearVelocity;
                Vector3 velocityDifference = jumpVector - playerRigidbody.linearVelocity;
                jumpVector = velocityDifference;
            }

            playerRigidbody.AddForce(jumpVector, ForceMode.Impulse);
            if (clampWalljumpVelocity == true) {
                playerRigidbody.linearVelocity = Vector3.ClampMagnitude(playerRigidbody.linearVelocity, maxWallJumpVelocity * jumpFatigue);
            }
        }
    }
    #endregion
}
