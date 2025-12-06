using UnityEngine;


namespace SIGGD.Goap.Config
{
    [CreateAssetMenu(fileName = "HyenaStats", menuName = "Scriptable Objects/Mobs/Hyena Stats")]
    public class HyenaStats : BaseStats
    {
        [Header("Attack Stuff")]

        public float beginningAttackCooldown = 0f;
        public float circleSpeed = 10f;
    }
}