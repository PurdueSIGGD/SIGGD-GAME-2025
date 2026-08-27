using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowSettingsMenu()
    {
        if (SettingsMenu.Instance) SettingsMenu.Instance.Show(true);
        // gameObject.SetActive(false);
    }

    public void ShowMainMenu()
    {
        if (SettingsMenu.Instance) SettingsMenu.Instance.Show(false);
        // gameObject.SetActive(true);
    }
}
