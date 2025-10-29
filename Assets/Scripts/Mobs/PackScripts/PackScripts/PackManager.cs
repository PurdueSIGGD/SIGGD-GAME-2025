using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Assertions.Must;

namespace SIGGD.Mobs.PackScripts
{
    public class PackManager : MonoBehaviour
    {
        [SerializeField] List<PackData> packs = new List<PackData>();
        public PackData JoinPacks(PackBehavior q, PackBehavior p)
        {
            if (q.GetPack() != null) q.GetPack().Lock();
            if (p.GetPack() != null) p.GetPack().Lock();

            PackData resultingPack = null;
            if (q.GetPack() == null && p.GetPack() == null) // neither mob has a pack
            {
                // print("JOIN: empty to empty");
                resultingPack = CreatePack(q, p);
            }
            else if (q.GetPack() == null && p.GetPack() != null) // p already is a pack
            {
                // print("JOIN: pack to empty");
                p.GetPack().AddToPack(q);
                resultingPack = p.GetPack();
            }
            else if (q.GetPack() != null && p.GetPack() == null) // q already is a pack
            {
                // print("JOIN: pack to empty");
                q.GetPack().AddToPack(p);
                resultingPack = q.GetPack();
            }
            else if (q.GetPack() != null && p.GetPack() != null) // q and p are packed up
            {
                // print("JOIN: merge");
                resultingPack = MergePacks(q.GetPack(), p.GetPack()); // merge packs automatically updates the pack of each agent
            }

            if (q.GetPack() != null) q.GetPack().Unlock();
            if (p.GetPack() != null) p.GetPack().Unlock();
            return resultingPack;
        }
        public void LeavePack(PackBehavior q)
        {
            PackData myPack = q.GetPack();
            if (myPack == null) return;

            myPack.Lock();
            myPack.RemoveFromPack(q);
            myPack.Unlock();
        }

        PackData CreatePack(PackBehavior q, PackBehavior p)
        {
            List<PackBehavior> founders = new List<PackBehavior>() { q, p };
            PackData newPack = new PackData(founders, max_members: q.Data.MaxPackSize);
            packs.Add(newPack);
            newPack.SetDisbandMethod(packs.Remove); // allows PackData objects to automatically remove themselves when disbanding
            q.SetPack(newPack);
            p.SetPack(newPack);
            return newPack;
        }
        PackData CreatePack(List<PackBehavior> founders)
        {
            PackData newPack = new PackData(founders, max_members: founders[0].Data.MaxPackSize);
            packs.Add(newPack);
            newPack.SetDisbandMethod(packs.Remove); // allows PackData objects to automatically remove themselves when disbanding
            foreach (PackBehavior member in newPack.GetPackMembers())
            {
                member.SetPack(newPack);
            }
            newPack.UpdateAlpha();
            return newPack;
        }
        PackData MergePacks(PackData q, PackData p)
        {
            // check if packs are the same
            if (q == p)
            {
                return q;
            }

            // check if mergeable
            int mergedMaxSize = EvaluateMergeSize(q, p);
            if (mergedMaxSize < 0)
                throw new ArgumentException("PackManager.MergePacks: Invalid merge sizes, check before merging packs!"); // quit merge on failed merge size


            // add all members from one pack to the other
            PackData newPack = q.MaxSize() < p.MaxSize() ? q : p;
            PackData otherPack = q.MaxSize() < p.MaxSize() ? p : q;
            while (otherPack.GetPackMembers().Count > 0)
            {
                newPack.AddToPack(otherPack.RemoveFromPack());
            }

            // List<PackBehavior> allMembers = new List<PackBehavior>(q.GetPackMembers().Concat(p.GetPackMembers()));
            // PackData combinedPack = CreatePack(allMembers);
            // packs.Add(combinedPack);

            // q.DisbandPack();
            // p.DisbandPack();

            return newPack;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns>New merged size, OR -1 on failed merge.</returns>
        public static int EvaluateMergeSize(PackData p, PackData q)
        {
            // verify max pack sizes
            int mergedMaxSize = q.MaxSize() < p.MaxSize() ? q.MaxSize() : p.MaxSize(); // use the smaller of the max sizes

            // disallow pack merging if merged pack would be too big
            if (mergedMaxSize < q.GetSize() + p.GetSize())
                return -1;
            return mergedMaxSize;
        }

        public static bool CanJoin(PackBehavior p, PackBehavior q, bool excludePack = false)
        {
            // skip locked packs
            if (q.GetPack() != null && q.GetPack().IsLocked() ||
                p.GetPack() != null && p.GetPack().IsLocked())
            {
                // print("CanJoin: Safety Lock Fault");
                return false;
            }

            if (q.agentType != p.agentType)
            {
                // skip pack behaviors of different agent type
                // print("CanJoin: Different Type Fault");
                return false;
            }

            // skip mobs of same pack if excludePack is set to true
            if (excludePack && p.GetPack() != null && q.GetPack() == p.GetPack())
            {
                // print("CanJoin: Exclude Pack Fault");
                return false;
            }

            // skip unmergeable packs
            if (q.GetPack() != null &&
                p.GetPack() != null &&
                EvaluateMergeSize(p.GetPack(), q.GetPack()) < 0)
            {
                // print("CanJoin: Merge Size Fault");
                return false;
            }

            // skip full packs
            if ((q.GetPack() != null && q.GetPack().IsFull()) ||
                (p.GetPack() != null && p.GetPack().IsFull()))
            {
                // print("CanJoin: Full Pack Fault");
                return false;
            }

            return true;
        }

        public static bool CanLeave(PackBehavior p)
        {
            PackData myPack = p.GetPack();
            if (myPack == null) return false; // pack doesn't exist fault

            if (myPack.IsLocked())
            {
                return false; // locked pack fault
            }
            return true;
        }

    }
}
