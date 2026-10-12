using System;
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

    /**
     * Brings in quest progress from a save. Progress only moves forward: the save is re-read on every scene
     * load and is older than what happened since it was written, so a quest finished after the last save
     * stays finished when the scene reloads after a death. An instance can also already exist with no
     * progress (outcomes register themselves as soon as their asset loads), so existing entries are merged
     * rather than kept or replaced outright.
     */
    public void LoadSavedData(Dictionary<QuestObjective, QuestObjectiveInstance> saveData)
    {
        foreach (QuestObjective key in saveData.Keys)
        {
            QuestObjectiveInstance saved = saveData[key];
            if (questInstances.TryGetValue(key, out var existing))
            {
                existing.totalCompletions = Math.Max(existing.totalCompletions, saved.totalCompletions);
                existing.conditionsMetCount = Math.Max(existing.conditionsMetCount, saved.conditionsMetCount);
            }
            else
            {
                questInstances[key] = saved;
            }
        }
    }

    /**
     * Forgets all quest progress. Used when the save is wiped to start a new game.
     */
    public void ResetAll()
    {
        questInstances.Clear();
    }

    public Dictionary<QuestObjective, QuestObjectiveInstance> GetQuestInstances()
    {
        return questInstances;
    }
}