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
        private EnemyAnimator animatorController;
        private HyenaLungeBehaviour HyenaLungeBehaviour;
        private HyenaCirclingBehaviour HyenaCirclingBehaviour;
        public bool isLunging;
        private TransformTarget currentTarget;
        private Coroutine attackRoutine;


        private void Awake()
        {
            isLunging = false;
            animatorController = GetComponent<EnemyAnimator>();
            HyenaLungeBehaviour = GetComponent<HyenaLungeBehaviour>();
            HyenaCirclingBehaviour = GetComponent<HyenaCirclingBehaviour>();
        }

        void Update()
        {

        }
        public void StartAttackSequence(IMonoAgent agent)
        {
            if (isLunging) return;
            attackRoutine = StartCoroutine(AttackSequenceWrapper());

        }
         
        private IEnumerator AttackSequenceWrapper()
        {
            isLunging = true;
            HyenaLungeBehaviour.exit = false;
            HyenaCirclingBehaviour.exit = false;

            yield return StartCoroutine(AttackSequence());

            isLunging = false;
            attackRoutine = null;
        }

        /**
         * Begins the attack sequence
         * 1. Attempt circling
         * 2. Lunging
         * 3. Changing hyena model
         * 4. Exiting the attack
         */
        private IEnumerator AttackSequence()
        {
            StartCoroutine(HyenaCirclingBehaviour.CircleLoop(GetTarget));
            yield return new WaitUntil(() => HyenaCirclingBehaviour.finished || HyenaCirclingBehaviour.exit);
            if (HyenaCirclingBehaviour.exit) yield break; // stop sequence
            StartCoroutine(HyenaLungeBehaviour.Lunge(GetTarget));
            animatorController.SetLungeModel(); // set hyena model
            yield return new WaitUntil(() => HyenaLungeBehaviour.lungeArriving || HyenaLungeBehaviour.exit);
            if (HyenaLungeBehaviour.exit) yield break; // stop sequence
            Debug.Log($"{gameObject.name} has begun attack animation");
            animatorController.PlayAttack();
            yield return new WaitUntil(() => HyenaLungeBehaviour.finishedLunging || HyenaLungeBehaviour.exit);
            if (HyenaLungeBehaviour.exit) yield break; // stop sequence
            StartCoroutine(HyenaLungeBehaviour.ExitLunge(GetTarget));
            yield return new WaitUntil(() => HyenaLungeBehaviour.finishedExiting || HyenaLungeBehaviour.exit); 
            if (HyenaLungeBehaviour.exit) yield break; // stop sequence
        }
        /// <summary>
        /// Sets the current target
        /// </summary>
        /// <param name="target"> The TransformTarget which is a reference </param>
        public void SetTarget(TransformTarget target)
        {
            this.currentTarget = target;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns> Returns Vector3.zero if currentTarget is null otherwise return the current target's position </returns>
        /// 

        public Vector3 GetTarget() => this.currentTarget != null ? this.currentTarget.Position : Vector3.zero;
        public void CancelAttack()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            if (HyenaCirclingBehaviour != null)
                HyenaCirclingBehaviour.ExitBehaviour();

            if (HyenaLungeBehaviour != null)
                HyenaLungeBehaviour.ExitBehaviour(); // and/or add an ExitBehaviour there too
            isLunging = false;
            currentTarget = null;
        }
    }
}