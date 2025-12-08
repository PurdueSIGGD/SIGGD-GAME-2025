using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    [SerializeField] string sceneID;

    public void OnSwitch()
    {
        SceneManager.LoadScene(sceneID);
    }
}
