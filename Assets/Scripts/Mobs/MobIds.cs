using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs
{
    public static class MobIds
    {
        public const string generic = "BaseAgent";
        public const string prey = "PreyAgent";
        public const string hyena = "HyenaAgent";
        public const string buffalo = "Buffalo";
        
        public static int GetAgentTypeByName(string name)
        {
            int count = NavMesh.GetSettingsCount();
            string[] agentTypes = new string[count + 2];
            for (int i = 0; i < count; i++)
            {
                Debug.Log(i);
                var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                string agentName = NavMesh.GetSettingsNameFromID(id);
                if (agentName == name)
                {
                    Debug.Log(id);
                    return id;
                }
            }
            return -1;
        }
    }
}