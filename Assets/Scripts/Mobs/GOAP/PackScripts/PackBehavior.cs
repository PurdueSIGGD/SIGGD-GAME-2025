using System;
using UnityEngine;

namespace SIGGD.Goap.PackScripts
{
    public class PackBehavior : MonoBehaviour
    {
        PackData myPack = null;
        [SerializeField] int powerLevel; // dummy value for now since powerLevel implementation is not done yet
        bool packAlpha;

        const int MINPOWER = 0;
        const int MAXPOWER = 99;

        void Start()
        {
            powerLevel = UnityEngine.Random.Range(MINPOWER, MAXPOWER);
        }
        public int GetPowerLevel()
        {
            return powerLevel;
        }
        public PackData GetPack()
        {
            return myPack;
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
    }
}