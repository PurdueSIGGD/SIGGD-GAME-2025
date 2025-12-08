using System.Collections.Generic;
using UnityEngine;

namespace Census
{
    public class MobCensus : MonoBehaviour
    {
        [SerializeField] List<MobCitizenData> citizens = new List<MobCitizenData>();
        public List<MobCitizenData> GetCitizens() { return citizens; }
        public void RegisterCitizen(GameObject prefab, GameObject instance)
        {
            citizens.Add(new MobCitizenData(prefab, instance));
        }
        public void RemoveCitizen(MobCitizenData targetCitizen)
        {
            citizens.Remove(targetCitizen);
        }
    }
}
