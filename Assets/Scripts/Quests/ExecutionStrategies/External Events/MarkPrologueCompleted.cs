
using System.Collections;
using FMOD.Studio;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MarkPrologueCompleted : ExternalEventTriggerer
{
    public override void TriggerExternalEvent()
    {
        var save = SaveManager.Instance;
        if (save != null)
        {
            save.Get<GameProgressModule>()?.CompletePrologue();
            save.SaveSettings();
            // Also flush gameplay so leaving the prologue keeps the player's progress even if a
            // pursued state would normally block a gated save — this is a scene exit, req. 5.
            save.SaveGameplay(SaveTrigger.SceneExit);
        }

        StartCoroutine(PlayExitSequenceAndLoadScene());
    }

    private IEnumerator PlayExitSequenceAndLoadScene()
    {
        bool inputWasDisabled = false;

        if (PlayerInput.Instance != null)
        {
            PlayerInput.Instance.DebugToggleInput(true);
            inputWasDisabled = true;
        }

        if (SceneFader.Instance != null)
        {
            yield return SceneFader.Instance.FadeToBlack();
        }
        else
        {
            Debug.LogWarning("MarkPrologueCompleted: SceneFader instance is missing, skipping fade.");
        }

        yield return new WaitForSecondsRealtime(2f);
        yield return PlayCapsuleCrashSequenceSfx();

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneAndFadeFromBlack("NathanA0Scene");
        }
        else
        {
            SceneManager.LoadScene("NathanA0Scene");
        }

        if (inputWasDisabled && PlayerInput.Instance != null)
        {
            PlayerInput.Instance.DebugToggleInput(false);
        }
    }

    private IEnumerator PlayCapsuleCrashSequenceSfx()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("MarkPrologueCompleted: AudioManager instance is missing, skipping prologue exit SFX.");
            yield break;
        }

        Vector3 worldPos = PlayerID.Instance != null ? PlayerID.Instance.transform.position : Vector3.zero;
        EventInstance crashSequenceInstance = AudioManager.Instance.PlayOneShotStoppableNoAsync("EnterCapsuleandCrashSequence", worldPos);
        if (!crashSequenceInstance.isValid())
        {
            Debug.LogWarning("MarkPrologueCompleted: EnterCapsuleandCrashSequence event could not be created.");
            yield break;
        }

        PLAYBACK_STATE playbackState = PLAYBACK_STATE.STOPPED;
        do
        {
            crashSequenceInstance.getPlaybackState(out playbackState);
            yield return null;
        } while (playbackState != PLAYBACK_STATE.STOPPED);

        crashSequenceInstance.release();
    }
}
