using System;
using Extensions.CustomMath.LogicComposition;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ObjectiveCondition : ICondition<QuestOutcome>
{
    public QuestObjective objectiveKey;

    public bool Evaluate(QuestOutcome context)
    {
        if (objectiveKey == null)
        {
            Debug.LogWarning("[ObjectiveCondition] Objective key is null.");
            return false;
        }
        
        return QuestManager.Instance.IsObjectiveComplete(objectiveKey);
    }
}

[Serializable]
public class OutcomeCondition : ICondition<QuestOutcome>
{ 
    public QuestOutcome outcomeKey;
    public int requiredCompletions = 1;

    public bool Evaluate(QuestOutcome context)
    {
        if (outcomeKey == null)
        {
            Debug.LogWarning("[OutcomeCondition] Outcome key is null.");
            return false;
        }
        
        return QuestManager.Instance.IsObjectiveComplete(outcomeKey, requiredCompletions);
    }
}

