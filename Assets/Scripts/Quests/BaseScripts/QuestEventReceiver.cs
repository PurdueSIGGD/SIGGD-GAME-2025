using System;
using Extensions.EventBus;
using UnityEngine;

/**
 * Attach to a GameObject to receive quest events and execute corresponding strategies. Should exist in the
 * scene where the quest event needs to be handled.
 */
public class QuestEventReceiver : MonoBehaviour
{
    public enum ReceiverType
    {
        TriggerOnceAlways, // cannot be reset once triggered after a positive broadcast
        CanBeDeactivatedToRetrigger, // can be reset when negatively broadcasted
        // and retriggered when positively broadcasted again
        AlwaysTriggers, // always triggers when the event is positively broadcasted
    }
    
    public ReceiverType receiverType = ReceiverType.TriggerOnceAlways;
    
    public QuestOutcome questOutcome;
    [SerializeReference] public IQuestExecutionStrategy executionStrategy;
    
    private bool CanBeDeactivated => receiverType != ReceiverType.TriggerOnceAlways;

    private EventBinding<QuestBroadcastEvent> eventBinding;
    
    private bool AlwaysActive => receiverType == ReceiverType.AlwaysTriggers;
    
    private bool isTriggered;

    private void Start()
    {
        TryTrigger();
        
    }

    private void OnEnable()
    {
        eventBinding = new EventBinding<QuestBroadcastEvent>(OnQuestTriggered);
        EventBus<QuestBroadcastEvent>.Register(eventBinding);
    }

    private void OnDisable()
    {
        EventBus<QuestBroadcastEvent>.Deregister(eventBinding);
    }
    

    private void OnQuestTriggered(QuestBroadcastEvent e)
    {
        Debug.Log($"QuestEventReceiver: Received quest event for objective {e.objective.name} on receiver for outcome {questOutcome.name}");
        
        if (e.objective != questOutcome)
        {
            return;
        }

        bool result = e.Evaluator();

        if (result)
        {
            Debug.Log($"QuestEventReceiver: Triggering quest outcome {questOutcome.name}");
            TryTrigger();
        }
        else
        {
            Debug.Log($"QuestEventReceiver: Deactivating quest outcome {questOutcome.name}");
            Deactivate();
        }
    }

    private void TryTrigger()
    {
        if (!isTriggered || AlwaysActive)
        {
            Debug.Log($"QuestEventReceiver: Triggering quest outcome");
            var instance = QuestManager.Instance.GetOrCreateInstance(questOutcome);
            
            
            if (instance.TryCompleteByReceiver()) // if able to be executed, do so
            {
                Debug.Log($"QuestEventReceiver: Triggered quest outcome");
                isTriggered = true;
                executionStrategy?.Initialize(this);
            }
            else
            {
                Debug.Log($"QuestEventReceiver: Could not trigger quest outcome - no pending executions");
            }
        }
    }
    
    private void Deactivate()
    {
        if (isTriggered && CanBeDeactivated)
        {
            isTriggered = false;
            executionStrategy?.Deactivate();
        }
    }

    private void Update()
    {
        if (isTriggered) executionStrategy.Update();
    }

    private void LateUpdate()
    {
        if (isTriggered) executionStrategy.LateUpdate();
    }

    private void FixedUpdate()
    {
        if (isTriggered) executionStrategy.FixedUpdate();
    }
}
