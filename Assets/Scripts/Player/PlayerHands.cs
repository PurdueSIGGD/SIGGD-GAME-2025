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
 * SetOverrideController()  <-- reverts back to the original override controller.\
 * 
 * These arms will automatically hide itself when climbing.
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

    [Header("Config")]
    [Tooltip("If true, at runtime, show the left hand")]
    [SerializeField] private bool CONFIG_ShowLeftHand = false;
    [Tooltip("If true, at runtime, show the right hand")]
    [SerializeField] private bool CONFIG_ShowRightHand = false;
    [Tooltip("If true, override the previous config, and make right hand visible if there is a non-default override controller")]
    [SerializeField] private bool CONFIG_ShowRightHand_OnlyWithTool = true;

    [Header("Presets")]
    [Tooltip("The default override controller when calling SetOverrideController() with no arguments.")]
    [SerializeField] private AnimatorOverrideController defaultOverrideController;

    [Tooltip("The children of this socket will be disabled when changing the override controller")]
    [SerializeField] private Transform rightHandSocket;

    [Tooltip("These bones will have their position and rotation reset when changing the override animator controller")]
    [SerializeField] private Transform[] armatureTransforms;
    private Vector3[] armaturePositions;
    private Quaternion[] armatureRotations;

    private ClimbAction climbScript;
    private Animator handAnimator;
    private bool showHands = true; // if true, player hands will be visible. If false, hands will be offscreen.
    private bool handsDisabled = false; // disable hands if true, not as important as show hands.
    #endregion

    #region Start and Awake Methods
    private void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("ERROR! TWO MAIN CAMERA OBJECTS ARE IN GAME");
        }

        armaturePositions = new Vector3[armatureTransforms.Length];
        armatureRotations = new Quaternion[armatureTransforms.Length];

        for (int i = 0; i < armatureTransforms.Length; i++) {
            armaturePositions[i] = armatureTransforms[i].position;
            armatureRotations[i] = armatureTransforms[i].rotation;
        }
    }

    void Start()
    {
        SetOverrideController();

        // retrieve essential objects
        UpdateClimbScript();
        UpdateHandAnimator();

        // show the hands
        ToggleLeftArm(CONFIG_ShowLeftHand);
        ToggleRightArm(CONFIG_ShowRightHand_OnlyWithTool ? false : CONFIG_ShowRightHand);

        if (DEBUG_TestActionAnimation) {
            // this tests the play action
            Invoke("PlayAction", 1f);
        }
    }
    #endregion

    void Update() {
        // don't show hands while climbing
        bool isClimbing = false;
        if (climbScript != null) {
            isClimbing = climbScript.IsClimbing();
        } else {
            UpdateClimbScript();
        }
        showHands = !isClimbing;

        // if disabled, don't show hands.
        if (handsDisabled) {
            showHands = false;
        }

        // toggles arm visibility based on if it should be visible at all or not.
        ToggleArms(showHands);
    }


    /// <summary>
    /// this plays the "action" animation.
    /// </summary>
    public void PlayAction() {
        if (CONFIG_ShowRightHand) {
            return;
        }

        handAnimator.SetTrigger("Action");

        // if debugging, this will loop the animation
        if (DEBUG_TestActionAnimation) {
            Invoke("PlayAction", 2.5f);
        }
    }

    #region Override Controller Setters
    public void SetOverrideController(AnimatorOverrideController newController) {
        // need to create a new controller to not modify the original asset
        UpdateHandAnimator();
        handAnimator.runtimeAnimatorController = new AnimatorOverrideController(newController);

        UpdateClimbScript();
        UpdateHandAnimator();
        DisableHandSocketChildren();
        RefreshArmTransforms();

        ToggleRightArm(newController != defaultOverrideController && CONFIG_ShowRightHand_OnlyWithTool);
    }

    public void SetOverrideController()
    {
        SetOverrideController(defaultOverrideController);
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
    /// resets the armature bones to their original position and rotation
    /// </summary>
    private void RefreshArmTransforms() { 
        for (int i = 0; i < armatureTransforms.Length; i++) {
            armatureTransforms[i].position = armaturePositions[i];
            armatureTransforms[i].rotation = armatureRotations[i];
        }
    }

    // updates references that get de reff'd for some reason (idk why its evil)
    private void UpdateClimbScript() {
        climbScript = PlayerID.Instance.gameObject.GetComponent<ClimbAction>();
    }
    private void UpdateHandAnimator() {
        handAnimator = GetComponent<Animator>();
    }
    #endregion

    #region Arm Toggles

    /// <summary>
    /// toggles the left arm's visibility. this is for the walkie talkie thing.
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleLeftArm(bool toggle) {
        UpdateHandAnimator();
        handAnimator.SetBool("LeftVisible", toggle);
    }


    /// <summary>
    /// toggles the right arm's visibility. this is for tools
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleRightArm(bool toggle) {
        UpdateHandAnimator();
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
        UpdateHandAnimator();
        handAnimator.SetBool("IsVisible", toggle);
    }
    #endregion
}
