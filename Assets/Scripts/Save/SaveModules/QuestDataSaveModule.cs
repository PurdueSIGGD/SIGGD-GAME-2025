using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestDataSaveModule : ISaveModule
{
    private readonly string savePath = $"{FileManager.savesDirectory}/questData";

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) 
            return false;

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        DeserializationContext context = new DeserializationContext();
        context.IndexReferenceResolver = new UnityReferenceResolver();
        QuestSaveData saveData = SerializationUtility.DeserializeValue<QuestSaveData>(bytes, DataFormat.Binary, context);
            
        if (saveData == null)
        {
            Debug.LogError("Failed to deserialize quest save data");
            return false;
        }

        // Convert from GUID-based format to runtime dictionary
        var questInstances = saveData.ToQuestInstances(RegistryHub.Instance);
        QuestManager.Instance.LoadSavedData(questInstances);
        return true;
    }

    public bool serialize()
    {
        var questData = QuestManager.Instance.GetQuestInstances();
        if (questData == null)
        {
            Debug.LogError("Cannot save quest data: QuestManager returned null");
            return false;
        }

        // Convert from runtime dictionary to GUID-based format
        QuestSaveData saveData = QuestSaveData.FromQuestInstances(questData, RegistryHub.Instance);

        SerializationContext context = new SerializationContext();
        context.IndexReferenceResolver = new UnityReferenceResolver();
        byte[] bytes = SerializationUtility.SerializeValue(saveData, DataFormat.Binary, context);
        FileManager.Instance.WriteFile(savePath, bytes);
        return true;
    }
}