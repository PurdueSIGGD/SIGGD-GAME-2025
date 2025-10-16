using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Config;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace SIGGD.Goap.Behaviours
{
    public class HungerBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;

        [field:SerializeField]
        public float stamina { get; set; }

        [SerializeField]
        private HungerConfigSO HungerConfig;

        private bool damageTickActive = true;

        [SerializeField]
        private EntityHealthManager HealthManager;
        private void Awake()
        {
            stamina = Random.Range(HungerConfig.minStartingHunger, HungerConfig.maxStartingHunger);
        }
        public void Update()
        {
            stamina -= Time.deltaTime * StatsConfig.staminaLossRate * 2;
        }
    }

}

