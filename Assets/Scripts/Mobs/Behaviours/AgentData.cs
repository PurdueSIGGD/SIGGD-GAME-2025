using UnityEngine;
using UnityEngine.AI; 

namespace SIGGD.Mobs
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentData : MonoBehaviour
    {
        private EntityHealthManager healthManager;
        private NavMeshAgent agent;

        public Boundary boundary;
        public NavMeshQueryFilter filter { get; private set; }

        private string mobId = "none";


        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            EntityHealthManager healthManager = GetComponent<EntityHealthManager>();
            filter = new NavMeshQueryFilter
            {
                agentTypeID = agent.agentTypeID,
                areaMask = NavMesh.AllAreas
            };
        }

        public string GetMobId()
        {
            return mobId;
        }

        public void SetMobId(string mobId)
        {
            this.mobId = mobId;
        }
    }
}