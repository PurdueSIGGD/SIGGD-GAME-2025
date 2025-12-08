
using UnityEngine.SceneManagement;

public class MarkPrologueCompleted : ExternalEventTriggerer
{
    public override void TriggerExternalEvent()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.gameProgressModule.CompletePrologue();
            SaveManager.Instance.Save();
        }
        SceneManager.LoadScene("TestTerrainScene");
    }
}
