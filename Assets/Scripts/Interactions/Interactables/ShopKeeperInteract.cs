using UnityEngine;

public class ShopKeeperInteract : MonoBehaviour, IInteractable<IInteractor>
{

    // For now the shopkeeper script is just opening the crafting menu since the shopkeeper
    // doesn't need to move (and I am unsure if it should also rotate to face the player or
    // if it stays completely stationary).
    public void OnHoverEnter(InteractableUI ui) { }
    public void OnHoverExit(InteractableUI ui) { }

    public void OnInteract(IInteractor interactor)
    {
        if (CraftingMenu.Instance)
            CraftingMenu.Instance.ShowCraftingMenu(true);
        else
            Debug.LogError("ShopKeeper: CraftingMenu instance not found!");
    }


}
