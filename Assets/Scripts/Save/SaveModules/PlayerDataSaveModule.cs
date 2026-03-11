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
    private PlayerStamina stamina;
    private ManageRespawn respawnManager;

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
        if (!stamina) stamina = PlayerID.Instance.playerStamina;
        if (!respawnManager) respawnManager = PlayerID.Instance.GetComponent<ManageRespawn>();
        playerData ??= new PlayerSaveData();

        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        playerData = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary);
        
        player.transform.position = playerData.Position;
        playerCam.SetRotation(playerData.Rotation);
        health.CurrentHealth = playerData.curHealth;
        hunger.CurrentHunger = playerData.curHunger;
        stamina.CurrentStamina = playerData.curStamina;
        stamina.StaminaDisabled = playerData.staminaDisabled;
        stamina.HasGloves = playerData.hasGloves;
        respawnManager.respawnPoint = playerData.RespawnPosition;

        Debug.Log("Set stamina " + stamina.CurrentStamina);

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
        stamina = PlayerID.Instance.playerStamina;

        if (player == null || playerCam == null || hunger == null || health == null)
        {
            Debug.LogWarning("Aborting save");
            return false;
        }

        if (PlayerID.Instance.IsAlive)
        {
            playerData.Position = player.transform.position;
        }
        else
        { // Set player's position as respawn point if saving while dead
            playerData.Position = respawnManager.respawnPoint;
        }
        playerData.Rotation = playerCam.GetRotation();
        playerData.curHealth = health.CurrentHealth;
        playerData.curHunger = hunger.CurrentHunger;
        playerData.curStamina = stamina.CurrentStamina;
        playerData.staminaDisabled = stamina.StaminaDisabled;
        playerData.hasGloves = stamina.HasGloves;
        playerData.RespawnPosition = respawnManager.respawnPoint;
        
        byte[] bytes = SerializationUtility.SerializeValue(playerData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
