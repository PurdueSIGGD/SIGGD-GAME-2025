using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;


public class VillageDestructionExternalEvent : ExternalEventTriggerer
{
    public GameObject village;
    public GameObject destroyedVillage;
    public static readonly string VillageExplosion = "VillageExplosion";

    private void Start()
    {
        // Restore the destroyed village when loading a save where it has already happened
        SaveManager.Instance?.WhenGameplayReady(() =>
        {
            if (this == null) return;
            if (SaveManager.Instance.Get<PlayerModule>()?.VillageDestroyed == true)
            {
                SwapToDestroyedVillage();
            }
        });
    }

    public override void TriggerExternalEvent()
    {
        Debug.Log("External event triggered!");

        if (village != null && destroyedVillage != null)
        {
            var player = SaveManager.Instance?.Get<PlayerModule>();
            if (player != null && player.VillageDestroyed)
            {
                // already destroyed in this save, nothing left to blow up
                SwapToDestroyedVillage();
                return;
            }

            SwapToDestroyedVillage();

            // The village is far from where the player is when this fires, so play the explosion at the
            // player instead of at the village to make sure it is heard
            Vector3 soundPos = PlayerID.Instance != null ? PlayerID.Instance.transform.position : transform.position;
            AudioManager.Instance.PlayOneShotNoAsync(VillageExplosion, soundPos);

            //We don't actually have to kill the villagers we can just spawn in some corpses as part of the destroyed village prefab.
            //Instead just delete them

            if (player != null) player.VillageDestroyed = true;
            // Forced save (same as a scene exit): the quest that triggers this only fires once, so the
            // destruction must reach disk even if the player is being pursued right now
            SaveManager.Instance?.SaveGameplay(SaveTrigger.SceneExit);
        }
        else
        {
            Debug.LogWarning("Village or destroyed village GameObject is not assigned.");
        }
    }

    public override void RestoreExternalEvent()
    {
        SwapToDestroyedVillage();
    }

    private void SwapToDestroyedVillage()
    {
        if (village != null) village.SetActive(false);
        if (destroyedVillage != null) destroyedVillage.SetActive(true);
    }
}
