using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : MonoBehaviour, ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/playerData";

    public static PlayerSaveData playerData = new PlayerSaveData();

    public static GameObject player;

    public static FirstPersonCamera mainCamera = player.GetComponent<FirstPersonCamera>();

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary);

        player.transform.position = playerData.Position;
        mainCamera.transform.rotation = playerData.Rotation;

        return true;
    }

    public bool serialize()
    {
        if (player && player.TryGetComponent(out PlayerID pid))
        {
            playerData.Position = pid.stateMachine.LastGroundedPosition;
            if (mainCamera) playerData.Rotation = mainCamera.transform.rotation;
        }

        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
