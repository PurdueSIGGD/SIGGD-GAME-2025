using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDataSaveModule : ISaveModule
{
    public SceneSaveData sceneData;

    private readonly string savePath = $"{FileManager.savesDirectory}/sceneData";
    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        sceneData = SerializationUtility.DeserializeValue<SceneSaveData>(bytes, DataFormat.Binary);
        SceneSaveManager.Instance.sceneName = sceneData.sceneName;
        return true;
    }

    public bool serialize()
    {
        sceneData.sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Serializing scene " +  sceneData.sceneName);
        byte[] bytes = SerializationUtility.SerializeValue(sceneData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);
        return true;
    }
}
