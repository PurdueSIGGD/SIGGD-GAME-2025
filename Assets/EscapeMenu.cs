using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    private Button quitButton;
    public GameObject buttonObj;
    public GameObject buttonObject;
    public GameObject fadedScreen;
    private bool isEnabled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (buttonObj != null)
        {
            quitButton = buttonObj.GetComponent<Button>();
            quitButton.onClick.AddListener(loadMainMenu);
        }
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
    public void loadMainMenu()
    {
      
        SceneManager.LoadScene("Main Menu");
    }
}