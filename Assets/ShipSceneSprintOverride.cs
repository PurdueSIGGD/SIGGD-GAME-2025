using System.Collections;
using UnityEngine;

public class ShipSceneSprintOverride : MonoBehaviour
{
    [SerializeField] private PlayerStamina stamina;

    void Start()
    {
        StartCoroutine(WaitToDisableSprint());

        IEnumerator WaitToDisableSprint()
        {
            yield return null;
            stamina.DisableSprint();
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Ship skipped!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("NathanA0Scene");
        }
    }
#endif
}
