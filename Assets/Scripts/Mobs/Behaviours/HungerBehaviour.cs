using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Config;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace SIGGD.Mobs
{
    public class HungerBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;

        [field:SerializeField]
        public float hunger { get; private set; }

        public int foodCount = 0;

        [SerializeField]
        private BaseStats HungerConfig;

        [SerializeField] private DamageContext hungerDamageContext;

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
                HealthManager.TakeDamage(hungerDamageContext);
                yield return new WaitForSeconds(HungerConfig.damageTickRate);
            }
            damageTickActive = false;
        }
        public void ReduceHunger(float amount)
        {
            hunger -= amount;
        }
    }
}

