using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Behaviours;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Mobs.PackScripts
{
    [Serializable]
    public class PackBehavior : MonoBehaviour
    {
        PackManager packManager;
        GoapActionProvider provider;
        public IAgentType agentType { get; private set; } = null;
        PackData myPack = null;
        [SerializeField] int powerLevel; // dummy value for now since powerLevel implementation is not done yet
        const int MINPOWER = 0;
        const int MAXPOWER = 99;
        [SerializeField] public PackBehaviorData Data;

        void Start()
        {
            packManager = FindFirstObjectByType<PackManager>().GetComponent<PackManager>();
            powerLevel = UnityEngine.Random.Range(MINPOWER, MAXPOWER);
            agentType = provider.AgentType;
        }
        void Update()
        {
            // validate membership to current pack via distance checking
            if (myPack != null)
            {
                float dist = CalculateDistanceVector(myPack.GetClosestMember(this.transform.position), this).magnitude;
                if (dist > Data.LeavePackRange)
                {
                    LeavePack();
                }
            }
        }
        public int GetPowerLevel()
        {
            return powerLevel;
        }
        public PackData GetPack()
        {
            return myPack;
        }
        public void JoinPack(PackBehavior other)
        {
            packManager.JoinPacks(this, other);
        }
        public void SetPack(PackData newPack)
        {
            // verify this script is in the pack
            if (!newPack.Contains(this))
            {
                throw new ArgumentException("PackBehavior.SetPack: newPack does not contain this mob, cannot set as myPack!");
            }
            myPack = newPack;
        }
        public void LeavePack()
        {
            myPack.RemoveFromPack(this);
            myPack = null;
        }
        public PackBehavior FindNearbyNeighbor()
        {
            int numSearchRingSplits = Data.NumSearchRingSplits;
            float maxSearchRing = Data.AgentTypeVisionRange;
            float searchIncrement = maxSearchRing / numSearchRingSplits;

            // search for packbehavior entities in larger and larger rings
            for (int i = 0; i < numSearchRingSplits; i++)
            {
                // LOS abstraction
                Collider[] hits = LookForMobs(i * searchIncrement);
                if (hits.Length == 0) continue;
                foreach (Collider hit in hits)
                {
                    PackBehavior otherPack = hit.gameObject.GetComponent<PackBehavior>();
                    if (otherPack.gameObject == this.gameObject) continue;
                    if (otherPack.agentType != this.agentType) continue; // skip pack behaviors of different agent type

                    // just straight up return the first thing you find for performance
                    return otherPack;
                }
            }
            // didn't find any enemy
            return null;
        }
        public Collider[] LookForMobs(float radius)
        {
            return Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Mob"));
        }
        public Vector3 GetRawAlphaPositionDiff()
        {
            return myPack.GetAlpha().gameObject.transform.position - this.transform.position;
        }
        public int GetCloseToAlphaKey()
        {
            if (myPack == null) return 0;
            return GetRawAlphaPositionDiff().magnitude <= Data.CloseEnoughToAlphaDist ? 1 : 0;
        }
        public int GetIsAlphaKey()
        {
            if (myPack == null) return 0;
            return myPack.GetAlpha() == this ? 1 : 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>A Vector3 pointint in the direction to get from B to A.</returns>
        public static Vector3 CalculateDistanceVector(PackBehavior a, PackBehavior b)
        {
            // can be replaced with a nav system based calculation in the future
            return a.gameObject.transform.position - b.gameObject.transform.position;
        }
    }
}