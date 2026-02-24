using UnityEngine;

public class ShopKeeperInteract : MonoBehaviour, IInteractable<IInteractor>
{
    private InteractableUI ui;
    public void OnHoverEnter(InteractableUI ui)
    {
        if (CraftingMenu.Instance.IsCanvasActive())
        {
            return;
        }

        ui.ActivateUI(this);
        this.ui = ui;
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        this.ui = ui;
    }

    public void OnInteract(IInteractor interactor)
    {
        ui.DeactivateUI();
        CraftingMenu.Instance.ShowCraftingMenu(true);
    }
}
