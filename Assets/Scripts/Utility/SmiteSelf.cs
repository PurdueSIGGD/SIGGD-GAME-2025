using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SmiteSelf : MonoBehaviour
{
    private static string mainDirectory;

    public void EverytimeThisIsCalledMeGetLittleSadder()
    {
        mainDirectory = Path.Combine(Application.persistentDataPath, "Data");

        if (Directory.Exists(mainDirectory))
        {
            Directory.Delete(mainDirectory, true);
        }
        SceneManager.LoadScene("Main Menu");
    }
}
