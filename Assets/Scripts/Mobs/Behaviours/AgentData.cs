
/*
using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Mobs.GOAP.Behaviours
{
    public class AgentData : MonoBehaviour
    {

        // Use this for initialization
        public int desperationLevel;
        public int powerLevel;
        public int energyLevel;
        public int aggressionLevel;
        void Start()
        {
            aggressionLevel = 1;
            powerLevel = 1;
            energyLevel = 1;
            aggressionLevel = 1;
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void IncreaseEnergy(int amount)
        {
            energyLevel = Mathf.Clamp(energyLevel + amount, 0, maxEnergy);
        }
        public void DecreaseEnergy(int amount)
        {
            energyLevel = Mathf.Clamp(energyLevel - amount, 0, maxEnergy);
        }
        public void IncreasePower(int amount)
        {
            powerLevel = Mathf.Clamp(powerLevel + amount, 0, maxPower);
        }
        public void DecreasePower(int amount)
        {
            powerLevel = Mathf.Clamp(powerLevel - amount, 0, maxPower);
        }
        public void IncreaseDesperation(int amount)
        {
            desperationLevel = Mathf.Clamp(desperationLevel + amount, 0, maxDesperation);
        }
        public void DecreaseDesperation(int amount)
        {
            desperationLevel = Mathf.Clamp(desperationLevel - amount, 0, maxDesperation);
        }
        public void IncreaseAggression(int amount)
        {
            aggressionLevel = Mathf.Clamp(aggressionLevel + amount, 0, maxAggression);
            
        }
        public void DecreaseAggression(int amount)
        {
            aggressionLevel = Mathf.Clamp(aggressionLevel - amount, 0, maxAggression);
        }
    }
}
*/