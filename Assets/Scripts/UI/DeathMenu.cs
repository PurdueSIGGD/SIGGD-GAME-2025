using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;

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
                    SaveManager.Instance.Save();
                    SceneManager.LoadScene("Main Menu");
                    ShowDeathMenu(false);
                });
    }

    public void ShowDeathMenu(bool enable)
    {
        if (enable)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            canvas.enabled = true;
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