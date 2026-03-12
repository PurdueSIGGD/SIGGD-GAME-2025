using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public SettingsMenuManager settingsMenuManager;
    private GameObject settingsCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        settingsCanvas = settingsMenuManager.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowSettingsMenu()
    {
        gameObject.SetActive(false);
        settingsCanvas.SetActive(true);
    }

    public void ShowMainMenu()
    {
        settingsCanvas.SetActive(false);
        gameObject.SetActive(true);
    }
}
