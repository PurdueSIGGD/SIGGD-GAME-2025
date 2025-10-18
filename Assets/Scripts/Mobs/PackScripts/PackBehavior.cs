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
    public class PackBehavior : MonoBehaviour
    {
        PackManager packManager;
        GoapActionProvider provider;
        public IAgentType agentType { get; private set; }
        PackData myPack = null;
        int powerLevel; // dummy value for now since powerLevel implementation is not done yet
        const int MINPOWER = 0;
        const int MAXPOWER = 99;
        [SerializeField] public PackBehaviorData Data { get; }

        void Start()
        {
            packManager = FindFirstObjectByType<PackManager>().GetComponent<PackManager>();
            powerLevel = UnityEngine.Random.Range(MINPOWER, MAXPOWER);
            agentType = provider.AgentType;
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
        public PackBehavior FindNearbyNeighbor(int numSearchRingSplits)
        {
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
                    if (otherPack.agentType != this.agentType) continue;

                    // just straight up return the first thing you find for performance
                    return otherPack;
                }
            }
            // didn't find any enemy
            return null;
        }
        public Collider[] LookForMobs(float radius)
        {
            return Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Mobs"));
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
        public static Vector3 CalculateDistance(PackBehavior a, PackBehavior b)
        {
            // can be replaced with a nav system based calculation in the future
            return a.gameObject.transform.position - b.gameObject.transform.position;
        }
    }
}