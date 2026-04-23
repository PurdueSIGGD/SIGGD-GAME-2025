using UnityEngine;

public class PlayerInApexRange : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("the trigger is entered");
        if (collider.CompareTag("Apex"))
        {
            Debug.Log("crossfading to apex");
            MusicManager.Instance.CrossFadeMusic("ApexLurk", 7.5f);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Apex"))
        {
            
            MusicManager.Instance.CrossFadeMusic(MusicSceneLink.Instance.sceneMusicKey, 7.5f);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        Debug.Log("the trigger is exited");
        if (collider.CompareTag("Apex"))
        {
            Debug.Log("crossfading to normal music again");
            MusicManager.Instance.CrossFadeMusic(MusicSceneLink.Instance.sceneMusicKey, 5f);
        }
    }
}
