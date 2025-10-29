using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.Spawners
{
    public class HyenaSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject agentPrefab;
        Bounds bounds;
        public float spawningX;
        public float spawningZ;
        public float spawnCount;
        // different mob types?
        private void Awake()
        {
            bounds = new Bounds(gameObject.transform.position, new Vector3(spawningX, 10, spawningZ));
            this.agentPrefab.SetActive(false);
        }
        void Start()
        {
            for (int i = 0; i < spawnCount; i++)
            {
                var agent = Instantiate(this.agentPrefab, GetRandomPosition(), new Quaternion(10, 10, 10, 10)).GetComponent<GoapActionProvider>();
                agent.gameObject.SetActive(true);
                NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
                NavMeshHit hit;
                navAgent.Raycast(Vector3.down, out hit);
                navAgent.Warp(hit.position);
            }
        }
        private Vector3 GetRandomPosition()
        {
            Vector2 randomPos = Random.insideUnitCircle * spawningX;
            return transform.TransformPoint(new Vector3(randomPos.x, -1, randomPos.y));
        }
    }

}