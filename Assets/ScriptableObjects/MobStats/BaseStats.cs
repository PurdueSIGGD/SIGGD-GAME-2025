using UnityEngine;

namespace SIGGD.Goap.Config
{
    [CreateAssetMenu(fileName = "BaseStats", menuName = "Scriptable Objects/Mobs/Base Stats")]
    public class BaseStats : ScriptableObject
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
        public LayerMask playerLayer;
        [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float staminaGainRate = 0.15f;
    }
}