using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Behaviours;
using System.Drawing;
using UnityEngine;

public class HyenaSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject agentPrefab;
    Bounds bounds;
    public float spawningX;
    public float spawningZ;
    public float spawnCount;
    // different mob types?
    private void Awake()
    {
        bounds = new Bounds(gameObject.transform.position, new Vector3(spawningX, 10, spawningZ));
        this.agentPrefab.SetActive(false);
    }
    void Start()
    {
        for (int i = 0; i < spawnCount; i++) {

            var agent = Instantiate(this.agentPrefab, new Vector3(10, 2, 10), new Quaternion(10, 10, 10, 10)).GetComponent<GoapActionProvider>();
            agent.gameObject.SetActive(true);
        }
    }
    private Vector3 GetRandomPosition() {
        Vector2 randomPos = Random.insideUnitCircle * spawningX;
        return new Vector3(randomPos.x, 2, randomPos.y);
    }
}
