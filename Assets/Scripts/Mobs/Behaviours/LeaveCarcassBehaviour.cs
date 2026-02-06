using System.Runtime.CompilerServices;
using UnityEngine;

namespace SIGGD.Mobs
{
    public class LeaveCarcassBehaviour : MonoBehaviour
    {

        [SerializeField]
        private GameObject carcassPrefab;
        private void Awake()
        {
        }
        private void OnEnable()
        {
            EntityHealthManager.OnDeath += SpawnCarcass;
        }
        private void OnDisable()
        {
            EntityHealthManager.OnDeath -= SpawnCarcass;
        }
        private void SpawnCarcass(DamageContext context)
        {
            if (context.victim == gameObject)
            {
                Instantiate(carcassPrefab, transform.position, Quaternion.identity);
            }
        }
    }

}