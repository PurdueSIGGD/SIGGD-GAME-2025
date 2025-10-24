using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace SIGGD.Goap.Behaviours
{
    public class HungerBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;

        [field:SerializeField]
        public float hunger { get; set; }

        private void Awake()
        {
            hunger = Random.Range(20f, 100f);
        }
        public void Update()
        {
            hunger += Time.deltaTime * 2f;
        }


    }

}

