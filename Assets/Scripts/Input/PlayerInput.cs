using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : Singleton<PlayerInput>
{
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
    
    public Action<InputAction.CallbackContext> OnJump = delegate { }; // jump event

    ////////
    protected override void Awake()
    {
        base.Awake();
        inputActions = new PlayerInputActions();
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
        inputActions.Player.Sprint.performed += InputSprint;
        inputActions.Player.Sprint.canceled += InputSprint;

        // can be "performed" or "canceled"
        // performed = pressed down. canceled = released input
    }

    ////// when disabled, deactivate inputs
    private void OnDisable() {
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
    }
}
