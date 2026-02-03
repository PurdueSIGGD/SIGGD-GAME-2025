
using System;
using System.Collections;
using UnityEngine;

namespace SIGGD.Mobs
{
    public class AgentHuntBehaviour : MonoBehaviour
    {

        Smell smell;
        public bool engagedInHunt;
        public bool inDanger;
        public GameObject currentTargetOfHunt;
        void Start() {
            currentTargetOfHunt = null;
        }

        public void SatisfiedWithHunt()
        {
            
        }
        public void SetHuntTarget(GameObject target)
        {
            if (currentTargetOfHunt != null)
                EntityHealthManager.OnDeath -= FinishHunt;
            currentTargetOfHunt = target;
            EntityHealthManager.OnDeath += FinishHunt;
        }
        private void FixedUpdate()
        {
        }
        private void FinishHunt(DamageContext context)
        {
            if (context.victim == currentTargetOfHunt)
            {
                currentTargetOfHunt = null;
            }
        }
    }
}
