/*
using SIGGD.Goap.Behaviours;
using System;
using System.Collections;
using UnityEngine;

namespace SIGGD.Goap.Behaviours
{
    public class AgentHuntBehaviour : MonoBehaviour
    {

        // Use this for initialization
        Smell smell;
        public bool engagedInHunt;
        public bool inDanger;
        public GameObject currentTargetOfHunt;
        void Start()
        {
        }

        public void SetHuntTarget(GameObject target)
        {
            if (currentTargetOfHunt != null)
                currentTargetOfHunt.GetComponent<EntityHealthManager>().OnDeath -= FinishHunt;
            currentTargetOfHunt = target;
            currentTargetOfHunt.GetComponent<EntityHealthManager>().OnDeath += FinishHunt;
        }
        private void FixedUpdate()
        {

        }
        private void FinishHunt()
        {
            currentTargetOfHunt = null;
        }
    }
}
 * Pick random location that is away from enemy range and away from visible enemies,
 * recompute every couple seconds, 
 * decides or needs new location or same location
 * Safe when outside line of sight
 * if noticed, even more danger level
 * Danger level depends on amoutn of enemies, being alone, enemies being engaged, distance of enemies, power level, stamina, cover
*/