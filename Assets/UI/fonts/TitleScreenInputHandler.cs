using UnityEngine;
using System;

public class TitleScreenInputHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuitTheGame()
    {
        Debug.Log("I AM QUITTING NOW...");
        Application.Quit();
    }
}
