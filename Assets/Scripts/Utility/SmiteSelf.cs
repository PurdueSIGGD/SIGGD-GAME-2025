using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SmiteSelf : MonoBehaviour
{
    private static string mainDirectory;

    public void EverytimeThisIsCalledMeGetLittleSadder()
    {
        YeetSaves();
        SceneManager.LoadScene("Main Menu");
    }

    public void YeetSaves()
    {
        mainDirectory = Path.Combine(Application.persistentDataPath, "Data");

        if (Directory.Exists(mainDirectory))
        {
            Directory.Delete(mainDirectory, true);
        }

        // quest progress also lives in memory for the whole session, so it has to be wiped with the files
        QuestManager.Instance.ResetAll();
    }
}
