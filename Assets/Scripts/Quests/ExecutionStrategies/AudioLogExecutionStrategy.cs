using UnityEngine;

public class AudioLogExecutionStrategy : IQuestExecutionStrategy
{
    [SerializeField] public string audioLogID;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        AudioLogManager.Instance.PlayAudioLog(audioLogID, PlayerID.Instance.gameObject);
        Debug.Log($"Playing audio log with ID: {audioLogID}");
    }

    public override string ToString()
    {
        return "Trigger Audio Log Event";
    }
}