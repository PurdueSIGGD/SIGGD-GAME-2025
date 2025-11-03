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
            Debug.Log("starting");
            if (isLunging) return;
            try
            {
                StartCoroutine(AttackSequenceWrapper());
            }
            catch (Exception e)
            {
                Debug.Log(e);
                isLunging = false;
            }

        }

        private IEnumerator AttackSequenceWrapper()
        {
            isLunging = true;
            HyenaLungeBehaviour.exit = false;
            HyenaCirclingBehaviour.exit = false;

            yield return StartCoroutine(AttackSequence());

            isLunging = false;
        }
        private IEnumerator AttackSequence()
        {
            StartCoroutine(HyenaCirclingBehaviour.CircleLoop(GetTarget));
            yield return new WaitUntil(() => HyenaCirclingBehaviour.finished || HyenaCirclingBehaviour.exit);
            if (HyenaCirclingBehaviour.exit) yield break;
            StartCoroutine(HyenaLungeBehaviour.Lunge(GetTarget));
            animatorController.SetLungeModel();
            yield return new WaitUntil(() => HyenaLungeBehaviour.lungeArriving || HyenaLungeBehaviour.exit);
            if (HyenaLungeBehaviour.exit) yield break;
            Debug.Log($"{gameObject.name} has begun attack animation");
            animatorController.PlayAttack();
            yield return new WaitUntil(() => HyenaLungeBehaviour.finishedLunging || HyenaLungeBehaviour.exit);
            if (HyenaLungeBehaviour.exit) yield break;
            StartCoroutine(HyenaLungeBehaviour.ExitLunge(GetTarget));
            yield return new WaitUntil(() => HyenaLungeBehaviour.finishedExiting || HyenaLungeBehaviour.exit);
            if (HyenaLungeBehaviour.exit) yield break;
        }
        public void SetTarget(TransformTarget target)
        {
            this.currentTarget = target;
        }
        public Vector3 GetTarget() => this.currentTarget != null ? this.currentTarget.Position : new Vector3(0,0,0);
    }
}