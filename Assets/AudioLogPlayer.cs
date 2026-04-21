using System;
using UnityEngine;

public class AudioLogPlayer : MonoBehaviour, IInteractable<IInteractor>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Action<ItemInfo, IInteractor> OnItemInteract;

    public ItemInfo itemInfo;
    public bool destroyAfterPickup;

    private bool interactable = true;
    private InteractableUI currentUi;

    public void OnHoverEnter(InteractableUI ui)
    {
        if (interactable)
        {
            ui.ActivateUI(this);
            currentUi = ui;
            Debug.Log($"Hovering over item: {itemInfo.itemName}");
        }
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        currentUi = null;
        Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor)
    {
        if (interactable)
        {
            Debug.Log($"Item {itemInfo.itemName} interacted by interactor.");

            Debug.Log($"Audio Strategy: {interactor}");

            Debug.Log(OnItemInteract == null ? "No AUDIO listeners" : "AUDIO Listeners found");
            OnItemInteract?.Invoke(itemInfo, interactor);

            interactable = false;
            if (currentUi) currentUi.DeactivateUI();
            if (destroyAfterPickup) Destroy(this.gameObject); // Remove the item from the scene
        }
    }
}
