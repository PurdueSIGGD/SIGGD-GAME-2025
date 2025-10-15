using UnityEngine;
using System.Collections.Generic;

namespace SIGGD.Goap.PackScripts
{
    public class PackManager : MonoBehaviour
    {
        [SerializeField] List<PackData> packs = new List<PackData>();
        public void CreatePack(PackBehavior q, PackBehavior p)
        {
            List<PackBehavior> founders = new List<PackBehavior>() { q, p };
            PackData newPack = new PackData(founders);
            packs.Add(newPack);
            newPack.SetDisbandMethod(packs.Remove); // allows PackData objects to automatically remove themselves when disbanding
        }

        public void JoinPacks(PackBehavior q, PackBehavior p)
        {
            if (q.GetPack() == null && p.GetPack() == null) // neither mob has a pack
            {
                CreatePack(q, p);
            }
            else if (q.GetPack() == null && p.GetPack() != null) // p already is a pack
            {
                p.GetPack().AddToPack(q);
            }
            else if (q.GetPack() != null && p.GetPack() == null) // q already is a pack
            {
                q.GetPack().AddToPack(p);
            }
            else if (q.GetPack() != null && p.GetPack() != null) // q and p are packed up
            {
                PackData.MergePacks(q.GetPack(), p.GetPack());
            }
        }
    }
}
