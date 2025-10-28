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
        inventoryButton.onClick.AddListener(ShowInventory);
        canvas.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowEscapeMenu(!isEnabled);
        }
    }

    public void ShowEscapeMenu(bool enable)
    {
        if (enable)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        Cursor.visible = canvas.enabled = isEnabled = enable;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void ShowInventory()
    {
        if (Inventory.Instance)
        {
            ShowEscapeMenu(false);
            Inventory.Instance.ShowInventory(true);
        }   
    }
}