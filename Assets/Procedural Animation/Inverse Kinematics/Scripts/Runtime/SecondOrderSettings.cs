using UnityEngine;

namespace ProceduralAnimation.Runtime {
    [CreateAssetMenu(menuName = "Second Order Dynamics/Settings", fileName = "New Settings")]
    public class SecondOrderSettings : ScriptableObject {
        //  Set a minimum to not break math
        [Min(0.001f)] public float f = 5;
        [Min(0.001f)] public float z = 0.75f;
        public float r = 0;
    }
}