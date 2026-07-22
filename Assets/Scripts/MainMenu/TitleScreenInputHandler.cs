using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenInputHandler : MonoBehaviour
{
    public string defaultSceneName;
    public GameObject loadingPanel;
    public GameObject settingsPanel;
    [SerializeField] OverrideStartMusic titleMusic;

    // AsyncOperation loadScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingPanel.SetActive(false);
        // loadScene = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        // loadScene.allowSceneActivation = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        // loadScene.allowSceneActivation = true;
        // await loadScene; // Make sure we've actually loaded the scene at this point
        loadingPanel.SetActive(true);
        titleMusic.StopActiveMusic();
        string sceneName = SaveManager.Instance?.Get<LastSceneModule>()?.SceneName ?? string.Empty;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Save manager has no saved scene. Loading default scene: " + defaultSceneName);
            SceneManager.LoadScene(defaultSceneName, LoadSceneMode.Single);
        }
        else {
            Debug.Log("Loading " + sceneName + " from scene save");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        
        //Debug.Log("Loading save on start");
        //SaveManager.Instance.Load();
        // not awaiting this because we don't need to
        // _ = SceneManager.UnloadSceneAsync("Assets/UI/titlescreen.unity");
    }
    
    public void LoadCredits()
    {
        SceneManager.LoadScene("Credtis", LoadSceneMode.Single);
    }

    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }


}
