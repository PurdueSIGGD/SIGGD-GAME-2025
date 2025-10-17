using UnityEngine;

namespace SIGGD.Goap.Config
{
    [CreateAssetMenu(fileName = "HungerConfigSO", menuName = "Scriptable Objects/HungerConfigSO")]
    public class HungerConfigSO : ScriptableObject
    {
        [Header("Hunger Settings")]
        public float minStartingHunger = 20f;
        public float maxStartingHunger = 100f;
        public float maxHunger = 300f;
        public float minHunger = -400f;
        public float damage = 2f;
        public float hungerGainRate = 5f;
        public float damageThreshold = 50f;
        public float damageTickRate = 5f;
        [Header("Hunting Settings")]
        public int hierarchialRank = 5;
    }

}