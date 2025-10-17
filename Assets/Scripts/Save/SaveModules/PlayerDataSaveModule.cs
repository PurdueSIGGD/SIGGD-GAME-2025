using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/playerData";

    public static PlayerSaveData playerData = new PlayerSaveData();

    public static GameObject player;

    public static FirstPersonCamera camera;

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary);

        player.transform.position = playerData.Position;
        camera.transform.rotation = playerData.Rotation;

        return true;
    }

    public bool serialize()
    {
        if (player) playerData.Position = player.transform.position;
        if (camera) playerData.Rotation = camera.transform.rotation;

        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
