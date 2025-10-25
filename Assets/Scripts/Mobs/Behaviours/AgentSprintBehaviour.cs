using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Config;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace SIGGD.Goap.Behaviours
{
    public class AgentSprintBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public float stamina { get; set; }

        //AgentData AgentData;
        bool sprintAllowed = false;

        [SerializeField]
        private BaseStatConfig statConfig;
        private void Awake()
        {
            //AgentData = GetComponent<AgentData>();
            stamina = statConfig.maxStamina;
        }
        public void Update()
        {

        }
        public void EnableSprint()
        {
            sprintAllowed = true;
        }
        public void DisableSprint()
        {
            sprintAllowed = false;
        }
        /*
        public void FixedUpdate()
        {
            if (stamina < statConfig.maxStamina * AgentData.energyLevel)
            {
                ReduceStamina(statConfig.staminaGainRate);
            }
        }
        
        public void ReduceStamina(float amount)
        {
            stamina -= amount * energyLevel;
        }
        public void AddStamina(float amount)
        {
            stamina += amount * energyLevel;
        }
        public float GetStamina()
        {
            return stamina;
        }
        public bool ShouldSprint()
        {
            if (stamina < statConfig.staminaThreshold)
                return false;
            return true;
        }
        public float CalculateSprintMultiplier()
        {
            if (!ShouldSprint())
                return 1f;
            stamina
            desperation,tiredness,distance,urgency,stamina
        }
        */
    }
}

