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
    [Tooltip("Determines how the outcome evaluates its completion based on the condition. " +
             "Toggleable: outcome can be marked complete/incomplete based on condition state (max completions should be 1). " +
             "Instantial: outcome is marked complete once condition is met and cannot be decremented.")]
    public OutcomeEvaluationMode evaluationMode = OutcomeEvaluationMode.Instantial;
    [Header("Quest Condition")]
    [SerializeReference] public ICondition<QuestOutcome> questCondition;

    private Dictionary<QuestObjective, ObjectiveCondition> questTriggerConditions;
    private EventBinding<QuestBroadcastEvent> questBroadcastBinding;

    public enum OutcomeEvaluationMode
    {
        Toggleable, // if the condition is met, the outcome is marked complete; if not met, it is marked incomplete
        Instantial // once the condition is met, the outcome is marked complete and cannot be decremented
    }

    private void OnEnable()
    {
        questBroadcastBinding = new EventBinding<QuestBroadcastEvent>(OnQuestBroadcastReceived);
        EventBus<QuestBroadcastEvent>.Register(questBroadcastBinding);

        questTriggerConditions = new();
        Collect(questCondition);
        
        // evaluate condition on enable to catch any pre-existing states

        CheckForExecution(false);
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

    private void CheckForExecution(bool checkForDeactivate = true)
    {
        if (questCondition == null) return;

        bool result = questCondition.Evaluate(this);
        
        // mark the quest as complete if the condition is met
        var instance = QuestManager.Instance.GetOrCreateInstance(this);


        if (result)
        {
            instance.MarkConditionsMet(); // mark this objective as complete
            Debug.Log($"[QuestOutcome] Broadcasting outcome '{name}' evaluation result: {true}");
        
            EventBus<QuestBroadcastEvent>.Raise(new QuestBroadcastEvent(this, () => true));
        }
        else if (evaluationMode == OutcomeEvaluationMode.Toggleable && checkForDeactivate)
        {
            instance.ResetCompletions(); // one or more conditions are not met, reset completions
            Debug.Log($"[QuestOutcome] Broadcasting outcome '{name}' evaluation result: {true}");
        
            EventBus<QuestBroadcastEvent>.Raise(new QuestBroadcastEvent(this, () => true));
        }
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

    private void OnValidate()
    {
        if (evaluationMode == OutcomeEvaluationMode.Toggleable)
            maxCompletions = 1; // Toggleable outcomes should only have 1 max completion, because triggering them again wouldn't make sense
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