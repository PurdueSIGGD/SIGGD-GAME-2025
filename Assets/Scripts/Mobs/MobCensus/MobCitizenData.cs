using System;
using UnityEngine;

namespace Census
{
    [Serializable]
    public class MobCitizenData
    {
        [SerializeField] GameObject prefab;
        [SerializeField] GameObject instance;
        [SerializeField] MobCitizenPassport passport;
        [SerializeField] MobCitizenDataRaw rawData;

        public MobCitizenData(GameObject prefab, GameObject instance)
        {
            this.prefab = prefab;
            this.instance = instance;
            this.passport = instance.GetComponent<MobCitizenPassport>();
            this.rawData = new MobCitizenDataRaw();
        }

        public MobCitizenDataRaw GetRawDataReference()
        {
            return rawData;
        }
    }
}