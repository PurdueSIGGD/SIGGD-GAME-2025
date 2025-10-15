using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using System.Linq;
using CrashKonijn.Goap.Core;

namespace SIGGD.Goap.PackScripts
{
    public class PackData
    {
        IAgentType agentType;
        List<PackBehavior> packMembers = new List<PackBehavior>();
        PackBehavior packAlpha = null;
        int MAX_MEMBERS;
        bool packFull = false;
        Func<PackData, bool> disbandMethod; // set to 'remove from pack list' by PackManager

        public PackData(List<PackBehavior> starterMembers, int max_members = int.MaxValue)
        {
            if (starterMembers.Count == 0)
                throw new ArgumentException("PackData Constructor: starterMembers cannot be empty!!!");
            if (starterMembers.Count > max_members)
                throw new ArgumentException("PackData Constructor: Length of starterMembers exceeds maximum member count - validate before creating a PackData!");

            this.packMembers = new List<PackBehavior>(starterMembers);
            this.packAlpha = CalculateAlpha(packMembers);
            this.agentType = packAlpha.agentType;
        }

        public void AddToPack(PackBehavior newMember)
        {
            if (packFull)
                throw new ArgumentException("PackData.AddToPack: Pack is full, cannot add anymore members, use IsFull() to verify pack emptiness.");

            packMembers.Add(newMember);

            if (packMembers.Count == MAX_MEMBERS)
            {
                packFull = true;
            }
        }

        public void RemoveFromPack(PackBehavior removedMember)
        {
            packMembers.Remove(removedMember);
            if (packMembers.Count < MAX_MEMBERS)
            {
                packFull = false;
            }
            if (packMembers.Count <= 1)
            {
                // disband pack if only one member remaining
                DisbandPack();
            }
        }
        public void SetDisbandMethod(Func<PackData, bool> disbandMethod)
        {
            this.disbandMethod = disbandMethod;
        }
        public void DisbandPack()
        {
            disbandMethod(this);
            packMembers.Clear();
        }

        public PackBehavior CalculateAlpha(List<PackBehavior> members)
        {
            PackBehavior tempAlpha = null;
            foreach (PackBehavior member in members)
            {
                if (tempAlpha == null || member.GetPowerLevel() > tempAlpha.GetPowerLevel())
                {
                    tempAlpha = member;
                }
            }
            return tempAlpha;
        }

        /// <summary>
        /// Updates the value of packAlpha
        /// </summary>
        /// <returns>True if alpha was changed, false if remained the same.</returns>
        public bool UpdateAlpha()
        {
            PackBehavior newAlpha = CalculateAlpha(packMembers);
            if (packAlpha == null || newAlpha != packAlpha)
            {
                packAlpha = newAlpha;
                return true;
            }
            return false;
        }

        public PackBehavior GetClosestMember(Vector3 position)
        {
            PackBehavior tempClosest = null;
            float closestDist = float.MaxValue;
            foreach (PackBehavior member in packMembers)
            {
                float dist = (member.gameObject.transform.position - position).magnitude;
                if (dist < closestDist)
                {
                    tempClosest = member;
                    closestDist = dist;
                }
            }
            return tempClosest;
        }
        public Vector3 GetCentroid()
        {
            Vector3 sumPositions = new Vector3();
            foreach (PackBehavior member in packMembers)
            {
                sumPositions += member.gameObject.transform.position;
            }
            return sumPositions / packMembers.Count;
        }
        public bool Contains(PackBehavior p)
        {
            return packMembers.FirstOrDefault(e => e == p) == null;
        }
        public List<PackBehavior> GetPackMembers()
        {
            return packMembers;
        }
        public PackBehavior GetAlpha()
        {
            return packAlpha;
        }
        public int MaxSize()
        {
            return MAX_MEMBERS;
        }
        public bool IsFull()
        {
            return packFull;
        }

        public static PackData MergePacks(PackData q, PackData p)
        {
            // check if packs are the same
            if (q == p)
            {
                return q;
            }

            // verify max pack sizes
            int mergedMaxSize = q.MaxSize() < p.MaxSize() ? q.MaxSize() : p.MaxSize(); // use the smaller of the max sizes

            // disallow pack merging if merged pack would be too big
            if (mergedMaxSize < q.MaxSize() + p.MaxSize())
            {
                return null;
            }
            List<PackBehavior> combined = new List<PackBehavior>(q.GetPackMembers().Concat(p.GetPackMembers()));
            PackData newPack = new PackData(combined);
            foreach (PackBehavior member in newPack.GetPackMembers())
            {
                member.SetPack(newPack);
            }
            return newPack;
        }
    }
}