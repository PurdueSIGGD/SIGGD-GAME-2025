
using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs
{
    public class AgentData : MonoBehaviour
    {

        // Use this for initialization
        private EntityHealthManager healthManager;
        private StaminaBehaviour staminaBehaviour;
        private NavMeshAgent agent;
        public int desperationLevel;
        public int powerLevel;
        public float energyLevel;
        public int aggressionLevel;
        private float maxEnergy = 0;
        private int maxPower = 0;
        private int maxDesperation = 0;
        private int maxAggression = 0;
        public NavMeshQueryFilter filter { get; private set; }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            EntityHealthManager healthManager = GetComponent<EntityHealthManager>();
            StaminaBehaviour staminaBehaviour = GetComponent<StaminaBehaviour>();
            NavMeshQueryFilter filter = new NavMeshQueryFilter();
            filter.agentTypeID = agent.agentTypeID;
            filter.areaMask = NavMesh.AllAreas;
            /*
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agent.agentTypeID, 
                areaMask = NavMesh.AllAreas
            };
            */
           // NavMeshQueryFilter navMeshQueryFilter = new nav
        }
        void Start()
        {
            aggressionLevel = 1;
            powerLevel = 1;
            energyLevel = 1;
            aggressionLevel = 1;
        }

        void Update()
        {

        }
        public void IncreaseEnergy(float factor)
        {
            energyLevel = Mathf.Clamp(energyLevel * factor, 0, maxEnergy);
        }
        public void DecreaseEnergy(int amount)
        {
            energyLevel = Mathf.Clamp(energyLevel - amount, 0, maxEnergy);
        }
        public void IncreasePower(int amount)
        {
            powerLevel = Mathf.Clamp(powerLevel + amount, 0, maxPower);
        }
        public void DecreasePower(int amount)
        {
            powerLevel = Mathf.Clamp(powerLevel - amount, 0, maxPower);
        }
        public void IncreaseDesperation(int amount)
        {
            desperationLevel = Mathf.Clamp(desperationLevel + amount, 0, maxDesperation);
        }
        public void DecreaseDesperation(int amount)
        {
            desperationLevel = Mathf.Clamp(desperationLevel - amount, 0, maxDesperation);
        }
        public void IncreaseAggression(int amount)
        {
            aggressionLevel = Mathf.Clamp(aggressionLevel + amount, 0, maxAggression);
        }
        public void DecreaseAggression(int amount)
        {
            aggressionLevel = Mathf.Clamp(aggressionLevel - amount, 0, maxAggression);
        }
        public void CalculateEnergyLevel()
        {
            float energyLevel = Mathf.Lerp(0, 1, healthManager.CurrentHealth * staminaBehaviour.stamina / 100);
        }
    }
}