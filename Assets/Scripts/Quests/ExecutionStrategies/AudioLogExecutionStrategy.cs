using UnityEngine;

public class AudioLogExecutionStrategy : IQuestExecutionStrategy
{
    [SerializeField] public AudioLogObject audioObject;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        AudioLogManager.Instance.PlayAudioLog(audioObject.audioName, PlayerID.Instance.gameObject);
        Debug.Log($"Playing audio log with ID: {audioObject.audioName}");
    }

    public override string ToString()
    {
        return "Trigger Audio Log Event";
    }
}