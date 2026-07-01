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
}
