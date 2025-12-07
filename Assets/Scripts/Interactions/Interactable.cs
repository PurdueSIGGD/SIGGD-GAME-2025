using System;
using UnityEngine;
public class Interactable : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemInteract;

    public ItemInfo itemInfo;

    private bool interactable = true;

    public void OnHoverEnter(InteractableUI ui)
    {
        if (interactable)
        {
            ui.ActivateUI(this);
            Debug.Log($"Hovering over item: {itemInfo.itemName}");
        }
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor)
    {
        if (interactable)
        {
            Debug.Log($"Item {itemInfo.itemName} interactable up by interactor.");
            OnItemInteract?.Invoke(itemInfo, interactor);
            interactable = false;
        }
    }
}