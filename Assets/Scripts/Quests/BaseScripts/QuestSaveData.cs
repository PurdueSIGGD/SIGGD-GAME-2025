using System;
using System.Collections.Generic;

/**
 * Serializable wrapper for quest save data.
 * Uses GUIDs to reference QuestObjectives instead of direct references,
 * since ScriptableObjects don't serialize well as dictionary keys.
 */
[System.Serializable]
public class QuestSaveData
{
    [System.Serializable]
    public struct QuestObjectiveData
    {
        public string objectiveGuid;
        public int totalCompletions;
        public int conditionsMetCount;
        public int maxCompletions;
    }

    public List<QuestObjectiveData> objectives = new();

    /// <summary>
    /// Convert from the runtime dictionary to a serializable format using GUIDs.
    /// </summary>
    public static QuestSaveData FromQuestInstances(Dictionary<QuestObjective, QuestObjectiveInstance> questInstances, RegistryHub registryHub)
    {
        var saveData = new QuestSaveData();
        var registry = registryHub.GetRegistry<QuestObjective>();

        if (registry == null)
        {
            UnityEngine.Debug.LogError("Quest registry not found in RegistryHub");
            return saveData;
        }

        foreach (var kvp in questInstances)
        {
            var objective = kvp.Key;
            var instance = kvp.Value;

            string guid = registry.GetGuid(objective);
            if (string.IsNullOrEmpty(guid))
            {
                UnityEngine.Debug.LogWarning($"Could not get GUID for objective: {objective.name}");
                continue;
            }

            saveData.objectives.Add(new QuestObjectiveData
            {
                objectiveGuid = guid,
                totalCompletions = instance.totalCompletions,
                conditionsMetCount = instance.conditionsMetCount,
                maxCompletions = instance.maxCompletions
            });
        }

        return saveData;
    }

    /// <summary>
    /// Convert from serialized format back to runtime dictionary using GUIDs to lookup objectives.
    /// </summary>
    public Dictionary<QuestObjective, QuestObjectiveInstance> ToQuestInstances(RegistryHub registryHub)
    {
        var questInstances = new Dictionary<QuestObjective, QuestObjectiveInstance>();
        var registry = registryHub.GetRegistry<QuestObjective>();

        if (registry == null)
        {
            UnityEngine.Debug.LogError("Quest registry not found in RegistryHub");
            return questInstances;
        }

        foreach (var data in objectives)
        {
            QuestObjective objective = registry.Get(data.objectiveGuid);
            if (objective == null)
            {
                UnityEngine.Debug.LogWarning($"Could not find objective with GUID: {data.objectiveGuid}");
                continue;
            }

            var instance = new QuestObjectiveInstance(objective)
            {
                totalCompletions = data.totalCompletions,
                conditionsMetCount = data.conditionsMetCount,
                maxCompletions = data.maxCompletions
            };

            questInstances[objective] = instance;
        }

        return questInstances;
    }
}
