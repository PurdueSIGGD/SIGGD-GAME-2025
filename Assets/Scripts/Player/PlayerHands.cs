using UnityEngine;

/*
 * HOW TO USE IN SCRIPTING
 *
 * PlayerHands playerHandScript = PlayerHands.instance;  <-- this will reference the script.
 *
 * playerHandScript.PlayAction();  <-- this will play the currently loaded tool animation.
 *
 * playerHandScript.ToggleRightArm(bool toggle);  <-- toggles the right hand, which only carrys tools.
 * playerHandScript.ToggleLeftArm(bool toggle);  <-- toggles the left hand, which only carrys the radio.
 * playerHandScript.DisableArms(bool isDisabled); <-- use to disable/reenable arms publically for whatever reason.
 *
 * SetOverrideController(AnimatorOverrideController newController)  <-- sets the override controller to something new. A new animation set for tools.
 * SetOverrideController()  <-- reverts back to the original override controller.
 *
 * These arms will automatically hide itself when climbing.
 *
 * PlayerHands is the ONLY script that should set the hand animator's bools (IsVisible / LeftVisible / RightVisible).
 * Other scripts should go through ToggleLeftArm / ToggleRightArm / DisableArms so the state can be restored
 * after an override controller swap.
 */

/*
 * IMPORTANT NOTES:
 * The animation controller is already set up. If you are working on a new tool animation, create a animtaor override controller and
 * replace the "Right Hand Action" and "Right Hand Idle" animation.
 * When animating a tool, make sure the "IsActive" property is set to true for whatever tool you are animating with.
 * Make sure the tool is parented to the RightHandSocket transform.
 * Make sure, when animating, to have the RightHandSocket transform in some keyframe.
 */

public class PlayerHands : MonoBehaviour
{
    public static PlayerHands instance; // use to reference this script's object

    #region instance variables
    [Header("Debug")]
    [Tooltip("If true, at runtime, loop and play the current action animation")]
    [SerializeField] private bool DEBUG_TestActionAnimation = false;
    [Tooltip("If true, while running, config_show(right/left)hand will override hand showing. " +
        "Otherwise, only show the right hand when an animator besides the default one is loaded")]
    [SerializeField] private bool DEBUG_ManualShowHands = false;

    [Header("   == Manual Control Needed == ")]
    [Tooltip("If true, at runtime, show the left hand")]
    [SerializeField] private bool CONFIG_ShowLeftHand = false;
    [Tooltip("If true, at runtime, show the right hand")]
    [SerializeField] private bool CONFIG_ShowRightHand = false;

    [Header("Presets")]
    [Tooltip("The default override controller when calling SetOverrideController() with no arguments.")]
    [SerializeField] private AnimatorOverrideController defaultOverrideController;

    [Tooltip("The children of this socket will be disabled when changing the override controller")]
    [SerializeField] private Transform rightHandSocket;

    [Tooltip("These bones will have their LOCAL position and rotation reset when changing the override animator controller")]
    [SerializeField] private Transform[] armatureTransforms;
    private Vector3[] armaturePositions;    // local space (relative to parent bone / camera)
    private Quaternion[] armatureRotations; // local space

    private ClimbAction climbScript;
    private Animator handAnimator;
    private bool showHands = true; // if true, player hands will be visible. If false, hands will be offscreen.
    private bool handsDisabled = false; // disable hands if true, not as important as show hands.

    // last values written to the animator, so they can be restored after a controller swap
    // (assigning runtimeAnimatorController rebinds the animator and resets every parameter)
    private bool leftVisible = false;
    #endregion

