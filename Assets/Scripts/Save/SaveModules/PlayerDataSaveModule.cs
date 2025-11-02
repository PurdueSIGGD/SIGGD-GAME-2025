using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : MonoBehaviour, ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/playerData";

    public static PlayerSaveData playerData;

    public static GameObject player;

    public static FirstPersonCamera playerCam;
    //public static PlayerStats stats;
    public static EntityHealthManager health;

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.JSON);
        
        if (!player) player = PlayerID.Instance.gameObject;
        if (!playerCam) playerCam = PlayerID.Instance.GetComponent<FirstPersonCamera>();
        playerData ??= new PlayerSaveData();
        
        player.transform.position = playerData.Position;
        return true;
    }

    public bool serialize()
    {
        //if (player && player.TryGetComponent(out PlayerID pid))
        //{
        //    playerData.Position = pid.stateMachine.LastGroundedPosition;
        //}

        playerCam = PlayerID.Instance.cam;
        player = PlayerID.Instance.rb.gameObject;
        //stats = player.GetComponent<PlayerStats>();
        health = player.GetComponent<EntityHealthManager>();

        if (player == null || playerCam == null || health == null)
        {
            Debug.LogWarning("Aborting save");
            return false;
        }

        playerData.Position = player.transform.position;
        playerData.Rotation = playerCam.GetRotation();
        
        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.JSON);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
