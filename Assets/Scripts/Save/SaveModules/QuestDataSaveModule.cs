using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestDataSaveModule : ISaveModule
{
    private readonly string savePath = $"{FileManager.savesDirectory}/questData";

    private Dictionary<QuestObjective, QuestObjectiveInstance> questData;



    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        DeserializationContext context = new DeserializationContext();
        context.IndexReferenceResolver = new UnityReferenceResolver();
        questData = SerializationUtility.DeserializeValue<Dictionary<QuestObjective, QuestObjectiveInstance>>(bytes, DataFormat.Binary, context);
        
        QuestManager.Instance.LoadSavedData(questData);
        return true;
    }

    public bool serialize()
    {
        questData = QuestManager.Instance.GetQuestInstances();
        if (questData == null)
        {
            Debug.LogError("Cannot save quest data");
            return false;
        }

        SerializationContext context = new SerializationContext();
        context.IndexReferenceResolver = new UnityReferenceResolver();
        byte[] bytes = SerializationUtility.SerializeValue(questData, DataFormat.Binary, context);
        FileManager.Instance.WriteFile(savePath, bytes);
        return true;
    }
}

[Serializable]
class QuestSaveData
{
    private List<QuestObjective> key;
    private List<QuestObjectiveInstance> val;
}