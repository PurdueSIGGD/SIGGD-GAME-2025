using System.Collections;
using UnityEngine;

public class PlayPrologueCapsuleOpen : MonoBehaviour
{

    private static readonly string capsuleOpenSound = "SpaceCapsuleOpen";
    IEnumerator Start()
    {
        while (!FMODEvents.Instance.Initialized)
        {
            yield return null;
        }
        AudioManager.Instance.PlayOneShotNoAsync(capsuleOpenSound, PlayerID.Instance.gameObject.transform.position);
    }
}
