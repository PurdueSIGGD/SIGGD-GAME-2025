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
        public float hunger { get; set; }

        [SerializeField]
        private HungerConfigSO HungerConfig;

        private bool damageTickActive = false;

        [SerializeField]
        private EntityHealthManager HealthManager;
        private void Awake()
        {
            hunger = Random.Range(HungerConfig.minStartingHunger, HungerConfig.maxStartingHunger);
        }
        public void Update()
        {
            hunger += Time.deltaTime * HungerConfig.hungerGainRate;
            if (!damageTickActive && hunger > HungerConfig.damageThreshold)
            {
                StartCoroutine(HungerDamageTick());
            }
        }
        private IEnumerator HungerDamageTick()
        {
            damageTickActive = true;
            while (hunger > HungerConfig.damageThreshold)
            {
                HealthManager.TakeDamage(HungerConfig.damage, gameObject, "hunger damage tick");
                yield return new WaitForSeconds(HungerConfig.damageTickRate);
            }
            damageTickActive = false;
        }
    }

}

