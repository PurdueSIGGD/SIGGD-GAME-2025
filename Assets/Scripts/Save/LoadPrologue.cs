using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPrologue : MonoBehaviour
{
    [SerializeField] string mainMenuName;
    [SerializeField] string prologueName;

    [SerializeField] SmiteSelf nukeSaveFunction;

    void Start()
    {
        var progress = SaveManager.Instance?.Get<GameProgressModule>();
        if (progress != null && progress.HasCompletedPrologue)
        {
            SceneManager.LoadScene(mainMenuName);
        }
        else
        {
            nukeSaveFunction.YeetSaves();
            SceneManager.LoadScene(prologueName);
        }
    }
}
