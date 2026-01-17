using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeExecutionStrategy : IQuestExecutionStrategy
{
    [SerializeField] public string sceneID;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        SceneManager.LoadScene(sceneID);
        Debug.Log($"Switching to {sceneID}");
    }

    public override string ToString()
    {
        return "Trigger Audio Log Event";
    }
}