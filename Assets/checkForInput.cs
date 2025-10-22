using UnityEngine;

public class checkForInput : MonoBehaviour
{
    public GameObject buttonObject;
    public GameObject fadedScreen;
    private bool isEnabled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Start()
    {
        buttonObject = GameObject.FindWithTag("QuitButton");
        fadedScreen = GameObject.FindWithTag("FadedScreen");
        buttonObject.SetActive(false);
        fadedScreen.SetActive(false);
        isEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isEnabled = !isEnabled;
            buttonObject.SetActive(isEnabled);
            fadedScreen.SetActive(isEnabled);
        }
    }
}
