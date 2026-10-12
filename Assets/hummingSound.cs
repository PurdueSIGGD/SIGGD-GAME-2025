using UnityEngine;

public class HummingSound : ExternalEventTriggerer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void TriggerExternalEvent()
    {
        Debug.Log("Triggered Music Time");
        //MusicManager.Instance.CrossFadeMusic("forestambiencefirst", 0.1f);
        //PlayerID.Instance.GetComponent<PlayerHummingSound>().StopHumming();
        PlayerID.Instance.GetComponent<PlayerMovement>().MakeFootstepsNormal();
    }

    public override void RestoreExternalEvent()
    {
        StartCoroutine(RestoreFootsteps());
    }

    // footsteps were already switched before the scene reloaded, keep them switched
    private System.Collections.IEnumerator RestoreFootsteps()
    {
        // wait a frame so this runs after PlayerMovement.Start has picked the scene's default footsteps
        yield return null;
        if (PlayerID.Instance != null) PlayerID.Instance.GetComponent<PlayerMovement>().MakeFootstepsNormal();
    }
}
