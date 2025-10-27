using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : Singleton<PlayerInput>
{
    private ClimbAction climbingScript;

    // input variable
    private PlayerInputActions inputActions = null;

    ////// publically readable variables for getting input ///////////
    /// NOTE: please use these
    // move and look vectors
    public Vector2 movementInput = Vector2.zero;
    public Vector2 lookInput = Vector2.zero;

    // these variables are true when the player is HOLDING sprint or crouch
    public bool sprintInput = false;
    public bool crouchInput = false;
    public bool jumpInput = false;
    
    public Action<InputAction.CallbackContext> OnJump = delegate { }; // jump event
    public event Action<InputAction.CallbackContext> OnAction = delegate { };

    ////////
    protected override void Awake()
    {
        base.Awake();
        inputActions = new PlayerInputActions();
        climbingScript = gameObject.GetComponent<ClimbAction>();
    }

    ////// when enabled, activate inputs
    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += InputOnMove;
        inputActions.Player.Move.canceled += InputOnMove; // stop moving when input is canceled
        inputActions.Player.Look.performed += InputOnLook;
        inputActions.Player.Look.canceled += InputOnLook; // stop looking when input is canceled

        inputActions.Player.Attack.performed += InputAttack;
        inputActions.Player.Interact.performed += InputInteract;

        inputActions.Player.Crouch.performed += InputCrouch;
        inputActions.Player.Crouch.canceled += InputCrouch;
        inputActions.Player.Jump.performed += InputJump;
        inputActions.Player.Jump.canceled += InputJump;
        inputActions.Player.Sprint.performed += InputSprint;
        inputActions.Player.Sprint.canceled += InputSprint;

        inputActions.Player.Climb.performed += InputClimb;
        inputActions.Player.ClimbLeft.performed += InputClimbLeft;
        inputActions.Player.ClimbLeft.canceled += InputClimbLeft;
        inputActions.Player.ClimbRight.performed += InputClimbRight;
        inputActions.Player.ClimbRight.canceled += InputClimbRight;

        // can be "performed" or "canceled"
        // performed = pressed down. canceled = released input
    }

    ////// when disabled, deactivate inputs
    private void OnDisable()
    {
        inputActions.Disable();

        inputActions.Player.Move.performed -= InputOnMove;
        inputActions.Player.Move.canceled -= InputOnMove;
        inputActions.Player.Look.performed -= InputOnLook;
        inputActions.Player.Look.canceled -= InputOnLook;

        inputActions.Player.Attack.performed -= InputAttack;
        inputActions.Player.Interact.performed -= InputInteract;

        inputActions.Player.Crouch.performed -= InputCrouch;
        inputActions.Player.Crouch.canceled -= InputCrouch;
        inputActions.Player.Jump.performed -= InputJump;
        inputActions.Player.Sprint.performed -= InputSprint;
        inputActions.Player.Sprint.canceled -= InputSprint;
    }

    // Added
    public void DebugToggleInput(bool enabled)
    {
        if (enabled) OnDisable();
        else OnEnable();
    }

    ////////////// input methods. Performed When inputing stuff ////////////
    private void InputOnMove(InputAction.CallbackContext callbackValue) {
        if (callbackValue.performed)
        {
            movementInput = callbackValue.ReadValue<Vector2>();
        }
        else if (callbackValue.canceled)
        {
            movementInput = Vector2.zero; // stop moving when input is canceled
        }
    }

    private void InputOnLook(InputAction.CallbackContext callbackValue) {
        if (callbackValue.performed)
        {
            lookInput = callbackValue.ReadValue<Vector2>();
        }
        else if (callbackValue.canceled)
        {
            lookInput = Vector2.zero; // stop looking when input is canceled
        }
    }


    //// jump, crouch, sprint inputs
    private void InputJump(InputAction.CallbackContext callbackValue) {
        // call something to jump (here)
        jumpInput = callbackValue.performed == true;
        
        OnJump?.Invoke(callbackValue);
    }
    private void InputCrouch(InputAction.CallbackContext callbackValue) {
        if (callbackValue.performed) { // player is holding down crouch
            crouchInput = true;
        } else if (callbackValue.canceled) { // player let go of crouch
            crouchInput = false;
        }
    }
    private void InputSprint(InputAction.CallbackContext callbackValue) {
        if (callbackValue.performed) { // player is holding down sprint
            sprintInput = true;
        } else if (callbackValue.canceled) { // player let go of sprint
            sprintInput = false;
        }
    }


    //// interact, attack inputs
    private void InputInteract(InputAction.CallbackContext callbackValue) {
        // call something to interact (here)
    }

    private void InputAttack(InputAction.CallbackContext callbackValue) {
        // call something to attack (here)

        OnAction?.Invoke(callbackValue);
    }

    // climbing
    private void InputClimb(InputAction.CallbackContext callbackValue)
    {
        // call something to attack (here)
        climbingScript.EnterClimbMode();
    }

    private void InputClimbLeft(InputAction.CallbackContext callbackValue)
    {
        // call climbing script to
        climbingScript.InputHand(callbackValue.performed, ClimbAction.Hand.LeftHand);
    }

    private void InputClimbRight(InputAction.CallbackContext callbackValue)
    {
        // call something to attack (here)
        climbingScript.InputHand(callbackValue.performed, ClimbAction.Hand.RightHand);
    }
}
