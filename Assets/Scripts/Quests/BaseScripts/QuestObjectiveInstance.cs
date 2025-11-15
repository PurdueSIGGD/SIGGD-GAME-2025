using System;
using UnityEngine;
using UnityEngine.Serialization;

/**
 * <summary>
 * Represents the state of a quest objective, tracking its completion status and how many times it has been completed.
 * We use a separate instance class to manage the state of quest objectives independently from their definitions
 * so that multiple players or sessions can have different states for the same quest objective.
 * </summary>
 */
[Serializable]
public class QuestObjectiveInstance
{
    public QuestObjective objective;
    
    public int totalCompletions; // Tracks the total number of times the objective has been completed
    public int conditionsMetCount; // Tracks how many times the objective's conditions have been met without being completed
    
    public int maxCompletions;

    private readonly object lockObject = new object();
    public bool ConditionsMet(int times = 1) => conditionsMetCount >= times;
    
    public QuestObjectiveInstance(QuestObjective objective)
    {
        this.objective = objective;
        this.maxCompletions = objective is QuestOutcome q ? q.maxCompletions : 1;
    }

    /**
     * <summary>
     * Marks the objective as completed, incrementing the completion count.
     * We want this to just increase the count, as some objectives may be repeatable, so
     * if there is another objective that requires this one to be completed some amount, we should track that.
     * </summary>
     *
     * <returns>True if the conditions were marked as met, false if already at max completions.</returns>
     */
    public bool MarkConditionsMet()
    {
        if (conditionsMetCount >= maxCompletions && maxCompletions > 0)
        {
            Debug.LogWarning($"[QuestOutcomeInstance] Cannot mark conditions met for {objective.name}: already at max completions ({maxCompletions}).");
            return false;
        }
        
        conditionsMetCount = Mathf.Clamp(conditionsMetCount + 1, 0, maxCompletions > 0 ? maxCompletions : int.MaxValue);
        Debug.Log($"[QuestOutcomeInstance] Conditions met: {objective.name} (Count: {conditionsMetCount}/{maxCompletions})");
        return true;
    }
    
    /**
     * <summary>
     * Marks the objective as completed by the receiver, incrementing the receiver completion count.
     * This is used when the objective's receiver successfully executes the objective.
     * </summary>
     * <returns>True if the objective was completed by the receiver, false if there are no pending executions.</returns>
     */
    public bool TryCompleteByReceiver()
    {
        lock (lockObject)
        {
            if (conditionsMetCount < 1 || (totalCompletions >= maxCompletions && maxCompletions > 0))
            {
                Debug.LogWarning($"[QuestOutcomeInstance] Cannot complete objective {objective.name} by receiver: " +
                                 $"conditionsMetCount={conditionsMetCount}, totalCompletions={totalCompletions}, maxCompletions={maxCompletions}.");
                return false;
            }

            totalCompletions++;
            conditionsMetCount = Mathf.Clamp(conditionsMetCount - 1, 0, maxCompletions > 0 ? maxCompletions : int.MaxValue);
            return true;
        }
    }

    /**
     * <summary>
     * Marks the objective as never completed, resetting the completion counts.
     * </summary>
     */
    public void ResetCompletions()
    {
        conditionsMetCount = 0;
    }
}