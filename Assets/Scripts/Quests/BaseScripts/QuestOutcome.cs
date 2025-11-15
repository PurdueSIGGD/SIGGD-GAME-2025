using System;
using System.Collections.Generic;
using Extensions.CustomMath.LogicComposition;
using Extensions.EventBus;
using UnityEngine;

/**
 * A Quest Outcome represents the completion state of a quest, defined by specific conditions that need to be met.
 */
[CreateAssetMenu(fileName = "New Quest Outcome", menuName = "Quests/Quest Outcome")]
public class QuestOutcome : QuestObjective
{
    public int maxCompletions = 1;
    
    [Header("Quest Condition")]
    [SerializeReference] public ICondition<QuestOutcome> questCondition;

    private Dictionary<QuestObjective, ObjectiveCondition> questTriggerConditions;
    private EventBinding<QuestBroadcastEvent> questBroadcastBinding;

    private void OnEnable()
    {
        questBroadcastBinding = new EventBinding<QuestBroadcastEvent>(OnQuestBroadcastReceived);
        EventBus<QuestBroadcastEvent>.Register(questBroadcastBinding);

        questTriggerConditions = new();
        Collect(questCondition);
        
        // evaluate condition on enable to catch any pre-existing states
        
        CheckForExecution();
    }

    private void OnDisable()
    {
        EventBus<QuestBroadcastEvent>.Deregister(questBroadcastBinding);
    }

    private void OnQuestBroadcastReceived(QuestBroadcastEvent e)
    {
        if (!questTriggerConditions.TryGetValue(e.objective, out var condition)) return; // Not relevant to this outcome

        CheckForExecution();
    }

    private void CheckForExecution()
    {
        if (questCondition == null) return;

        bool result = questCondition.Evaluate(this);
        
        // mark the quest as complete if the condition is met
        var instance = QuestManager.Instance.GetOrCreateInstance(this);


        if (result)
        {
            instance.MarkConditionsMet(); // mark this objective as complete
        }
        else
        {
            instance.ResetCompletions(); // one or more conditions are not met, reset completions
        }
        
        Debug.Log($"[QuestOutcome] Broadcasting outcome '{name}' evaluation result: {result}");
        
        EventBus<QuestBroadcastEvent>.Raise(new QuestBroadcastEvent(this, () => result));
    }

    private void Collect<TContext>(ICondition<TContext> condition)
    {
        switch (condition)
        {
            case ObjectiveCondition leaf:
                questTriggerConditions[leaf.objectiveKey] = leaf;
                break;

            case AndCondition<TContext> andC:
                foreach (var child in andC.children)
                    Collect(child);
                break;

            case OrCondition<TContext> orC:
                foreach (var child in orC.children)
                    Collect(child);
                break;

            case NotCondition<TContext> notC when notC.child != null:
                Collect(notC.child);
                break;
        }
    }
}

public struct QuestBroadcastEvent : IEvent
{
    public QuestObjective objective;
    public Func<bool> Evaluator;

    public QuestBroadcastEvent(QuestObjective objective, Func<bool> evaluator)
    {
        this.objective = objective;
        Evaluator = evaluator;
    }
}