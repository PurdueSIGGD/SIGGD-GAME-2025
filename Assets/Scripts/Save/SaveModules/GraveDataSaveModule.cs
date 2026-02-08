using Sirenix.Serialization;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GraveDataSaveModule : ISaveModule
{

    public GraveSaveData graveSaveData;
    private readonly string savePath = $"{FileManager.savesDirectory}/graveData";
    public bool deserialize()
    {
        graveSaveData ??= new GraveSaveData();
        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        graveSaveData = SerializationUtility.DeserializeValue<GraveSaveData>(bytes, DataFormat.Binary);

        if (graveSaveData.info != null && graveSaveData.count != null)
        {
            string infos = "[";
            for (int i = 0; i < graveSaveData.info.Length; i++)
            {
                infos += graveSaveData.info[i].itemName + " ";
            }
            infos += "]";
            Debug.Log(infos);
            PlayerID.Instance.GetComponent<ManageRespawn>().CreateGrave(graveSaveData.position, graveSaveData.rotation,
                graveSaveData.info, graveSaveData.count);
        }
        else
        {
            Debug.Log("No inventory in grave object");
        }

        return true;
    }

    public bool serialize()
    {
        GameObject graveObj = PlayerID.Instance.GetComponent<ManageRespawn>().GetCurGrave();
        if (graveObj != null)
        {
            graveSaveData.position = graveObj.transform.position;
            graveSaveData.rotation = graveObj.transform.rotation;
            graveSaveData.info = graveObj.GetComponent<GraveInteract>().info;
            graveSaveData.count = graveObj.GetComponent<GraveInteract>().count;
            string infos = "[";
            for (int i = 0; i < graveSaveData.info.Length; i++) {
                infos += graveSaveData.info[i].itemName + " ";
            }
            infos += "]";
            Debug.Log(infos);
            byte[] bytes = SerializationUtility.SerializeValue(graveSaveData, DataFormat.Binary);
            FileManager.Instance.WriteFile(savePath, bytes);
            Debug.Log("Serialized grave object");
        }
        else
        {
            // Write empty
            graveSaveData.position = new Vector3(0, 0, 0);
            graveSaveData.rotation = Quaternion.identity;
            graveSaveData.info = null;
            graveSaveData.count = null;
            byte[] bytes = SerializationUtility.SerializeValue(graveSaveData, DataFormat.Binary);
            FileManager.Instance.WriteFile(savePath, bytes);
            Debug.Log("No grave object to serialize");
        }
        return true;
    }
}
