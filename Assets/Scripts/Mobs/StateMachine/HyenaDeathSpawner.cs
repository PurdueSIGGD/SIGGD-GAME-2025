using UnityEngine;

namespace SIGGD.Mobs.StateMachine
{
    public class HyenaDeathSpawner : MonoBehaviour
    {
        [SerializeField] private float destroyDelay = 5f;
        
        private void Awake()
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}