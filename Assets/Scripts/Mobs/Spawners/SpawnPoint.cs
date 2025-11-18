using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] GameObject mobPrefab;
    bool spawned = false;
    private GameObject thingSpawned = null;
    [SerializeField] float spawnChance = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !spawned && Random.value <= spawnChance)
        {
            thingSpawned = Instantiate(mobPrefab, transform.position, Quaternion.identity);
            Debug.Log("Mob Spawned at Spawn Point");
            thingSpawned.SetActive(true);
            spawned = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && spawned)
        {
            spawned = false;
            Destroy(thingSpawned);
            Debug.Log("Player Exited Spawn Point Area");
        }
    }
}
