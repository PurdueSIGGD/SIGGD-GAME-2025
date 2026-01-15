using System;
using Extensions.EventBus;
using UnityEngine;

public class QuestEventBroadcaster : MonoBehaviour
{
    public QuestObjective objectiveKey;
    [SerializeReference] public IQuestConditionStrategy conditionStrategy;

    private bool conditionTriggered;

    private void Awake()
    {
        RunBroadcastCheck(strategy => strategy.Initialize(this));
    }

    private void Update()
    {
        RunBroadcastCheck(strategy => strategy.Update());
    }

    private void LateUpdate()
    {
        RunBroadcastCheck(strategy => strategy.LateUpdate());
    }

    private void FixedUpdate()
    {
        RunBroadcastCheck(strategy => strategy.FixedUpdate());
    }

    private void OnDestroy()
    {
        RunBroadcastCheck(strategy => strategy.Destroy());
    }

    private void RunBroadcastCheck(Action<IQuestConditionStrategy> action)
    {
        if (conditionTriggered && conditionStrategy.StopIfTriggered()) return; // Skip if already triggered and 
                                                                                      // and the strategy would continuously broadcast
                                                                                        // otherwise we would broadcast all the time
        action?.Invoke(conditionStrategy);
    }
    
    public void ResetBroadcast()
    {
        conditionTriggered = false;
    }

    public void Broadcast()
    {
        bool condition = conditionStrategy?.Evaluate() ?? false;
        
        conditionTriggered = condition;
        
        var instance = QuestManager.Instance.GetOrCreateInstance(objectiveKey); // Get or create instance for this objective
        if (condition)
        {
            if (instance.MarkConditionsMet()) // marking objectives that are completed by this broadcaster as complete
            {
                EventBus<QuestBroadcastEvent>.Raise(new QuestBroadcastEvent(objectiveKey, () => true));
            }
        }
        else
        {
            instance.ResetCompletions(); // reset completions if the condition is not met
            EventBus<QuestBroadcastEvent>.Raise(new QuestBroadcastEvent(objectiveKey, () => false));
        }
    }
}