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
        private HyenaLungeBehaviour hyenaLungeBehaviour;
        private HyenaCirclingBehaviour hyenaCirclingBehaviour;
        public bool isLunging;
        private TransformTarget currentTarget;
        private Coroutine attackRoutine;


        private void Awake()
        {
            isLunging = false;
            animatorController = GetComponent<EnemyAnimator>();
            hyenaLungeBehaviour = GetComponent<HyenaLungeBehaviour>();
            hyenaCirclingBehaviour = GetComponent<HyenaCirclingBehaviour>();
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
            hyenaLungeBehaviour.exit = false;
            hyenaCirclingBehaviour.exit = false;
            Transform target = GetTarget();
            if (target == null)
            {
                isLunging = false;
                attackRoutine = null;
                yield break;
            }
            animatorController.SetLook(true);
            animatorController.SetLookTarget(target);

            yield return StartCoroutine(AttackSequence(target));

            animatorController.SetLook(false);
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
        private IEnumerator AttackSequence(Transform target)
        {
            if (hyenaCirclingBehaviour == null || hyenaLungeBehaviour == null || animatorController == null)
                yield break;
            StartCoroutine(hyenaCirclingBehaviour.CircleLoop(target));
            yield return new WaitUntil(() => hyenaCirclingBehaviour.finished || hyenaCirclingBehaviour.exit);
            if (hyenaCirclingBehaviour.exit) yield break; // stop sequence
            StartCoroutine(hyenaLungeBehaviour.Lunge(target));
            animatorController.SetLungeModel(); // set hyena model
            yield return new WaitUntil(() => hyenaLungeBehaviour.lungeArriving || hyenaLungeBehaviour.exit);
            if (hyenaLungeBehaviour.exit) yield break; // stop sequence
            Debug.Log($"{gameObject.name} has begun attack animation");
            animatorController.PlayAttack();
            yield return new WaitUntil(() => hyenaLungeBehaviour.finishedLunging || hyenaLungeBehaviour.exit);
            animatorController.EndAttack();
            if (hyenaLungeBehaviour.exit) yield break; // stop sequence
            StartCoroutine(hyenaLungeBehaviour.ExitLunge(target));
            yield return new WaitUntil(() => hyenaLungeBehaviour.finishedExiting || hyenaLungeBehaviour.exit); 
            if (hyenaLungeBehaviour.exit) yield break; // stop sequence
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

        public Transform GetTarget() => this.currentTarget != null ? this.currentTarget.Transform : null;
        public void CancelAttack()
        {
            Transform currTarget = GetTarget();
            if (currTarget != null && Vector3.Distance(currentTarget.Transform.position, transform.position) < 20f) return;
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            if (hyenaCirclingBehaviour != null)
                hyenaCirclingBehaviour.ExitBehaviour();

            if (hyenaLungeBehaviour != null)
                hyenaLungeBehaviour.ExitBehaviour(); // and/or add an ExitBehaviour there too
            isLunging = false;
            currentTarget = null;
        }
    }
}