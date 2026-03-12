using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    public MainMenuManager mainMenuManager;
    public GameObject initialPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // This is used to load the currently used input overrides
        SaveManager.Instance.Load();

        OpenPanel(initialPanel);
    }

    public void ResetPlayerInputs()
    {
        SaveManager.Instance.inputOverrideSaveModule.ResetPlayerInputs();
        foreach (var rebinder in FindObjectsByType<InputRebinder>(FindObjectsSortMode.None))
        {
            rebinder.Restart();
        }
    }

    // This goes back to main menu
    public void GoBack()
    {
        mainMenuManager.ShowMainMenu();
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
