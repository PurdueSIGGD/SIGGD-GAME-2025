using UnityEditor;
using UnityEngine;

public class SceneChangeCollider : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private Transform newPosition;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SaveManager.Instance.Save();   
            Debug.Log("Scene change collided");
            SceneFader.Instance.FadeToScene(targetSceneName);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1);
#if UNITY_EDITOR
        Handles.Label(transform.position + Vector3.up * 1.5f, "Scene Change Collider");
#endif
    }
}
