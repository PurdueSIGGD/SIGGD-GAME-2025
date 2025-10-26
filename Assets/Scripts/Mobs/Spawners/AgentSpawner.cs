using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using System.Drawing;
using UnityEngine;
namespace SIGGD.Mobs.Spawners
{
    public class AgentSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject agentPrefab;

        // different mob types?
        private void Awake()
        {
            this.agentPrefab.SetActive(false);
        }
        void Start()
        {
            var agent = Instantiate(this.agentPrefab, new Vector3(10, -1, 10), new Quaternion(10, 10, 10, 10)).GetComponent<GoapActionProvider>();

            agent.gameObject.SetActive(true);
        }

        void Update()
        {

        }
    }

}