using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPrologue : MonoBehaviour
{
    [SerializeField] string mainMenuName;
    [SerializeField] string prologueName;

    [SerializeField] SmiteSelf nukeSaveFunction;

    void Start()
    {
        if (SaveManager.Instance.gameProgressModule.saveData.hasCompletedPrologue)
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
