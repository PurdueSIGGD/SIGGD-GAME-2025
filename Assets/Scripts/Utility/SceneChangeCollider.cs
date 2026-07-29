using SIGGD.Save;
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
            // Forced scene-exit save (req. 5): captures inventory/quests/player/grave/scene
            // even if GameStateManager would normally block the save.
            SaveManager.Instance?.SaveGameplay(SaveTrigger.SceneExit);
            Debug.Log("Scene change collided");
            SceneFader.Instance.FadeToScene(targetSceneName, newPosition);
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
