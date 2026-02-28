using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class InputRebinder : MonoBehaviour
{
    public InputActionReference actionRef;
    public int bindingIndex;

    TMP_Text text;
    Button button;

    // Am I listening for input?
    bool listening = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        text = transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        text.text = GetActionString();

        button.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnClick()
    {
        listening = true;
        text.text = "Listening...";
        // button.interactable = false;

        actionRef.action.Disable();
        actionRef.action.PerformInteractiveRebinding(bindingIndex)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                Debug.Log("WEFOPIJEFWPOIEWJFPOIEWFJPWEFOIJWEFP");
                op.Dispose();
                listening = false;
                text.text = GetActionString();
                button.interactable = true;

                actionRef.action.Enable();
            }).Start();
    }

    string GetActionString()
    {
        return InputControlPath.ToHumanReadableString(
            actionRef.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    void InputHandler(InputEventPtr ptr, InputDevice device)
    {
        if (device is not Keyboard)
            return;
        
        ptr.EnumerateChangedControls();
    }
}
