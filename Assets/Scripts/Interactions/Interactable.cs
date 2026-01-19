using System;
using UnityEngine;
public class Interactable : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemInteract;

    public ItemInfo itemInfo;

    private bool interactable = true;
    private InteractableUI currentUi;

    public void OnHoverEnter(InteractableUI ui)
    {
        if (interactable)
        {
            ui.ActivateUI(this);
            currentUi = ui;
            //Debug.Log($"Hovering over item: {itemInfo.itemName}");
        }
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        currentUi = null;
        //Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor, InteractableUI ui)
    {
        if (interactable)
        {
            Debug.Log($"Item {itemInfo.itemName} interacted up by interactor.");
            OnItemInteract?.Invoke(itemInfo, interactor);
            interactable = false;
            if (currentUi) currentUi.DeactivateUI();
        }
    }
}