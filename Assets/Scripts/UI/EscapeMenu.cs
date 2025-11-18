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
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowEscapeMenu(!canvas.enabled);
        }
    }

    public void ShowEscapeMenu(bool enable)
    {
        if (enable)
        {
            ShowInventory(false);
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
            ShowInventory(false); // close inventory along with escape menu
        }
        PlayerInput.Instance.DebugToggleInput(enable);
        //Cursor.visible = canvas.enabled = isEnabled = enable;
    }

    public void LoadMainMenu()
    {
        SaveManager.Instance.Save();
        SceneManager.LoadScene("Main Menu");
    }

    public void ShowInventory(bool enabled)
    {
        if (Inventory.Instance)
        {
            Inventory.Instance.ShowInventory(enabled);
            if (enabled) {
                canvas.enabled = false; // hide escape menu when displaying inventory
            }
        }   
    }
}