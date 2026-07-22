using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;

public class SettingsMenu : Singleton<SettingsMenu>
{
    public GameObject previousView = null;

    public GameObject initialPanel;

    private Canvas canvas;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GetComponent<Canvas>();
        Show(false);

        // This is used to load the currently used input overrides
        //if (SaveManager.Instance)
        //    SaveManager.Instance.Load();

        OpenPanel(initialPanel);
    }

    public void Show(bool enabled)
    {
        // Will doing this be too slow?
        if (!enabled)
        {
            // Closing settings — persist volume/rebind changes.
            SaveManager.Instance?.SaveSettings();
        }

        if (previousView)
        {
            previousView.SetActive(!enabled);
        }
        else if (!enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (PlayerInput.Instance)
                PlayerInput.Instance.DebugToggleInput(false);
        }

        canvas.enabled = enabled;
    }

    public void ResetPlayerInputs()
    {
        SaveManager.Instance?.Get<InputOverrideModule>()?.ResetPlayerInputs();
        foreach (var rebinder in FindObjectsByType<InputRebinder>(FindObjectsSortMode.None))
        {
            rebinder.Restart();
        }
    }

    public void ResetAudioLevels()
    {
        SaveManager.Instance?.Get<AudioLevelsModule>()?.ResetAudioLevels();
        foreach (var vcaController in FindObjectsByType<ControllerVca>(FindObjectsSortMode.None))
        {
            vcaController.Restart();
        }
    }

    // This goes back to main menu
    public void Close()
    {
        Show(false);
    }

    public void OpenPanel(GameObject panel)
    {
        Transform parent = panel.transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (child == panel)
            {
                child.SetActive(true);
            }
            else
            {
                child.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
