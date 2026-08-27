using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
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
