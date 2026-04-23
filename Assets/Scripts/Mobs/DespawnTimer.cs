using UnityEngine;

public class DespawnTimer : MonoBehaviour
{
    [SerializeField] float despawnTime = 5f;
    private void Update()
    {
        despawnTime -= Time.deltaTime;
        if (despawnTime <= 0)
            Destroy(gameObject);
    }
}
