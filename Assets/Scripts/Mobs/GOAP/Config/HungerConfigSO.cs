using UnityEngine;

namespace SIGGD.Goap.Config
{
    [CreateAssetMenu(fileName = "HungerConfigSO", menuName = "Scriptable Objects/HungerConfigSO")]
    public class HungerConfigSO : ScriptableObject
    {
        public float hunger = 0f;
    }

}