using System;
using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemPickUp;

    public ItemInfo itemInfo;


    public void OnHoverEnter(InteractableUI ui)
    {
        ui.ActivateUI(this);
        Debug.Log($"Hovering over item: {itemInfo.itemName}");
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor, InteractableUI ui)
    {
        int leftover = interactor.Inventory.AddItem(itemInfo, 1);
        if (leftover > 0)
        { // Don't do anything if there is no inventory space
            Debug.Log($"Out of inventory space. Unable to pick up {itemInfo.itemName}.");
            ui.ResetInteractUI();
        }
        else
        {
            Debug.Log($"Item {itemInfo.itemName} picked up by interactor.");
            OnItemPickUp?.Invoke(itemInfo, interactor);
            Destroy(this.gameObject); // Remove the item from the scene
        }
    }
}