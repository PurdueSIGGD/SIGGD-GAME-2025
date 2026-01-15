using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Config;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace SIGGD.Mobs
{
    public class StaminaBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public float stamina { get; private set; }

        //AgentData AgentData;
        bool sprintAllowed = false;

        [SerializeField]
        private BaseStats stats;
        public float energyLevel = 1f;
        private void Awake()
        {
            //AgentData = GetComponent<AgentData>();

            stamina = stats.maxStamina;
        }
        public void Update()
        {

        }

        public void FixedUpdate()
        {
            //if (stamina < statConfig.maxStamina * AgentData.energyLevel)
            if (stamina < stats.maxStamina)
            {
                AddStamina(stats.staminaGainRate * Time.fixedDeltaTime);
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
        /*
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

