using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipCutSceneUtil : MonoBehaviour
{
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Cutscene skipped!");
            SceneManager.LoadScene("ShipScene");
        }
    }
#endif
}
