using CrashKonijn.Agent.Runtime;
using System.Collections;
using UnityEngine;

namespace SIGGD.Goap.Behaviours
{
    public class HyenaAttackManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private EnemyAnimator animatorController;
        private HyenaLungeBehaviour HyenaLungeBehaviour;
        public bool isLunging;

        private void Awake()
        {
            isLunging = false;
            animatorController = GetComponent<EnemyAnimator>();
            HyenaLungeBehaviour = GetComponent<HyenaLungeBehaviour>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void StartAttackSequence(Vector3 target)
        {
            isLunging = true;
            StartCoroutine(AttackSequence(target));
        }

        private IEnumerator AttackSequence(Vector3 target)
        {
            StartCoroutine(HyenaLungeBehaviour.Lunge(target));
            yield return new WaitUntil(() => HyenaLungeBehaviour.finished);
            animatorController.PlayAttack();
            isLunging = false;

        }
    }

}