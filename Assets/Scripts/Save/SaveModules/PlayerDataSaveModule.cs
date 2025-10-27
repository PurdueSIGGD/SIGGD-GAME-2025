using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/playerData";

    public static PlayerSaveData playerData = new PlayerSaveData();

    public static GameObject player;

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary);

        return true;
    }

    public bool serialize()
    {
        if (player && player.TryGetComponent(out PlayerID pid))
        {
            playerData.Position = pid.playerMovement.LastGroundedPosition;
        }

        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
