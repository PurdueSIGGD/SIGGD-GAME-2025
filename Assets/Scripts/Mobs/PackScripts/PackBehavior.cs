using System;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Behaviours;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Goap.PackScripts
{
    public class PackBehavior : MonoBehaviour
    {

        GoapActionProvider provider;
        public IAgentType agentType { get; private set; }
        PackData myPack = null;
        [SerializeField] int powerLevel; // dummy value for now since powerLevel implementation is not done yet
        const int MINPOWER = 0;
        const int MAXPOWER = 99;
        [SerializeField] float packVisionRange; // maximum detectable distance for mobs of the same type 

        [Header("Pack Behavior World Key References")]
        [SerializeField] int distanceFromAlpha;
        [SerializeField] int isAlpha = 0;

        void Start()
        {
            powerLevel = UnityEngine.Random.Range(MINPOWER, MAXPOWER);
            agentType = provider.AgentType;
        }
        void Update()
        {
            UpdateKeys();
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
            UpdateKeys();
        }
        public void UpdateKeys()
        {
            if (myPack == null) return;
            distanceFromAlpha = (int)Mathf.Floor(
                (myPack.GetAlpha().gameObject.transform.position - this.transform.position).magnitude // floor the magnitude of difference in position
            );
            isAlpha = myPack.GetAlpha() == this ? 1 : 0;
        }
        public int GetDistanceKey()
        {
            if (myPack == null) return -1;
            return distanceFromAlpha;
        }
        public int GetIsAlphaKey()
        {
            if (myPack == null) return 0;
            return isAlpha;
        }
    }
}