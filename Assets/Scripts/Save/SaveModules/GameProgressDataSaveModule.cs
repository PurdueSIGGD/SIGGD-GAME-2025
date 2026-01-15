using System;
using Sirenix.Serialization;

public class GameProgressDataSaveModule : ISaveModule
{
    public GameProgressData saveData = new();
    private readonly string savePath = $"{FileManager.savesDirectory}/gameProgress";

    public bool serialize()
    {
        byte[] bytes = SerializationUtility.SerializeValue(saveData, DataFormat.JSON);
        FileManager.Instance.WriteFile(savePath, bytes);
        return true;
    }

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath))
        {
            saveData = new();
            return false;
        }

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        saveData = SerializationUtility.DeserializeValue<GameProgressData>(bytes, DataFormat.JSON);
        return true;
    }

    public void CompletePrologue()
    {
        saveData.hasCompletedPrologue = true;
    }
}

[Serializable]
public class GameProgressData
{
    public bool hasCompletedPrologue;
}
