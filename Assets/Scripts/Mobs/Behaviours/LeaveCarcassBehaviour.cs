using System.Runtime.CompilerServices;
using UnityEngine;

public class LeaveCarcassBehaviour : MonoBehaviour
{
  
    private EntityHealthManager healthManager;
    [SerializeField]
    private GameObject carcassPrefab;
    private void Awake()
    {
        healthManager = GetComponent<EntityHealthManager>();
    }
    private void OnEnable()
    {
        healthManager.OnDeath += SpawnCarcass;
    }
    private void OnDisable()
    {
        healthManager.OnDeath -= SpawnCarcass;
    }
    private void SpawnCarcass()
    {
        Instantiate(carcassPrefab, transform.position, Quaternion.identity);
    }
}
