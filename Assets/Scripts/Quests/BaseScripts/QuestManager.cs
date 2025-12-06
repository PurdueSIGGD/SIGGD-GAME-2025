using System.Collections.Generic;
using Extensions.Patterns;

/**
 * Manages quest objectives and their instances within the game. Persists across scene loads and
 * provides methods to retrieve or create quest objective instances and check their completion status.
 */
public class QuestManager : LazySingleton<QuestManager>
{

    private readonly Dictionary<QuestObjective, QuestObjectiveInstance> questInstances = new();

    public QuestObjectiveInstance GetOrCreateInstance(QuestObjective objective)
    {
        if (!questInstances.TryGetValue(objective, out var instance))
        {
            instance = new QuestObjectiveInstance(objective);
            questInstances[objective] = instance;
        }
        return instance;
    }

    public bool IsObjectiveComplete(QuestObjective objective, int times = 1)
    {
        return questInstances.TryGetValue(objective, out var state) && state.ConditionsMet(times);
    }
}