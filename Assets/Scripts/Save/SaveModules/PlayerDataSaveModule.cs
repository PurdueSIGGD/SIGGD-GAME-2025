using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : MonoBehaviour, ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/playerData";

    public static PlayerSaveData playerData = new PlayerSaveData();

    public static GameObject player;

    public static Camera playerCam;
    public static PlayerStats stats;
    public static EntityHealthManager health;

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.JSON);
        if (!player)
        {
            player = PlayerID.Instance.gameObject;
        }
        if (!playerCam)
        {
            playerCam = Camera.main;
        }
        player.transform.position = playerData.Position;
        playerCam.transform.eulerAngles = playerData.Rotation;
        Debug.Log($"[LOAD] Camera rotation loaded: {playerData.Rotation}");

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
        stats = player.GetComponent<PlayerStats>();
        health = player.GetComponent<EntityHealthManager>();

        if (player == null || playerCam == null || stats == null || health == null) return false;

        playerData.Position = player.transform.position;
        playerData.Rotation = playerCam.transform.eulerAngles;
        Debug.Log($"[SAVE] Camera rotation saved: {playerData.Rotation}");
        

        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.JSON);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
