
using System;
using System.Collections;
using UnityEngine;

namespace SIGGD.Mobs
{
    public class AgentHuntBehaviour : MonoBehaviour
    {

        // Use this for initialization
        Smell smell;
        public bool engagedInHunt;
        public bool inDanger;
        public GameObject currentTargetOfHunt;
        void Start() {
        }

        // Update is called once per frame
        /*
        void Update()
        {
            if (smell.sensed && !inDanger)
            {
                if (smell.targetRef.getHierachialRank > .baseStateConfig + margin) {
                    inDanger = true;
                    engagedInHunt = false;
                }
                   // if stronger 
                // if weaker ignore
                engagedInHunt == false &&
                // goap goal?

            }
            if (engagedInHunt == true)
            {

            }
        }
        */
        public void SetHuntTarget(GameObject target)
        {
            if (currentTargetOfHunt != null)
                currentTargetOfHunt.GetComponent<EntityHealthManager>().OnDeath -= FinishHunt;
            currentTargetOfHunt = target;
            currentTargetOfHunt.GetComponent<EntityHealthManager>().OnDeath += FinishHunt;
        }
        private void FixedUpdate()
        {
            Debug.Log($"real pos{gameObject.transform.position}");
        }
        private void FinishHunt()
        {
            currentTargetOfHunt = null;
        }
    }
}
