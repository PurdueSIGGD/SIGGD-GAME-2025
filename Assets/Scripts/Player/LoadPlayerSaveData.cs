using UnityEngine;

/// <summary>
/// Reads save data into player gameobject
/// </summary>
public class LoadPlayerSaveData : MonoBehaviour
{
    private GameObject player;
    private PlayerSaveData saveData;

    // Loading data here as SaveManager executes prior to PlayerID instance
    void Start()
    {
        // load in data saved from PlayerSaveData, if possible
        player = PlayerID.Instance.gameObject;
        saveData = PlayerDataSaveModule.playerData;
        player.transform.position = saveData.Position;
    }
}
