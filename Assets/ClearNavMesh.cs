using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class ClearNavMesh : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Force Cleanup NavMesh")]
    public static void ForceCleanupNavMesh()
    {
        if (Application.isPlaying)
            return;

        NavMesh.RemoveAllNavMeshData();
    }
#endif

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
