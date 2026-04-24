using UnityEngine;

public class PlayerInApexRange : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("the trigger is entered");
        if (collider.CompareTag("Apex"))
        {
            Debug.Log("crossfading to apex");
            MusicManager.Instance.CrossFadeMusic("ApexLurk", 2f);
            MusicManager.Instance.curMusicState = MusicManager.MusicCycleState.ApexLurk;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        Debug.Log("the trigger is exited");
        if (collider.CompareTag("Apex"))
        {
            Debug.Log("crossfading to normal music again");

            MusicManager.Instance.CrossFadeMusic("ForestAmbianceAfterFirstApex", 2f);
            MusicManager.Instance.curMusicState = MusicManager.MusicCycleState.Playing;
        }
    }
}