    #region Start and Awake Methods
    private void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("ERROR! TWO MAIN CAMERA OBJECTS ARE IN GAME");
        }

        handAnimator = GetComponent<Animator>();

        // Cache the rest pose in LOCAL space. The arm is parented to the camera, so a world-space
        // pose would be wrong as soon as the player moved or turned.
        armaturePositions = new Vector3[armatureTransforms.Length];
        armatureRotations = new Quaternion[armatureTransforms.Length];

        for (int i = 0; i < armatureTransforms.Length; i++) {
            armaturePositions[i] = armatureTransforms[i].localPosition;
            armatureRotations[i] = armatureTransforms[i].localRotation;
        }
    }

    private void OnDestroy()
    {
        if (instance == this) {
            instance = null;
        }
    }

    void Start()
    {
        // retrieve essential objects
        UpdateClimbScript();

        // apply the default controller right away (not queued) so the first frame is correct
        ApplyOverrideController(defaultOverrideController);

        // show the hands
        ToggleLeftArm(CONFIG_ShowLeftHand);
        ToggleRightArm(DEBUG_ManualShowHands ? true : CONFIG_ShowRightHand);

        if (DEBUG_TestActionAnimation) {
            // this tests the play action
            Invoke("PlayAction", 3f);
        }
    }
    #endregion

    void Update() {
        // apply the most recent queued override controller, rate limited
        if (hasPendingController && Time.time - lastOverrideChange >= OverrideSwitchDelay) {
            hasPendingController = false;
            ApplyOverrideController(pendingController);
        }

        // don't show hands while climbing
        bool isClimbing = false;
        if (climbScript != null) {
            isClimbing = climbScript.IsClimbing();
        } else {
            UpdateClimbScript();
        }
        showHands = !isClimbing;

        // if hands are disabled, don't show hands.
        if (handsDisabled) {
            showHands = false;
        }

        // toggles arm visibility based on if it should be visible at all or not.
        ToggleArms(showHands);

        if (DEBUG_ManualShowHands) {
            ToggleRightArm(CONFIG_ShowRightHand);
            ToggleLeftArm(CONFIG_ShowLeftHand);
        }
    }


    /// <summary>
    /// this plays the "action" animation.
    /// </summary>
    public void PlayAction() {
        if (!showHands) {
            return;
        }
        handAnimator.SetTrigger("Action");

        // if debugging, this will loop the animation
        if (DEBUG_TestActionAnimation == true) {
            Invoke("PlayAction", 2.5f);
        }
    }

    #region Override Controller Setters
    [Header("Override Switching Limits")]
    [Tooltip("Min time between switching override controller. Will buffer to most recently set controller. If too low, switching quickly causes lag.")]
    [SerializeField] float OverrideSwitchDelay = 0.5f;
    private float lastOverrideChange = -100f;

    private AnimatorOverrideController pendingController;
    private bool hasPendingController = false;

    /// <summary>
    /// Queues a new override controller. Switching the controller too often causes frame drops, so the
    /// request is applied in Update, at most once every OverrideSwitchDelay seconds. The most recent
    /// request always wins.
    /// </summary>
    public void SetOverrideController(AnimatorOverrideController newController) {
        pendingController = newController;
        hasPendingController = true;
    }

    public void SetOverrideController()
    {
        SetOverrideController(defaultOverrideController);
    }

    /// <summary>
    /// Actually swaps the controller. Only called from Start and Update.
    /// </summary>
    private void ApplyOverrideController(AnimatorOverrideController newController) {
        if (newController == null || newController == handAnimator.runtimeAnimatorController) {
            return;
        }

        lastOverrideChange = Time.time;

        DisableHandSocketChildren();

        // Reset the bones BEFORE swapping. Assigning runtimeAnimatorController rebinds the animator,
        // and with Write Defaults on, the rebind records the current pose as the default pose.
        // Resetting first means the default is the clean rest pose, not a mid-swing one.
        RefreshArmTransforms();
        handAnimator.runtimeAnimatorController = newController;

        // the rebind reset every parameter, so restore them
        handAnimator.SetBool("IsVisible", showHands);
        ToggleLeftArm(leftVisible);
        if (DEBUG_ManualShowHands) {
            ToggleRightArm(CONFIG_ShowRightHand);
        } else {
            ToggleRightArm(newController != defaultOverrideController);
        }
    }
    #endregion

    #region Animator Cleanup Methods
    /// <summary>
    /// sets the visibility of every child in right hand socket to false. This is done to ensure
    /// unused props aren't staying visible when switching controllers.
    /// </summary>
    private void DisableHandSocketChildren() {
        foreach (Transform child in rightHandSocket) {
            child.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// resets the armature bones to their original LOCAL position and rotation
    /// </summary>
    private void RefreshArmTransforms() {
        for (int i = 0; i < armatureTransforms.Length; i++) {
            armatureTransforms[i].localPosition = armaturePositions[i];
            armatureTransforms[i].localRotation = armatureRotations[i];
        }
    }

    // the climb script lives on the player, which may not exist yet when this object wakes up
    private void UpdateClimbScript() {
        if (PlayerID.Instance != null) {
            climbScript = PlayerID.Instance.gameObject.GetComponent<ClimbAction>();
        }
    }
    #endregion

    #region Arm Toggles

    /// <summary>
    /// toggles the left arm's visibility. this is for the walkie talkie thing.
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleLeftArm(bool toggle) {
        leftVisible = toggle;
        handAnimator.SetBool("LeftVisible", toggle);
    }


    /// <summary>
    /// toggles the right arm's visibility. this is for tools
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleRightArm(bool toggle) {
        handAnimator.SetBool("RightVisible", toggle);
    }


    /// <summary>
    /// public use method for disabling hands for whatever reason.
    /// </summary>
    /// <param name="armsDisabled"></param>
    public void DisableArms(bool armsDisabled) {
        handsDisabled = armsDisabled;
    }


    // script use only
    /// <summary>
    /// toggles visibility of both arms without removing the individual 'is visible' booleans
    /// </summary>
    /// <param name="toggle"></param>
    private void ToggleArms(bool toggle)
    {
        handAnimator.SetBool("IsVisible", toggle);
    }
    #endregion
}