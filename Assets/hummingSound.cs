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
}
