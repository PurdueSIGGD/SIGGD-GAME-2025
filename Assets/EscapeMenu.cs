using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    private Button quitButton;
    public GameObject buttonObj;
    public GameObject buttonObject;
    public GameObject fadedScreen;
    public GameObject inventoryButton;
    public Canvas inventoryCanvas;
    [SerializeField] private Inventory inventory;
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
        inventoryButton.SetActive(false);
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
            inventoryButton.SetActive(isEnabled);
            inventoryCanvas.enabled = false;
        }
        if (inventoryCanvas.enabled)
        {
            isEnabled = false;
            buttonObject.SetActive(false);
            fadedScreen.SetActive(false);
            inventoryButton.SetActive(false);
        }
    }
    public void loadMainMenu()
    {
        
        SceneManager.LoadScene("Main Menu");
    }
    public void openInventoryMenu()
    {

        //inventory.ShowInventory(!inventory.inventoryEnabled);
        inventoryCanvas.enabled = true;
        isEnabled = false;
        buttonObject.SetActive(false);
        fadedScreen.SetActive(false);
        inventoryButton.SetActive(false);
    }
    
}