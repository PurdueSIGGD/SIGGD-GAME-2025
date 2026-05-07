using System;
using UnityEngine;

public class AudioLogPlayer : MonoBehaviour, IInteractable<IInteractor>
{
    public AudioLogObject audioLog;
    public bool destroyAfterPickup;

    private bool interactable = true;
    private InteractableUI currentUi;

    public void OnHoverEnter(InteractableUI ui)
    {
        if (interactable)
        {
            ui.ActivateUI(this);
            currentUi = ui;
        }
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        currentUi = null;
    }

    public void OnInteract(IInteractor interactor)
    {
        Debug.Log("on interact happened");
        if (interactable)
        {
            Debug.Log("log should be playing");
            AudioLogManager.Instance.PlayAudioLog(audioLog.name, PlayerID.Instance?.gameObject);

            interactable = false;

            if (currentUi) currentUi.DeactivateUI();
            if (destroyAfterPickup) Destroy(this.gameObject); // Remove the item from the scene
        }
    }
}
