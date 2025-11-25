using UnityEngine;
using UnityEngine.SceneManagement;

public class MapRawImage : MonoBehaviour
{
    public string mapSceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.LoadScene(mapSceneName, LoadSceneMode.Additive);
        Debug.Log("MAP SCENE LOADED!!!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
