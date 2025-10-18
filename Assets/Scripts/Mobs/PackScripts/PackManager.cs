using UnityEngine;
using System.Collections.Generic;

namespace SIGGD.Mobs.PackScripts
{
    public class PackManager : MonoBehaviour
    {
        [SerializeField] List<PackData> packs = new List<PackData>();
        public PackData CreatePack(PackBehavior q, PackBehavior p)
        {
            List<PackBehavior> founders = new List<PackBehavior>() { q, p };
            PackData newPack = new PackData(founders);
            packs.Add(newPack);
            newPack.SetDisbandMethod(packs.Remove); // allows PackData objects to automatically remove themselves when disbanding
            q.SetPack(newPack);
            p.SetPack(newPack);
            return newPack;
        }

        public PackData JoinPacks(PackBehavior q, PackBehavior p)
        {
            print("Joining two packs!");
            PackData resultingPack = null;
            if (q.GetPack() == null && p.GetPack() == null) // neither mob has a pack
            {
                resultingPack = CreatePack(q, p);
            }
            else if (q.GetPack() == null && p.GetPack() != null) // p already is a pack
            {
                p.GetPack().AddToPack(q);
                resultingPack = p.GetPack();
            }
            else if (q.GetPack() != null && p.GetPack() == null) // q already is a pack
            {
                q.GetPack().AddToPack(p);
                resultingPack = q.GetPack();
            }
            else if (q.GetPack() != null && p.GetPack() != null) // q and p are packed up
            {
                resultingPack = PackData.MergePacks(q.GetPack(), p.GetPack()); // merge packs automatically updates the pack of each agent
            }
            return resultingPack;
        }
    }
}
