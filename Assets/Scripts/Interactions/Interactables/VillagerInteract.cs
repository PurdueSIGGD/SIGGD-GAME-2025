using UnityEngine;

public class VillagerInteract : MonoBehaviour, IInteractable<IInteractor>
{
    InteractableUI ui;

    public void OnHoverEnter(InteractableUI ui) {
        if (CraftingMenu.Instance.IsCanvasActive())
        {
            return;
        }

        ui.ActivateUI(this);
        this.ui = ui;
    }

    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
        this.ui = ui;
    }

    public void OnInteract(IInteractor interactor)
    {
        // Check player has a flower selected
        if (Inventory.Instance.GetSelectedItem()?.itemName == ItemInfo.ItemName.Flower)
        {
            Inventory.Instance.Decrement();
            ItemInfo slimeball = RecipeInfo.Instance.NamesToItemInfos[ItemInfo.ItemName.Slimeball];
            Inventory.Instance.AddItem(slimeball, 1);
        }
    }
}
