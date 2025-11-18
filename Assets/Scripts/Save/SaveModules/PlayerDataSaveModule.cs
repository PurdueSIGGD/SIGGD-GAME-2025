using Sirenix.Serialization;
using UnityEngine;

public class PlayerDataSaveModule : ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/playerData";

    public static PlayerSaveData playerData;

    public static GameObject player;

    public static FirstPersonCamera playerCam;
    public static PlayerHunger hunger;
    public static EntityHealthManager health;

    public bool deserialize()
    {
        if (!player) player = PlayerID.Instance.gameObject;
        if (!playerCam) playerCam = PlayerID.Instance.GetComponent<FirstPersonCamera>();
        if (!health) health = PlayerID.Instance.GetComponent<EntityHealthManager>();
        if (!hunger) hunger = PlayerID.Instance.GetComponent<PlayerHunger>();
        playerData ??= new PlayerSaveData();

        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary);
        
        player.transform.position = playerData.Position;
        if (playerData.curHealth < 0)
        {
            health.CurrentHealth = health.MaxHealth;
        }
        else
        {
            health.CurrentHealth = playerData.curHealth;
        }

        if (playerData.curHunger < 0)
        {
            hunger.CurrentHunger = hunger.MaxHunger;
        }
        else
        {
            hunger.CurrentHunger = playerData.curHunger;
        }

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
        hunger = PlayerID.Instance.GetComponent<PlayerHunger>();
        health = PlayerID.Instance.GetComponent<EntityHealthManager>();

        if (player == null || playerCam == null || health == null)
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
