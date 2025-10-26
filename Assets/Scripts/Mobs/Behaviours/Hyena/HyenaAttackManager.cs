using CrashKonijn.Agent.Runtime;
using System.Collections;
using UnityEngine;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System;

namespace SIGGD.Mobs.Hyena
{
    public class HyenaAttackManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private EnemyAnimator animatorController;
        private HyenaLungeBehaviour HyenaLungeBehaviour;
        private HyenaCirclingBehaviour HyenaCirclingBehaviour;
        public bool isLunging;
        private TransformTarget currentTarget;

        private void Awake()
        {
            isLunging = false;
            animatorController = GetComponent<EnemyAnimator>();
            HyenaLungeBehaviour = GetComponent<HyenaLungeBehaviour>();
            HyenaCirclingBehaviour = GetComponent<HyenaCirclingBehaviour>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void StartAttackSequence(IMonoAgent agent)
        {
            Debug.Log("starting lunge");
            try
            {
                isLunging = true;
                StartCoroutine(AttackSequence());
            }
            catch (Exception e)
            {
                Debug.Log(e);
            } finally {
                isLunging = false;
            }

        }

        private IEnumerator AttackSequence()
        {
            StartCoroutine(HyenaCirclingBehaviour.Circle(GetTarget));
            yield return new WaitUntil(() => HyenaCirclingBehaviour.finishedCircling);
            StartCoroutine(HyenaCirclingBehaviour.WalkTowardsTarget(GetTarget));
            yield return new WaitUntil(() => HyenaCirclingBehaviour.finishedWalking);
            StartCoroutine(HyenaLungeBehaviour.Lunge(GetTarget));
            animatorController.SetLungeModel();
            yield return new WaitUntil(() => HyenaLungeBehaviour.lungeArriving);
            Debug.Log($"{gameObject.name} has begun attack animation");
            animatorController.PlayAttack();
            yield return new WaitUntil(() => HyenaLungeBehaviour.finished);
            isLunging = false;
        }
        public void SetTarget(TransformTarget target)
        {
            this.currentTarget = target;
            Debug.Log("set new target");
        }
        public Vector3 GetTarget() => this.currentTarget != null ? this.currentTarget.Position : new Vector3(0,0,0);
    }

}