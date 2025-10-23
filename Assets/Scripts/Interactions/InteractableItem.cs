using System;
using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable<IInteractor>
{
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
    
    public void OnInteract(IInteractor interactor)
    {
        Debug.Log(interactor.Inventory.AddItem(itemInfo, 1));
        Debug.Log($"Item {itemInfo.itemName} picked up by interactor.");
        Destroy(this.gameObject); // Remove the item from the scene
    }
}