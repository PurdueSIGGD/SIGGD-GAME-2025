using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject escapeMenu;

    private ManageRespawn respawnManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas.enabled = false;
        if (!respawnManager) respawnManager = PlayerID.Instance.GetComponent<ManageRespawn>();
        respawnButton.onClick.AddListener(() =>
        {
            respawnManager.RespawnPlayer();
            ShowDeathMenu(false);
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            // Respawn player and immediately switch to main menu
            respawnManager.RespawnPlayer();
            ShowDeathMenu(false);
            Debug.Log("Going to main menu");
            SaveManager.Instance.Save();
            SceneManager.LoadScene("Main Menu");
            Debug.Log($"Current Scene: {SceneManager.GetActiveScene()}");
        });
    }

    public void ShowDeathMenu(bool enable)
    {
        escapeMenu.GetComponent<EscapeMenu>().ShowEscapeMenu(false);
        Debug.Log("Show death menu: " + enable);
        if (enable)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            canvas.enabled = true;
            ObjectPlacer.Instance.ExitPlacementMode();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canvas.enabled = false;

        }
        PlayerInput.Instance.DebugToggleInput(enable);
    }
}