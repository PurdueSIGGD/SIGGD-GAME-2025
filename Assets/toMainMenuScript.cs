using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class toMainMenuScript : MonoBehaviour
{
    private Button quitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject buttonObj = GameObject.FindWithTag("QuitButton");
        if (buttonObj != null)
        {
            quitButton = buttonObj.GetComponent<Button>();
            quitButton.onClick.AddListener(loadMainMenu);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void loadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
