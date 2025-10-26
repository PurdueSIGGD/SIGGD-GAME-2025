using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Goap;
using Utility;
namespace SIGGD.Mobs.Hyena
{
    public class HyenaCirclingBehaviour : MonoBehaviour
    {
        public float attackInterval;
        public float telegraph;
        public float beginningAttackCooldown;
        public float attackOffset;
        private Rigidbody rb;
        private NavMeshAgent NavMeshAgent;
        private AgentMoveBehaviour AgentMoveBehaviour;
        private AgentHuntBehaviour AgentHuntBehaviour;
        public bool finishedWalking;
        public bool finishedCircling;

        Animator animator;
        private void Awake()
        {
            finishedCircling = false;
            finishedWalking = false;
            rb = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            AgentMoveBehaviour = GetComponent<AgentMoveBehaviour>();
            AgentHuntBehaviour = GetComponent<AgentHuntBehaviour>();

        }
        void Start()
        {
            beginningAttackCooldown = 0f;
        }


        public IEnumerator Circle(Func<Vector3> GetTarget)
        {
            finishedCircling = false;
            float duration = UnityEngine.Random.Range(3f, 7f);
            float elapsed = 0f;

            Vector3 direction = new Vector3(0, UnityEngine.Random.Range(-1, 1) > 0 ? 1 : -1, 0);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 next = Vector3.Cross((GetTarget() - transform.position).normalized, direction);
                float offset = UnityEngine.Random.Range(0.45f, 0.55f);
                if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
                    Pathfinding.MovePartialPath(NavMeshAgent, next * offset * 30, Time.deltaTime * 50 * 100);

                yield return new WaitForFixedUpdate(); 
            }
            Debug.Log("stopped circling");
            finishedCircling = true;
        }
        public IEnumerator WalkTowardsTarget(Func<Vector3> GetTarget)
        {

            finishedWalking = false;
            float duration = UnityEngine.Random.Range(1f, 2f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
                    Pathfinding.MovePartialPath(NavMeshAgent, GetTarget(), Time.deltaTime * 35 * 100);

                yield return new WaitForFixedUpdate();
            }
            NavMeshAgent.isStopped = true;
            Debug.Log("stopped walking");
            yield return new WaitUntil(() => NavMeshAgent.pathPending != true);
            finishedWalking = true;
        }
    }
}
