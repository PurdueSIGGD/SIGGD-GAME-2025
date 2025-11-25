using System;
using UnityEngine;

public class SwapScene : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Pranav's Test Scene 2");
        }
        else if (Input.GetKeyDown(KeyCode.Comma))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Pranav's Test Scene");
        }
    }
}