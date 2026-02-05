using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : ISaveModule
{
    public PlayerSaveData playerData;

    private readonly string savePath = $"{FileManager.savesDirectory}/playerData";
    private GameObject player;
    private FirstPersonCamera playerCam;
    private PlayerHunger hunger;
    private EntityHealthManager health;

    public bool deserialize()
    {
        if (!PlayerID.Instance)
        {
            Debug.LogWarning("Cannot find Player when attempting to load player data");
            return false;
        }
        if (!player) player = PlayerID.Instance.gameObject;
        if (!playerCam) playerCam = PlayerID.Instance.cam;
        if (!health) health = PlayerID.Instance.playerHealth;
        if (!hunger) hunger = PlayerID.Instance.playerHunger;
        playerData ??= new PlayerSaveData();

        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary);
        
        player.transform.position = playerData.Position;
        playerCam.SetRotation(playerData.Rotation);
        health.CurrentHealth = playerData.curHealth;
        hunger.CurrentHunger = playerData.curHunger;

        return true;
    }

    public bool serialize()
    {
        if (!PlayerID.Instance)
        {
            Debug.LogWarning("Cannot find player when attempting to save player data");
            return false;
        }
        playerCam = PlayerID.Instance.cam;
        player = PlayerID.Instance.gameObject;
        hunger = PlayerID.Instance.playerHunger;
        health = PlayerID.Instance.playerHealth;

        if (player == null || playerCam == null || hunger == null || health == null)
        {
            Debug.LogWarning("Aborting save");
            return false;
        }

        playerData.Position = player.transform.position;
        playerData.Rotation = playerCam.GetRotation();
        playerData.curHealth = health.CurrentHealth;
        playerData.curHunger = hunger.CurrentHunger;
        
        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
