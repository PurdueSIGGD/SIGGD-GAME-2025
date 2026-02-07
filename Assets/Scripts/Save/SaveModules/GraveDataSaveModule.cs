using Sirenix.Serialization;
using UnityEngine;

public class GraveDataSaveModule : ISaveModule
{
    public bool deserialize()
    {
        return true;
    }

    public bool serialize()
    {
        GameObject graveObj = PlayerID.Instance.GetComponent<ManageRespawn>().GetCurGrave();
        if (graveObj != null) 
        {
        }
        return true;
    }
}
