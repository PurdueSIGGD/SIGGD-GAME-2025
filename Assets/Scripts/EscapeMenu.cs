using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private Button quitButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Canvas canvas;
    private bool isEnabled;

    void Start()
    {
        quitButton.onClick.AddListener(LoadMainMenu);
        inventoryButton.onClick.AddListener(() => {
            ShowInventory(true);
        });
        canvas.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("H pressed");
            if (!Inventory.Instance.isEnabled())
            { // only toggle escape menu if inventory is not currently enabled
                ShowEscapeMenu(!isEnabled);
            }
            ShowInventory(false); // always hide inventory when h is pressed
        }
    }

    public void ShowEscapeMenu(bool enable)
    {
        if (enable)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            canvas.enabled = true;
            isEnabled = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canvas.enabled = false;
            isEnabled = false;
        }
        //Cursor.visible = canvas.enabled = isEnabled = enable;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void ShowInventory(bool enabled)
    {
        if (Inventory.Instance)
        {
            Inventory.Instance.ShowInventory(enabled);
            if (enabled) ShowEscapeMenu(false); // hide escape menu when displaying inventory
        }   
    }
}