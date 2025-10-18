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

<<<<<<< Updated upstream
=======
        player.transform.position = playerData.Position;
        camera.transform.rotation = playerData.Rotation;

>>>>>>> Stashed changes
        return true;
    }

    public bool serialize()
    {
<<<<<<< Updated upstream
        if (player) playerData.Position = player.transform.position;
=======
        playerData.Position = player.transform.position;
        playerData.Rotation = camera.transform.rotation;
>>>>>>> Stashed changes

        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
