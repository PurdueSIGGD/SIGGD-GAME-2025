using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenInputHandler : MonoBehaviour
{
    public string mainSceneName;

    // AsyncOperation loadScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // loadScene = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        // loadScene.allowSceneActivation = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void QuitGame()
    {
        Debug.Log("I AM QUITTING NOW...");
        Application.Quit();
    }

    public async void StartGame()
    {
        Debug.Log("STARTING");

        // loadScene.allowSceneActivation = true;
        // await loadScene; // Make sure we've actually loaded the scene at this point
        Debug.Log("HIIII");

        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);

        // not awaiting this because we don't need to
        // _ = SceneManager.UnloadSceneAsync("Assets/UI/titlescreen.unity");
    }
}
