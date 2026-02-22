using System;
using UnityEngine;

namespace MobCensus
{
    [Serializable]
    public class MobCitizenData
    {
        [SerializeField] GameObject prefab;
        [SerializeField] GameObject instance;
        [SerializeField] MobCitizenPassport passport;
        [SerializeField] MobCitizenDataRaw rawData;

        public MobCitizenData(GameObject prefab, GameObject instance, string mobId, Boundary boundary)
        {
            this.prefab = prefab;
            this.instance = instance;

            rawData = new MobCitizenDataRaw();
            rawData.SetMobId(mobId);
            rawData.SetBoundary(boundary);
            passport = instance.GetComponent<MobCitizenPassport>();
            //passport.WriteMobCitizenData();
        }
        public MobCitizenPassport GetPassport()
        {
            return passport;
        }
        public void UpdateRawData()
        {
            passport.WriteMobCitizenData();
        }
        public MobCitizenDataRaw GetRawData()
        {
            return rawData;
        }
    }
}