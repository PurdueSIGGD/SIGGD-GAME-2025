using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenInputHandler : MonoBehaviour
{
    public string mainSceneName;
    public GameObject loadingPanel;
    [SerializeField] OverrideStartMusic titleMusic;

    // AsyncOperation loadScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // loadScene = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        // loadScene.allowSceneActivation = false;
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
        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
        // not awaiting this because we don't need to
        // _ = SceneManager.UnloadSceneAsync("Assets/UI/titlescreen.unity");
    }
}
