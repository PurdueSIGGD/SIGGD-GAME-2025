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
        PackBehavior lastNearesNeighbor;

        void Start()
        {
            packManager = FindFirstObjectByType<PackManager>().GetComponent<PackManager>();
            powerLevel = UnityEngine.Random.Range(MINPOWER, MAXPOWER);
            agentType = provider.AgentType;
        }
        void FixedUpdate()
        {
            // validate membership to current pack via distance checking
            if (myPack != null)
            {
                // PackBehavior neighbor = FindNearbyNeighbor(excludePack: false, specificPack: myPack);
                PackBehavior neighbor = myPack.GetAlpha();
                if (neighbor != null)
                {
                    if (CheckLeaveRange(neighbor))
                    {
                        TryLeavePack();
                    }
                }
            }
        }
        void OnDestroy()
        {
            ForceLeavePack();
        }
        public int GetPowerLevel()
        {
            return powerLevel;
        }
        public PackData GetPack()
        {
            return myPack;
        }
        public void TryJoinPack(PackBehavior other)
        {
            PackData newPack = packManager.JoinPacks(this, other);
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
        public void TryLeavePack()
        {
            if (PackManager.CanLeave(this))
            {
                packManager.LeavePack(this);
            }
        }
        public void ForceLeavePack()
        {
            myPack.RemoveFromPack(this);
            myPack = null;
        }
        public PackBehavior FindNearbyNeighbor(bool excludePack = false, PackData specificPack = null)
        {
            int numSearchRingSplits = Data.NumSearchRingSplits;
            float maxSearchRing = Data.AgentTypeVisionRange;
            float searchIncrement = maxSearchRing / numSearchRingSplits;

            // search for packbehavior entities in larger and larger rings
            for (int i = 0; i < numSearchRingSplits; i++)
            {
                // LOS abstraction
                Collider[] hits = LookForMobs(
                    transform.position,
                    i * searchIncrement);

                if (hits.Length == 0) continue;
                foreach (Collider hit in hits)
                {
                    PackBehavior otherPack = hit.gameObject.GetComponent<PackBehavior>();
                    if (otherPack == null) continue;
                    if (otherPack == this) continue; // skip self
                    if (specificPack != null && otherPack.GetPack() != specificPack) continue; // skip if mob not in specific pack we're looking for
                    if (PackManager.CanJoin(this, otherPack, excludePack: excludePack))
                        lastNearesNeighbor = otherPack;
                    return otherPack; // just straight up return the first thing you find for performance
                }
            }
            // didn't find any enemy
            lastNearesNeighbor = null;
            return null;
        }
        public PackBehavior GetLastNearestNeighbor()
        {
            return lastNearesNeighbor;
        }
        public Collider[] LookForMobs(Vector3 position, float radius)
        {
            return Physics.OverlapSphere(position, radius, LayerMask.GetMask("Mob"));
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
        public bool IsHappyWithPack()
        {
            if (myPack == null)
            {
                return false;
            }
            return myPack.GetSize() == Data.MaxPackSize;
        }
        public bool CheckLeaveRange(PackBehavior other)
        {
            float dist = CalculateDistanceVector(this, other).magnitude;
            return dist >= Data.LeavePackRange;
        }
        public bool CheckLeaveRange(Vector3 position)
        {
            float dist = CalculateDistanceVector(
                this.gameObject.transform.position,
                position).magnitude;
            return dist >= Data.LeavePackRange;
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
        public static Vector3 CalculateDistanceVector(Vector3 a, Vector3 b)
        {
            // can be replaced with a nav system based calculation in the future
            return a - b;
        }
    }
}