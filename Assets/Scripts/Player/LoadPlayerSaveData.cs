using UnityEngine;

/// <summary>
/// Reads save data into player gameobject
/// </summary>
public class LoadPlayerSaveData : MonoBehaviour
{
    private GameObject player;
    private PlayerSaveData saveData;
    private Camera playerCam;

    // Loading data here as SaveManager executes prior to PlayerID instance
    void Start()
    {
        // load in data saved from PlayerSaveData, if possible
        player = PlayerID.Instance.gameObject;
        playerCam = PlayerID.Instance.cam; 
        saveData = PlayerDataSaveModule.playerData;
        player.transform.position = saveData.Position;
        playerCam.transform.eulerAngles = saveData.Rotation;
    }
}
