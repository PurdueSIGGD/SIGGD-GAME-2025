using UnityEngine;

public class AudioLogPlayerStrategy : IPlayerActionStrategy
{
    [Header("Audio Object for Audio Log")]
    [SerializeField] public AudioLogObject audioObject;

    protected override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Audio Log Strategy Invoked");
        AudioLogManager.Instance.PlayAudioLog(audioObject.audioName, PlayerID.Instance.gameObject);
        Debug.Log($"Playing audio log with ID: {audioObject.audioName}");
    }

}
