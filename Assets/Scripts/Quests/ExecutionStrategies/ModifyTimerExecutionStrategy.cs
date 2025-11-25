using UnityEngine;
public class ModifyTimerExecutionStrategy : IQuestExecutionStrategy
{
    public enum TimerModificationType
    {
        AddTime,
        SubtractTime,
        SetTimeScale,
        Pause,
        Resume,
        TogglePause
    }
    
    public QuestEventBroadcaster eventBroadcaster;
    public TimerModificationType modificationType;
    public float timeValue;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        if (eventBroadcaster == null)
        {
            Debug.LogError("Event Broadcaster is not assigned.");
            return;
        }
        
        if (eventBroadcaster.conditionStrategy is not TimerConditionStrategy timerCondition)
        {
            Debug.LogError("The condition strategy is not a TimerConditionStrategy.");
            return;
        }
        
        switch (modificationType)
        {
            case TimerModificationType.AddTime:
                timerCondition.Skip(timeValue);
                break;
            case TimerModificationType.SubtractTime:
                timerCondition.Skip(-timeValue);
                break;
            case TimerModificationType.SetTimeScale:
                timerCondition.SetTimeScale(timeValue);
                break;
            case TimerModificationType.Pause:
                timerCondition.Pause();
                break;
            case TimerModificationType.Resume:
                timerCondition.Resume();
                break;
            case TimerModificationType.TogglePause:
                if (timerCondition.IsRunning)
                {
                    timerCondition.Pause();
                }
                else
                {                   
                    timerCondition.Resume();
                }
                break;
            default:
                Debug.LogError("Invalid modification type.");
                break;
        }
    }
}