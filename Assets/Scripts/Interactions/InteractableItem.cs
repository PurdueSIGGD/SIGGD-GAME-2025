using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable<IInteractor>
{
    public ItemInfo itemInfo;
    
    public void OnHoverEnter()
    {
        // Show item highlight or tooltip
        Debug.Log($"Hovering over item: {itemInfo.itemName}");
    }
    
    public void OnHoverExit()
    {
        // Hide item highlight or tooltip
        Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }
    
    public void OnInteract(IInteractor interactor)
    {
        interactor.Inventory.AddItem(itemInfo, 1);
        Debug.Log($"Item {itemInfo.itemName} picked up by interactor.");
        Destroy(this.gameObject); // Remove the item from the scene
    }
}