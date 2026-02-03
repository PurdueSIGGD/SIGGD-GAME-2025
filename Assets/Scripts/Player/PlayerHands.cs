using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    private bool showHands = true; // if true, player hands will be visible. If false, hands will be offscreen.
    private bool handsDisabled = false; // disable hands if true, not as important as show hands.

    [Header("Debug")]
    [Tooltip("If true, at runtime, loop and play the current action animation")]
    [SerializeField] private bool DEBUG_TestActionAnimation = false;

    [Header("Config")]
    [Tooltip("If true, at runtime, show the left hand")]
    [SerializeField] private bool CONFIG_ShowLeftHand = false;
    [Tooltip("If true, at runtime, show the right hand")]
    [SerializeField] private bool CONFIG_ShowRightHand = false;

    [Header("Presets")]
    [SerializeField] private AnimatorOverrideController defaultOverrideController;

    private ClimbAction climbScript;
    private Animator handAnimator;


    void Start()
    {
        SetOverrideController();

        // retrieve essential objects
        UpdateClimbScript();
        UpdateHandAnimator();

        // show the hands
        ToggleLeftArm(CONFIG_ShowLeftHand);
        ToggleRightArm(CONFIG_ShowRightHand);

        if (DEBUG_TestActionAnimation) {
            // this tests the play action
            Invoke("PlayAction", 1f);
        }
    }

    // updates references that get de reff'd for some reason (idk why its evil)
    private void UpdateClimbScript() {
        climbScript = PlayerID.Instance.gameObject.GetComponent<ClimbAction>();
    }
    private void UpdateHandAnimator() {
        handAnimator = GetComponent<Animator>();
    }
    
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


    public void SetOverrideController(AnimatorOverrideController newController) {
        // need to create a new controller to not modify the original asset
        UpdateHandAnimator();
        handAnimator.runtimeAnimatorController = new AnimatorOverrideController(newController);

        UpdateClimbScript();
        UpdateHandAnimator();
    }

    public void SetOverrideController() { 
        SetOverrideController(defaultOverrideController);
    }


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
}
