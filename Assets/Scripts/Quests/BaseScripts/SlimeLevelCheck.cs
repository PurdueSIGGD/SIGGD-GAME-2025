using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;

public class SlimeLevelCheck : MonoBehaviour
{
    [SerializeField] public AudioLogObject audioObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //this all should work but for some reason the audio log object for spit isn't playing audio. 
    void Start()
    {
        var player = SaveManager.Instance?.Get<PlayerModule>();
        //if (player != null) player.SlimeLevel++;

        if (player != null && player.SlimeLevel >= 1)
        {
            Debug.Log("Play first time slimed dialogue");
            AudioLogManager.Instance.PlayAudioLog(audioObject.audioName, PlayerID.Instance.gameObject);
            Debug.Log($"Playing audio log with ID: {audioObject.audioName}");
            SphereCollider collider = GetComponent<SphereCollider>();

            collider.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
