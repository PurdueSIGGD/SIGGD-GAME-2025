using UnityEngine;

public class SceneSaveManager : Singleton<SceneSaveManager>
{
    public SceneDataSaveModule sceneModule = null;
    public bool saveScene = true;

    public string sceneName;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (saveScene) sceneModule = new SceneDataSaveModule();
        Load();
    }

    public void Load()
    {
        sceneModule.deserialize();
    }

    public void Save() {
        sceneModule.serialize();
    }
}
