using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputOverrideSaveModule : ISaveModule
{
    private readonly string savePath = $"{FileManager.savesDirectory}/playerInput.json";
    private InputActionAsset playerAsset;

    public InputOverrideSaveModule()
    {
        foreach (var asset in Resources.FindObjectsOfTypeAll<InputActionAsset>())
        {
            if (asset.name == "PlayerInputActions")
                playerAsset = asset;
        }
    }

    public void ResetPlayerInputs()
    {
        string json = "{}";
        playerAsset.LoadBindingOverridesFromJson(json);
        serialize();
        deserialize();
    }

    public bool serialize()
    {
        string json = playerAsset.SaveBindingOverridesAsJson();
        FileManager.Instance.WriteFile(savePath, Encoding.UTF8.GetBytes(json));
        return true;
    }
    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;
        string json = Encoding.UTF8.GetString(FileManager.Instance.ReadFile(savePath));
        playerAsset.LoadBindingOverridesFromJson(json);

        if (PlayerInput.Instance != null)
        {
            PlayerInput.Instance.LoadBindingOverrides(json);
        }

        return true;
    }

    // private static ArrayList overrides = new();
    // private static Dictionary<InputActionReference, string> overrides = new();

    // public static void FindAsset()
    // {
    //     Resources.Load<InputActionAsset>("Assets/Scripts/Input/PlayerInputActions.inputactions");
    // }

    // public static void ApplyAssetToAsset(InputActionAsset from, InputActionAsset to)
    // {

    // }

    // public static void SaveOverride(InputActionReference actionRef)
    // {
    //     if (overrides.ContainsKey(actionRef))
    //     {
    //         // Debug.Log("Exists, removing...");
    //         overrides.Remove(actionRef);
    //     }
    //     overrides.Add(actionRef, actionRef.action.SaveBindingOverridesAsJson());
    // }

    // public static void ApplyOverrides(IInputActionCollection2 inputActions)
    // {
    //     foreach (InputActionReference refer in overrides.Keys)
    //     {
    //         Debug.Log($"Applying input action override for {refer.name}");
    //         inputActions.LoadBindingOverridesFromJson(overrides[refer]);
    //     }
    // }
}
