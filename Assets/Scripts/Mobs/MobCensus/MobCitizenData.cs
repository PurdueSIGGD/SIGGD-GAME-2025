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

        public MobCitizenData(GameObject prefab, GameObject instance, string mobId)
        {
            this.prefab = prefab;
            this.instance = instance;

            rawData = new MobCitizenDataRaw();
            rawData.SetMobId(mobId);
            passport = instance.GetComponent<MobCitizenPassport>();
            passport.WriteMobCitizenData();
        }
        public MobCitizenPassport GetPassport()
        {
            return passport;
        }
        public MobCitizenDataRaw GetRawDataReference()
        {
            return rawData;
        }
    }
}