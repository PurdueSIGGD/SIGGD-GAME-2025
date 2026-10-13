using UnityEngine;

public class PlayerInApexRange : MonoBehaviour
{
    // The final boss scene runs its own music (see NuclearReactorInteract), so the lurk music stays out of it
    private static bool BossSceneMusic => NuclearReactorInteract.Instance != null;

    private void OnTriggerEnter(Collider collider)
    {
        //Debug.Log("the trigger is entered");
        if (collider.CompareTag("Apex"))
        {
            if (BossSceneMusic) return;

            //Debug.Log("crossfading to apex");
            MusicManager.Instance.CrossFadeMusic("ApexLurk", 2f);
            MusicManager.Instance.curMusicState = MusicManager.MusicCycleState.ApexLurk;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        //Debug.Log("the trigger is exited");
        if (collider.CompareTag("Apex"))
        {
            if (BossSceneMusic || MusicSceneLink.Instance == null) return;

            //Debug.Log("crossfading to normal music again");

            MusicManager.Instance.CrossFadeMusic(MusicSceneLink.Instance.sceneMusicKey, 2f);
            MusicManager.Instance.curMusicState = MusicManager.MusicCycleState.Playing;
        }
    }
}
