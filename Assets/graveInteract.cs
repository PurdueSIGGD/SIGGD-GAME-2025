using UnityEngine;

public class graveInteract : MonoBehaviour, IInteractable<IInteractor>
{
    private UISlot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar

    public void OnHoverEnter(InteractableUI ui)
    {
        ui.ActivateUI(this);
        //Debug.Log($"Hovering over item: {itemInfo.itemName}");
    }
    
    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        //Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }
    
    public void OnInteract(IInteractor interactor)
    {
        Destroy(this.gameObject); // Remove the item from the scene

    }
    public void doSomething()
    {
        
    }
}
