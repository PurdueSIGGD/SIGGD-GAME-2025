/*

using CrashKonijn.Goap.Runtime;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private GameObject agentPrefab;

    private void Awake()
    {
        this.goap = FindObjectOfType<GoapBehaviour>();
    }
    void Start()
    {
        var agent = Instantiate(this.agentPrefab, new Vector3(10,10,10), new Quaternion(10,10,10,10)).GetComponent<GoapActionProvider>();
        var brain = agent.GetComponent<GenericAgentBrain>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
*/