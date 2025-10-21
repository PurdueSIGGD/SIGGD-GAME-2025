using UnityEngine;

namespace ProceduralAnimation.Runtime {
    [CreateAssetMenu(menuName = "Second Order Dynamics/Settings", fileName = "New Settings")]
    public class SecondOrderSettings : ScriptableObject {
        [Min(0)] public float f = 5;
        [Min(0)] public float z = 0.75f;
        public float r = 0;
    }
}